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
        private readonly string DoNetVersionString = ResourceService.AppInformationResource.GetString("DoNetVersion");
        private readonly string MediaInfoVersionString = ResourceService.AppInformationResource.GetString("MediaInfoVersion");
        private readonly string WindowsAppSDKVersionString = ResourceService.AppInformationResource.GetString("WindowsAppSDKVersion");
        private readonly string WinUIVersionString = ResourceService.AppInformationResource.GetString("WinUIVersion");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly OverlappedPresenter overlappedPresenter;
        private readonly SUBCLASSPROC appInformationWindowSubClassProc;
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
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(480 * dpi);
            int height = Convert.ToInt32(280 * dpi);
            User32Library.GetWindowRect((nint)mainWindow.AppWindow.Id.Value, out RECT parentRect);
            int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
            int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
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
            appInformationWindowSubClassProc = new SUBCLASSPROC(AppInformationWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, appInformationWindowSubClassProc, 0, 0);

            SetWindowTheme();
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

            Close();
            await MainWindow.ShowNotificationAsync(new CopyPasteNotificationTip(copyResult));
        }

        /// <summary>
        /// 关闭
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            Close();
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
        /// 应用信息窗口消息处理
        /// </summary>
        private nint AppInformationWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                        int width = Convert.ToInt32(480 * dpi);
                        int height = Convert.ToInt32(280 * dpi);
                        User32Library.GetWindowRect((nint)MainWindow.AppWindow.Id.Value, out RECT parentRect);
                        int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
                        int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
                        User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                        int width = Convert.ToInt32(480 * dpi);
                        int height = Convert.ToInt32(280 * dpi);
                        User32Library.GetWindowRect((nint)MainWindow.AppWindow.Id.Value, out RECT parentRect);
                        int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
                        int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
                        User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
                        break;
                    }
                // 窗口销毁后触发的消息
                case WindowMessage.WM_DESTROY:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, appInformationWindowSubClassProc, 0);
                        taskCompletionSource.TrySetResult(ContentDialogResult.None);
                        MainWindow.Activate();
                        MainWindow = null;
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
                        double dpi = Convert.ToDouble(wParam) / 96;
                        int width = Convert.ToInt32(480 * dpi);
                        int height = Convert.ToInt32(280 * dpi);
                        User32Library.GetWindowRect((nint)MainWindow.AppWindow.Id.Value, out RECT parentRect);
                        int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
                        int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
                        User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
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
    }
}
