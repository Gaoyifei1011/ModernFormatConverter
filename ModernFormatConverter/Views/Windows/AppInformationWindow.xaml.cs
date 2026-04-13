using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.Dwmapi;
using ModernFormatConverter.WindowsAPI.PInvoke.KernelAppCore;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 应用信息窗口
    /// </summary>
    public sealed partial class AppInformationWindow : Window, INotifyPropertyChanged
    {
        private readonly string TitleString = ResourceService.AppInformationResource.GetString("Title");
        private readonly string DoNetVersionString = ResourceService.AppInformationResource.GetString("DoNetVersion");
        private readonly string MediaInfoVersionString = ResourceService.AppInformationResource.GetString("MediaInfoVersion");
        private readonly string WindowsAppSDKVersionString = ResourceService.AppInformationResource.GetString("WindowsAppSDKVersion");
        private readonly string WinUIVersionString = ResourceService.AppInformationResource.GetString("WinUIVersion");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly OverlappedPresenter overlappedPresenter;
        private readonly SUBCLASSPROC conversionToolsWindowSubClassProc;
        private readonly ContentIsland contentIsland;
        private readonly InputKeyboardSource inputKeyboardSource;
        private readonly InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;

        private string _windowTitle;

        public string WindowTitle
        {
            get { return _windowTitle; }

            set
            {
                if (!string.Equals(_windowTitle, value))
                {
                    _windowTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
                }
            }
        }

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

        private bool _isLoadCompleted = false;

        public bool IsLoadCompleted
        {
            get { return _isLoadCompleted; }

            set
            {
                if (!Equals(_isLoadCompleted, value))
                {
                    _isLoadCompleted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadCompleted)));
                }
            }
        }

        private WinRTObservableCollection<DictionaryEntry> AppInformationCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AppInformationWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            WindowTitle = TitleString;
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
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(480 * dpi);
            int height = Convert.ToInt32(280 * dpi);
            User32Library.GetWindowRect((nint)mainWindow.AppWindow.Id.Value, out RECT parentRect);
            int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
            int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
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

            // 为应用主窗口添加窗口过程
            conversionToolsWindowSubClassProc = new SUBCLASSPROC(ConversionToolsWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, conversionToolsWindowSubClassProc, 0, 0);

            SetWindowTheme();
            AppWindow.Closing += (sender, args) =>
            {
                mainWindow.Activate();
            };
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

        #region 第二部分：窗口右键菜单事件

        /// <summary>
        /// 窗口移动
        /// </summary>
        private void OnMoveClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is MenuFlyout menuFlyout)
            {
                menuFlyout.Hide();
                User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_MOVE, 0);
            }
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        private void OnSizeClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is MenuFlyout menuFlyout)
            {
                menuFlyout.Hide();
                User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_SIZE, 0);
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_CLOSE, 0);
        }

        #endregion 第二部分：窗口右键菜单事件

        #region 第三部分：窗口内容挂载的事件

        /// <summary>
        /// 应用主题变化时设置标题栏按钮的颜色
        /// </summary>
        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetClassicMenuTheme(sender.ActualTheme);
        }

        #endregion 第三部分：窗口内容挂载的事件

        #region 第四部分：内容挂载的事件

        /// <summary>
        /// 应用信息初始化触发的事件
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            List<KeyValuePair<string, Version>> dependencyInformationList = [];
            await Task.Run(() =>
            {
                uint bufferLength = 0;

                KernelAppCoreLibrary.GetCurrentPackageInfo(PACKAGE_FLAGS.PACKAGE_PROPERTY_STATIC, ref bufferLength, null, out uint count);

                if (count > 0)
                {
                    List<PACKAGE_INFO> packageInfoList = [];
                    byte[] buffer = new byte[bufferLength];
                    KernelAppCoreLibrary.GetCurrentPackageInfo(PACKAGE_FLAGS.PACKAGE_PROPERTY_STATIC, ref bufferLength, buffer, out count);

                    for (int index = 0; index < count; index++)
                    {
                        int packageInfoSize = Marshal.SizeOf<PACKAGE_INFO>();
                        nint packageInfoPtr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, index * packageInfoSize);
                        Marshal.Copy(buffer, index * packageInfoSize, packageInfoPtr, packageInfoSize);
                        PACKAGE_INFO packageInfo = Marshal.PtrToStructure<PACKAGE_INFO>(packageInfoPtr);
                        packageInfoList.Add(packageInfo);
                    }

                    foreach (PACKAGE_INFO packageInfo in packageInfoList)
                    {
                        // WinUI 3 版本信息
                        if (packageInfo.packageFullName.Contains("Microsoft.WindowsAppRuntime"))
                        {
                            dependencyInformationList.Add(new KeyValuePair<string, Version>(WindowsAppSDKVersionString, new Version(packageInfo.packageId.version.Parts.Major, packageInfo.packageId.version.Parts.Minor, packageInfo.packageId.version.Parts.Build, packageInfo.packageId.version.Parts.Revision)));

                            FileVersionInfo winUI3File = FileVersionInfo.GetVersionInfo(Path.Combine(packageInfo.path, "Microsoft.UI.Xaml.Controls.dll"));
                            dependencyInformationList.Add(new KeyValuePair<string, Version>(WinUIVersionString, new Version(winUI3File.FileMajorPart, winUI3File.FileMinorPart, winUI3File.FileBuildPart, winUI3File.FilePrivatePart)));
                            break;
                        }
                    }

                    // .NET 版本信息
                    dependencyInformationList.Add(new KeyValuePair<string, Version>(DoNetVersionString, new Version(RuntimeInformation.FrameworkDescription.Remove(0, 15))));

                    // MediaInfo 版本信息
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MediaInfo.dll"));
                    dependencyInformationList.Add(new KeyValuePair<string, Version>(MediaInfoVersionString, new Version(fileVersionInfo.FileVersion)));
                }
            });

            foreach (KeyValuePair<string, Version> dependencyInformation in dependencyInformationList)
            {
                AppInformationCollection.Add(new DictionaryEntry(dependencyInformation.Key, dependencyInformation.Value));
            }

            IsLoadCompleted = true;
        }

        /// <summary>
        /// 复制应用信息
        /// </summary>
        private async void OnCopyAppInformationClicked(object sender, RoutedEventArgs args)
        {
            bool copyResult = false;

            try
            {
                StringBuilder stringBuilder = await Task.Run(() =>
                {
                    StringBuilder stringBuilder = new();
                    foreach (DictionaryEntry appInformationItem in AppInformationCollection)
                    {
                        stringBuilder.Append(appInformationItem.Key);
                        stringBuilder.Append(appInformationItem.Value);
                        stringBuilder.Append(Environment.NewLine);
                    }

                    return stringBuilder;
                });

                copyResult = CopyPasteHelper.CopyToClipboard(Convert.ToString(stringBuilder));
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AppInformationWindow), nameof(OnCopyAppInformationClicked), 1, e);
            }

            CloseWindow();
            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(copyResult));
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void OnCloseWindowClicked(object sender, RoutedEventArgs args)
        {
            CloseWindow();
        }

        #endregion 第四部分：内容挂载的事件

        #region 第五部分：自定义事件

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

        #endregion 第五部分：自定义事件

        #region 第六部分：窗口及内容属性设置

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

        #endregion 第六部分：窗口及内容属性设置

        #region 第七部分：窗口过程

        /// <summary>
        /// 应用窗口消息处理
        /// </summary>
        private nint ConversionToolsWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            if (TitlebarMenuFlyout.IsOpen)
                            {
                                TitlebarMenuFlyout.Hide();
                            }
                        }, null);
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            if (TitlebarMenuFlyout.IsOpen)
                            {
                                TitlebarMenuFlyout.Hide();
                            }

                            if (AppInformationPage.IsLoaded)
                            {
                                double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                                overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(480 * dpi);
                                overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(280 * dpi);
                            }
                        }, null);
                        break;
                    }
                // 窗口关闭时触发的消息
                case WindowMessage.WM_CLOSE:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, conversionToolsWindowSubClassProc, 0);
                        break;
                    }
                // 当用户按下鼠标左键时，光标位于窗口的非工作区内的消息
                case WindowMessage.WM_NCLBUTTONDOWN:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }
                        break;
                    }
                // 当用户按下鼠标右键并释放时，光标位于窗口的非工作区内的消息
                case WindowMessage.WM_NCRBUTTONUP:
                    {
                        if (wParam is 2 && Content is not null && Content.XamlRoot is not null)
                        {
                            System.Drawing.Point cursorPos = new((int)LOWORD((uint)lParam), (int)HIWORD((uint)lParam));
                            User32Library.MapWindowPoints(0, hWnd, ref cursorPos, 2); ;
                            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;

                            FlyoutShowOptions options = new()
                            {
                                ShowMode = FlyoutShowMode.Standard,
                                Position = Environment.OSVersion.Version.Build > 22000 ? new Point(cursorPos.X / dpi, cursorPos.Y / dpi) : new Point(cursorPos.X, cursorPos.Y)
                            };

                            TitlebarMenuFlyout.ShowAt(Content, options);
                        }
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
                        overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(480 * Convert.ToDouble(wParam) / 96);
                        overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(280 * Convert.ToDouble(wParam) / 96);
                        break;
                    }
                // 选择窗口右键菜单的条目时接收到的消息
                case WindowMessage.WM_SYSCOMMAND:
                    {
                        SYSTEMCOMMAND sysCommand = (SYSTEMCOMMAND)(wParam & 0xFFF0);

                        if (sysCommand is SYSTEMCOMMAND.SC_MOUSEMENU)
                        {
                            FlyoutShowOptions options = new()
                            {
                                Position = new Point(0, 15),
                                ShowMode = FlyoutShowMode.Standard
                            };
                            TitlebarMenuFlyout.ShowAt(null, options);
                            return 0;
                        }
                        else if (sysCommand is SYSTEMCOMMAND.SC_KEYMENU)
                        {
                            if (lParam is (int)System.Windows.Forms.Keys.Space)
                            {
                                FlyoutShowOptions options = new()
                                {
                                    Position = new Point(0, 45),
                                    ShowMode = FlyoutShowMode.Standard
                                };
                                TitlebarMenuFlyout.ShowAt(null, options);
                                return 0;
                            }
                        }
                        break;
                    }
            }
            return Comctl32Library.DefSubclassProc(hWnd, Msg, wParam, lParam);
        }

        #endregion 第七部分：窗口过程

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

        private uint HIWORD(uint dword)
        {
            return (dword >> 16) & 0xffff;
        }

        private uint LOWORD(uint dword)
        {
            return dword & 0xffff;
        }
    }
}
