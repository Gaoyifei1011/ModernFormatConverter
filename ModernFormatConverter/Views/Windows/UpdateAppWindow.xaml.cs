using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.Dwmapi;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.System;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 更新应用窗口
    /// </summary>
    public sealed partial class UpdateAppWindow : Window, INotifyPropertyChanged
    {
        private readonly string CancelString = ResourceService.UpdateAppResource.GetString("Cancel");
        private readonly string CloseString = ResourceService.UpdateAppResource.GetString("Close");
        private readonly string CloseAppString = ResourceService.UpdateAppResource.GetString("CloseApp");
        private readonly string UpdateString = ResourceService.UpdateAppResource.GetString("Update");
        private readonly string UpdateDownloadingString = ResourceService.UpdateAppResource.GetString("UpdateDownloading");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private Progress<StorePackageUpdateStatus> storePackageUpdateProgress = null;
        private CancellationTokenSource cancellationTokenSource = null;
        private readonly OverlappedPresenter overlappedPresenter;
        private readonly SUBCLASSPROC licenseWindowSubClassProc;
        private readonly ContentIsland contentIsland;
        private readonly InputKeyboardSource inputKeyboardSource;
        private readonly InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;

        private MainWindow MainWindow { get; set; }

        private ElementTheme _windowTheme;

        public ElementTheme WindowTheme
        {
            get { return _windowTheme; }

            set
            {
                if (!Equals(_windowTheme, value))
                {
                    _windowTheme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTheme)));
                }
            }
        }

        private UpdateAppResultKind _updateAppResultKind = UpdateAppResultKind.Initialize;

        public UpdateAppResultKind UpdateAppResultKind
        {
            get { return _updateAppResultKind; }

            set
            {
                if (!Equals(_updateAppResultKind, value))
                {
                    _updateAppResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateAppResultKind)));
                }
            }
        }

        private string _primaryText;

        public string PrimaryText
        {
            get { return _primaryText; }

            set
            {
                if (!string.Equals(_primaryText, value))
                {
                    _primaryText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrimaryText)));
                }
            }
        }

        private string _closeText;

        public string CloseText
        {
            get { return _closeText; }

            set
            {
                if (!string.Equals(_closeText, value))
                {
                    _closeText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloseText)));
                }
            }
        }

        private string _updateDownloadString;

        public string UpdateDownloadString
        {
            get { return _updateDownloadString; }

            set
            {
                if (!string.Equals(_updateDownloadString, value))
                {
                    _updateDownloadString = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateDownloadString)));
                }
            }
        }

        private bool _isCancelingUpdate;

        public bool IsCancelingUpdate
        {
            get { return _isCancelingUpdate; }

            set
            {
                if (!Equals(_isCancelingUpdate, value))
                {
                    _isCancelingUpdate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCancelingUpdate)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public UpdateAppWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
            if (IntPtr.Size is 8)
            {
                User32Library.SetWindowLongPtr((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, mainWindow.AppWindow.Id.Value);
            }
            else
            {
                User32Library.SetWindowLong((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, mainWindow.AppWindow.Id.Value);
            }
            overlappedPresenter = AppWindow.Presenter as OverlappedPresenter;
            overlappedPresenter.IsMaximizable = false;
            overlappedPresenter.IsMinimizable = false;
            overlappedPresenter.IsResizable = false;
            overlappedPresenter.IsModal = true;
            overlappedPresenter.SetBorderAndTitleBar(true, false);
            ResizeWindow(UpdateAppResultKind);
            int cornerPreference = 2;
            DwmapiLibrary.DwmSetWindowAttribute((nint)AppWindow.Id.Value, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, Marshal.SizeOf<int>());
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);

            // 挂载相应的事件
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            licenseWindowSubClassProc = new SUBCLASSPROC(LincenseWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, licenseWindowSubClassProc, 0, 0);

            SetWindowTheme();
            AppWindow.Closing += (sender, args) =>
            {
                MainWindow.Activate();
                MainWindow = null;
            };
            PrimaryText = UpdateString;
            CloseText = CloseString;
        }

        #region 第一部分：窗口辅助类挂载的事件

        /// <summary>
        /// 处理键盘系统按键事件
        /// </summary>
        private async void OnSystemKeyDown(InputKeyboardSource sender, KeyEventArgs args)
        {
            if (args.VirtualKey is VirtualKey.F10 && Content is not null && Content.XamlRoot is not null)
            {
                await Task.Delay(50);
                SetPopupControlTheme(WindowTheme);
            }
        }

        /// <summary>
        /// 处理鼠标事件
        /// </summary>
        private async void OnPointerReleased(InputPointerSource sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonReleased && Content is not null && Content.XamlRoot is not null)
            {
                await Task.Delay(50);
                SetPopupControlTheme(WindowTheme);
            }
        }

        #endregion 第一部分：窗口辅助类挂载的事件

        #region 第二部分：窗口内容挂载的事件

        /// <summary>
        /// 应用主题变化时设置标题栏按钮的颜色
        /// </summary>
        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetClassicMenuTheme(sender.ActualTheme);
        }

        #endregion 第二部分：窗口内容挂载的事件

        #region 第三部分：内容挂载的事件

        /// <summary>
        /// 加载完成后触发的事件
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            SetPopupControlTheme(WindowTheme);
        }

        /// <summary>
        /// 更新应用
        /// </summary>
        private async void OnUpdateClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (UpdateAppResultKind is UpdateAppResultKind.Successfully)
                {
                    (Application.Current as MainApp).Dispose();
                }
                else
                {
                    UpdateAppResultKind = UpdateAppResultKind.Pending;
                    ResizeWindow(UpdateAppResultKind);
                    UpdateDownloadString = string.Format(UpdateDownloadingString, VolumeSizeHelper.ConvertVolumeSizeToString(0), VolumeSizeHelper.ConvertVolumeSizeToString(0));
                    CloseText = CancelString;
                    if (cancellationTokenSource is null)
                    {
                        StoreContext storeContext = StoreContext.GetDefault();
                        IReadOnlyList<StorePackageUpdate> storePackageUpdateList = await storeContext.GetAppAndOptionalStorePackageUpdatesAsync();
                        cancellationTokenSource = new();
                        bool updateFailed = false;
                        storePackageUpdateProgress = new();
                        storePackageUpdateProgress.ProgressChanged += (sender, progress) =>
                        {
                            if (progress.PackageUpdateState is StorePackageUpdateState.Pending)
                            {
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Pending;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CancelString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.Downloading)
                            {
                                synchronizationContext.Post((_) =>
                                {
                                    string downloadedSize = VolumeSizeHelper.ConvertVolumeSizeToString(progress.PackageDownloadSizeInBytes);
                                    string totalSize = VolumeSizeHelper.ConvertVolumeSizeToString(progress.PackageBytesDownloaded);
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Downloading;
                                        ResizeWindow(UpdateAppResultKind);
                                        UpdateDownloadString = string.Format(UpdateDownloadingString, downloadedSize, totalSize);
                                        CloseText = CancelString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.Deploying)
                            {
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Deploying;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CancelString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.Canceled)
                            {
                                updateFailed = true;
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Canceled;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CloseString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.OtherError)
                            {
                                updateFailed = true;
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Failed;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CloseString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.ErrorLowBattery)
                            {
                                updateFailed = true;
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Failed;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CloseString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.ErrorWiFiRecommended)
                            {
                                updateFailed = true;
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Failed;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CloseString;
                                    }
                                }, null);
                            }
                            else if (progress.PackageUpdateState is StorePackageUpdateState.ErrorWiFiRequired)
                            {
                                updateFailed = true;
                                synchronizationContext.Post((_) =>
                                {
                                    if (UpdateAppResultKind is not UpdateAppResultKind.Canceling)
                                    {
                                        UpdateAppResultKind = UpdateAppResultKind.Failed;
                                        ResizeWindow(UpdateAppResultKind);
                                        CloseText = CloseString;
                                    }
                                }, null);
                            }
                        };
                        StorePackageUpdateResult storePackageUpdateResult = await storeContext.TrySilentDownloadAndInstallStorePackageUpdatesAsync(storePackageUpdateList).AsTask(cancellationTokenSource.Token, storePackageUpdateProgress);
                        cancellationTokenSource.Dispose();
                        cancellationTokenSource = null;
                        CloseText = CloseString;
                        if (storePackageUpdateResult.OverallState is StorePackageUpdateState.Completed)
                        {
                            if (updateFailed)
                            {
                                UpdateAppResultKind = UpdateAppResultKind.Failed;
                                ResizeWindow(UpdateAppResultKind);
                            }
                            else
                            {
                                UpdateAppResultKind = UpdateAppResultKind.Successfully;
                                ResizeWindow(UpdateAppResultKind);
                                PrimaryText = CloseAppString;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                UpdateAppResultKind = UpdateAppResultKind.Canceled;
                ResizeWindow(UpdateAppResultKind);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                CloseText = CloseString;
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(UpdateAppWindow), nameof(OnUpdateClicked), 1, e);
            }
            catch (Exception e)
            {
                UpdateAppResultKind = UpdateAppResultKind.Failed;
                ResizeWindow(UpdateAppResultKind);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                CloseText = CloseString;
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(UpdateAppWindow), nameof(OnUpdateClicked), 2, e);
            }
        }

        /// <summary>
        /// 取消更新或关闭更新窗口
        /// </summary>
        private void OnCancelOrCloseClicked(object sender, RoutedEventArgs args)
        {
            if (UpdateAppResultKind is UpdateAppResultKind.Pending || UpdateAppResultKind is UpdateAppResultKind.Downloading || UpdateAppResultKind is UpdateAppResultKind.Deploying)
            {
                if (cancellationTokenSource is not null)
                {
                    try
                    {
                        cancellationTokenSource.Cancel();
                        ResizeWindow(UpdateAppResultKind);
                        UpdateAppResultKind = UpdateAppResultKind.Canceling;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(UpdateAppWindow), nameof(OnCancelOrCloseClicked), 1, e);
                    }
                }
                else
                {
                    UpdateAppResultKind = UpdateAppResultKind.Canceled;
                    ResizeWindow(UpdateAppResultKind);
                }
            }
            else
            {
                CloseWindow();
            }
        }

        #endregion 第三部分：内容挂载的事件

        #region 第四部分：自定义事件

        /// <summary>
        /// 设置选项发生变化时触发的事件
        /// </summary>
        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                if (string.Equals(args.PropertyName, nameof(ThemeService.AppTheme)))
                {
                    SetWindowTheme();
                }
            }, null);
        }

        #endregion 第四部分：自定义事件

        #region 第五部分：窗口及内容属性设置

        /// <summary>
        /// 设置应用显示的主题
        /// </summary>
        public void SetWindowTheme()
        {
            WindowTheme = string.Equals(ThemeService.AppTheme, ThemeService.ThemeList[0]) ? Application.Current.RequestedTheme is ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark : Enum.TryParse(ThemeService.AppTheme, out ElementTheme elementTheme) ? elementTheme : ElementTheme.Default;
        }

        /// <summary>
        /// 设置传统菜单标题栏按钮的主题色
        /// </summary>
        private void SetClassicMenuTheme(ElementTheme theme)
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;

            if (theme is ElementTheme.Light)
            {
                titleBar.PreferredTheme = TitleBarTheme.Light;
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceLight);
            }
            else
            {
                titleBar.PreferredTheme = TitleBarTheme.Dark;
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceDark);
            }

            UxthemeLibrary.FlushMenuThemes();
        }

        /// <summary>
        /// 设置所有弹出控件主题
        /// </summary>
        private void SetPopupControlTheme(ElementTheme elementTheme)
        {
            foreach (Popup popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(Content.XamlRoot))
            {
                popup.RequestedTheme = elementTheme;

                if (popup.Child is FlyoutPresenter flyoutPresenter)
                {
                    flyoutPresenter.RequestedTheme = elementTheme;
                }

                if (popup.Child is Grid grid && grid.Name is "OuterOverflowContentRootV2")
                {
                    grid.RequestedTheme = elementTheme;
                }
            }
        }

        #endregion 第五部分：窗口及内容属性设置

        #region 第六部分：窗口过程

        /// <summary>
        /// 应用窗口消息处理
        /// </summary>
        private nint LincenseWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        ResizeWindow(UpdateAppResultKind);
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        ResizeWindow(UpdateAppResultKind);
                        break;
                    }
                // 窗口关闭时触发的消息
                case WindowMessage.WM_CLOSE:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, licenseWindowSubClassProc, 0);
                        synchronizationContext.Post((_) =>
                        {
                            if (cancellationTokenSource is not null && (UpdateAppResultKind is UpdateAppResultKind.Pending || UpdateAppResultKind is UpdateAppResultKind.Downloading || UpdateAppResultKind is UpdateAppResultKind.Deploying))
                            {
                                try
                                {
                                    cancellationTokenSource.Cancel();
                                    UpdateAppResultKind = UpdateAppResultKind.Canceling;
                                }
                                catch (Exception e)
                                {
                                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(UpdateAppWindow), nameof(OnCancelOrCloseClicked), 1, e);
                                }
                            }
                        }, null);
                        break;
                    }
                // 当用户按下鼠标右键并释放时，光标位于窗口的非工作区内的消息
                case WindowMessage.WM_NCRBUTTONUP:
                    {
                        return 0;
                    }
                // 应用主题设置跟随系统发生变化时，当系统主题设置发生变化时修改修改应用背景色
                case WindowMessage.WM_SETTINGCHANGE:
                    {
                        SetWindowTheme();
                        SetClassicMenuTheme(WindowTheme);

                        synchronizationContext.Post((_) =>
                        {
                            SetPopupControlTheme(WindowTheme);
                        }, null);
                        break;
                    }
                // 窗口 DPI 发生变化后触发的消息
                case WindowMessage.WM_DPICHANGED:
                    {
                        ResizeWindow(UpdateAppResultKind);
                        break;
                    }
                // 选择窗口右键菜单的条目时接收到的消息
                case WindowMessage.WM_SYSCOMMAND:
                    {
                        SYSTEMCOMMAND sysCommand = (SYSTEMCOMMAND)(wParam & 0xFFF0);

                        if (sysCommand is SYSTEMCOMMAND.SC_MOUSEMENU)
                        {
                            return 0;
                        }
                        else if (sysCommand is SYSTEMCOMMAND.SC_KEYMENU)
                        {
                            if (lParam is (int)System.Windows.Forms.Keys.Space)
                            {
                                return 0;
                            }
                        }
                        break;
                    }
            }
            return Comctl32Library.DefSubclassProc(hWnd, Msg, wParam, lParam);
        }

        #endregion 第六部分：窗口过程

        /// <summary>
        /// 显示模态窗口
        /// </summary>
        public async Task<ContentDialogResult> ShowAsync()
        {
            taskCompletionSource = new();
            AppWindow.Show();
            return await taskCompletionSource.Task;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseWindow()
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_CLOSE, 0, 0);
        }

        /// <summary>
        /// 调整窗口大小
        /// </summary>
        private void ResizeWindow(UpdateAppResultKind updateAppResultKind)
        {
            synchronizationContext.Post((_) =>
            {
                double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                int width = Convert.ToInt32(550 * dpi);
                int height = Convert.ToInt32(updateAppResultKind is UpdateAppResultKind.Failed ? 260 : 230 * dpi);
                User32Library.GetWindowRect((nint)MainWindow.AppWindow.Id.Value, out RECT parentRect);
                int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
                int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
                User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOZORDER);
            }, null);
        }

        /// <summary>
        /// 检查更新应用状态
        /// </summary>
        private Visibility CheckUpdateAppResultKind(UpdateAppResultKind updateAppResultKind, UpdateAppResultKind comparedUpdateAppResultKind)
        {
            return Equals(updateAppResultKind, comparedUpdateAppResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查应用是否正在更新中
        /// </summary>
        private bool GetIsNotUpdating(UpdateAppResultKind updateAppResultKind)
        {
            return !(updateAppResultKind is UpdateAppResultKind.Pending || UpdateAppResultKind is UpdateAppResultKind.Downloading || UpdateAppResultKind is UpdateAppResultKind.Canceling || UpdateAppResultKind is UpdateAppResultKind.Deploying);
        }

        /// <summary>
        /// 检查应用是否正在更新中
        /// </summary>
        private Visibility GetUpdateProgressState(UpdateAppResultKind updateAppResultKind)
        {
            return (updateAppResultKind is UpdateAppResultKind.Pending || UpdateAppResultKind is UpdateAppResultKind.Downloading || UpdateAppResultKind is UpdateAppResultKind.Canceling || UpdateAppResultKind is UpdateAppResultKind.Deploying) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查应用是否正在取消更新中
        /// </summary>
        private bool GetIsNotCanceling(UpdateAppResultKind updateAppResultKind)
        {
            return updateAppResultKind is not UpdateAppResultKind.Canceling;
        }
    }
}
