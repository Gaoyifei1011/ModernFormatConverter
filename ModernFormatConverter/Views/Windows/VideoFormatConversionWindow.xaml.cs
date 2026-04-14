using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ModernFormatConverter.Extensions.Backdrop;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Dialogs
{
    /// <summary>
    /// 视频格式转换窗口
    /// </summary>
    public sealed partial class VideoFormatConversionWindow : Window, INotifyPropertyChanged
    {
        private readonly string CopyString = ResourceService.VideoFormatConversionResource.GetString("Copy");
        private readonly string CustomString = ResourceService.VideoFormatConversionResource.GetString("Custom");
        private readonly string DefaultString = ResourceService.VideoFormatConversionResource.GetString("Default");
        private readonly string DefaultSizeString = ResourceService.VideoFormatConversionResource.GetString("DefaultSize");
        private readonly string NoneString = ResourceService.VideoFormatConversionResource.GetString("None");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC videoFormatConversionWindowSubClassProc;
        private ContentIsland contentIsland;
        private InputKeyboardSource inputKeyboardSource;
        private InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;

        private ConversionToolsWindow ConversionToolsWindow { get; set; }

        private SystemBackdrop _windowSystemBackdrop;

        public SystemBackdrop WindowSystemBackdrop
        {
            get { return _windowSystemBackdrop; }

            set
            {
                _windowSystemBackdrop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowSystemBackdrop)));
            }
        }

        private ElementTheme _windowTheme;

        public ElementTheme WindowTheme
        {
            get { return _windowTheme; }

            set
            {
                _windowTheme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTheme)));
            }
        }

        private SelectorBarItem _selectedItem;

        public SelectorBarItem SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                _selectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        private KeyValuePair<string, string> _selectedFormatConversionType;

        public KeyValuePair<string, string> SelectedFormatConversionType
        {
            get { return _selectedFormatConversionType; }

            set
            {
                _selectedFormatConversionType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormatConversionType)));
            }
        }

        private KeyValuePair<string, string> _selectedSizeLimitation;

        public KeyValuePair<string, string> SelectedSizeLimitation
        {
            get { return _selectedSizeLimitation; }

            set
            {
                _selectedSizeLimitation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSizeLimitation)));
            }
        }

        private KeyValuePair<string, string> _selectedVideoEncoding;

        public KeyValuePair<string, string> SelectedVideoEncoding
        {
            get { return _selectedVideoEncoding; }

            set
            {
                _selectedVideoEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoEncoding)));
            }
        }

        private KeyValuePair<string, string> _selectedScreenSize;

        public KeyValuePair<string, string> SelectedScreenSize
        {
            get { return _selectedScreenSize; }

            set
            {
                _selectedScreenSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedScreenSize)));
            }
        }

        private int _screenWidth;

        public int ScreenWidth
        {
            get { return _screenWidth; }

            set
            {
                _screenWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenWidth)));
            }
        }

        private int _screenHeight;

        public int ScreenHeight
        {
            get { return _screenHeight; }

            set
            {
                _screenHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenHeight)));
            }
        }

        private KeyValuePair<string, string> _selectedBitRate;

        public KeyValuePair<string, string> SelectedBitRate
        {
            get { return _selectedBitRate; }

            set
            {
                _selectedBitRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBitRate)));
            }
        }

        public List<KeyValuePair<string, string>> FormatConversionTypeList { get; } =
        [
            new KeyValuePair<string, string>("MP4", ".mp4"),
            new KeyValuePair<string, string>("MKV", ".mkv"),
            new KeyValuePair<string, string>("GIF", ".gif"),
            new KeyValuePair<string, string>("WebM", ".webm"),
            new KeyValuePair<string, string>("AVI", ".avi"),
            new KeyValuePair<string, string>("FLV", ".flv"),
            new KeyValuePair<string, string>("MOV", ".mov"),
            new KeyValuePair<string, string>("M3U8", ".m3u8"),
            new KeyValuePair<string, string>("WMV", ".wmv"),
            new KeyValuePair<string, string>("3GP", ".3gp"),
            new KeyValuePair<string, string>("3G2", ".3g2"),
            new KeyValuePair<string, string>("MPG", ".mpg"),
            new KeyValuePair<string, string>("VOB", ".vob"),
            new KeyValuePair<string, string>("OGG", ".ogg"),
            new KeyValuePair<string, string>("SWF", ".swf"),
        ];

        public List<KeyValuePair<string, string>> SizeLimitationList { get; } = [];

        public List<KeyValuePair<string, string>> VideoEncodingList { get; } = [];

        public List<KeyValuePair<string, string>> ScreenSizeList { get; } = [];

        public List<KeyValuePair<string, string>> BitRateList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoFormatConversionWindow(ConversionToolsWindow conversionToolsWindow, VideoConversionFileModel videoConversionFileModel)
        {
            InitializeData();
            InitializeComponent();
            InitializeUI(conversionToolsWindow);

            if (videoConversionFileModel is VideoFormatConversionModel videoFormatConversion)
            {
                // TODO：未完成
                SelectedFormatConversionType = FormatConversionTypeList[0];
                SelectedSizeLimitation = SizeLimitationList[0];
                SelectedVideoEncoding = VideoEncodingList[0];
                SelectedScreenSize = ScreenSizeList[0];
            }
        }

        public VideoFormatConversionWindow(ConversionToolsWindow conversionToolsWindow, VideoConversionTypeKind videoConversionTypeKind, WinRTObservableCollection<VideoConversionFileModel> videoConversionFileCollection)
        {
            InitializeData();
            InitializeComponent();
            InitializeUI(conversionToolsWindow);

            if (videoConversionFileCollection.All(item => item is VideoFormatConversionModel))
            {
                SelectedFormatConversionType = FormatConversionTypeList[0];
                SelectedSizeLimitation = SizeLimitationList[0];
                SelectedVideoEncoding = VideoEncodingList[0];
                SelectedScreenSize = ScreenSizeList[0];
            }
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
            SetTitleBarTheme(sender.ActualTheme);
            SetClassicMenuTheme(sender.ActualTheme);
        }

        #endregion 第三部分：窗口内容挂载的事件

        #region 第四部分：内容挂载的事件

        /// <summary>
        /// 加载完成后触发的事件
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            // 设置标题栏主题
            SetTitleBarTheme((Content as FrameworkElement).ActualTheme);
            SetPopupControlTheme(WindowTheme);
        }

        /// <summary>
        /// 点击选择器栏选中项发生变化时发生的事件
        /// </summary>
        private void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectedItem = sender.SelectedItem;
            int index = sender.Items.IndexOf(SelectedItem);
            // TODO：未完成
        }

        /// <summary>
        /// 分割
        /// </summary>
        private void OnSegmentationClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            CloseWindow();
            taskCompletionSource?.TrySetResult(ContentDialogResult.Primary);
        }

        /// <summary>
        /// 滚动列表视图发生变化时触发的事件
        /// </summary>
        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 打开视频编辑
        /// </summary>
        private void OnVideoEditClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 格式转换类型选中项发生变化时触发的事件
        /// </summary>
        private void OnFormatConversionTypeRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> formatConversionType)
            {
                SelectedFormatConversionType = formatConversionType;
            }
        }

        /// <summary>
        /// 视频编码选中项发生变化时触发的事件
        /// </summary>
        private void OnVideoEncodingRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> videoEncoding)
            {
                SelectedVideoEncoding = videoEncoding;
            }
        }

        /// <summary>
        /// 大小限制选中项发生变化时触发的事件
        /// </summary>
        private void OnSizeLimitationRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> sizeLimitation)
            {
                SelectedSizeLimitation = sizeLimitation;
            }
        }

        /// <summary>
        /// 屏幕大小选中项发生变化时触发的事件
        /// </summary>
        private void OnScreenSizeRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> screenSize)
            {
                SelectedScreenSize = screenSize;
                if (!Equals(SelectedScreenSize.Key, ScreenSizeList[17].Key))
                {
                    ScreenWidth = 1;
                    ScreenHeight = 1;
                }
            }
        }

        /// <summary>
        /// 比特率选中项发生变化时触发的事件
        /// </summary>
        private void OnBitRateRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> bitRate)
            {
                SelectedBitRate = bitRate;
            }
        }

        /// <summary>
        /// 自定义屏幕宽度发生变化时触发的事件
        /// </summary>
        private void OnScreenWidthValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    int newValue = Convert.ToInt32(args.NewValue);
                    ScreenWidth = newValue < 1 ? 1 : Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(OnScreenWidthValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 自定义屏幕高度发生变化时触发的事件
        /// </summary>
        private void OnScreenHeightValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    ScreenHeight = args.NewValue < 1 ? 1 : Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(OnScreenHeightValueChanged), 1, e);
                }
            }
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
                if (string.Equals(args.PropertyName, nameof(BackdropService.AppBackdrop)))
                {
                    SetSystemBackdrop();
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
        /// 设置应用的背景色
        /// </summary>
        private void SetSystemBackdrop()
        {
            if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[1]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.Base);
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(ConversionToolsPage, "BackgroundDefault", false);
            }
        }

        /// <summary>
        /// 设置标题栏按钮的主题色
        /// </summary>
        private void SetTitleBarTheme(ElementTheme theme)
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;

            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ForegroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.InactiveForegroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (theme is ElementTheme.Light)
            {
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 23, 23, 23);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 0, 0, 0);
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(51, 0, 0, 0);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 153, 153, 153);
            }
            else
            {
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 242, 242, 242);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 255, 255, 255);
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(51, 255, 255, 255);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 102, 102, 102);
            }
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
        private nint VideoFormatConversionWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
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

                            if (ConversionToolsPage.IsLoaded)
                            {
                                double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                                overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(768 * dpi);
                                overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(560 * dpi);
                            }
                        }, null);
                        break;
                    }
                // 窗口激活状态发生变化时触发的消息
                case WindowMessage.WM_ACTIVATEAPP:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            try
                            {
                                if (WindowSystemBackdrop is MaterialBackdrop materialBackdrop && materialBackdrop.BackdropConfiguration is not null)
                                {
                                    materialBackdrop.BackdropConfiguration.IsInputActive = AlwaysShowBackdropService.AlwaysShowBackdropValue || wParam is not 0;
                                }
                            }
                            catch (Exception e)
                            {
                                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(VideoFormatConversionWindowSubClassProc), 1, e);
                            }
                        }, null);
                        break;
                    }
                // 窗口关闭时触发的消息
                case WindowMessage.WM_CLOSE:
                    {
                        AlwaysShowBackdropService.PropertyChanged -= OnServicePropertyChanged;
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        BackdropService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, videoFormatConversionWindowSubClassProc, 0);
                        // TODO：未完成，目前仅测试
                        taskCompletionSource?.TrySetResult(ContentDialogResult.Primary);
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
                        overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(768 * Convert.ToDouble(wParam) / 96);
                        overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(560 * Convert.ToDouble(wParam) / 96);
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
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            SizeLimitationList.Add(new KeyValuePair<string, string>("None", NoneString));
            SizeLimitationList.Add(new KeyValuePair<string, string>("10", "10MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("15", "15MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("20", "20MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("25", "25MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("30", "30MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("35", "35MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("40", "40MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("50", "50MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("60", "60MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("70", "70MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("80", "80MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("90", "90MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("100", "100MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("150", "150MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("200", "200MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("300", "300MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("500", "500MB"));

            VideoEncodingList.Add(new KeyValuePair<string, string>("Copy", CopyString));
            VideoEncodingList.Add(new KeyValuePair<string, string>("VVC", "VVC(H266)"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("HEVC", "HEVC(H265)"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("AVC", "AVC(H264)"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("MPEG4_DivX", "MPEG4(DivX)"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("MPEG4_Xvid", "MPEG4(Xvid)"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("AV1", "AV1"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("VP9", "VP9"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("1280I", "1280i"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("1920I", "1920i"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("3840I", "3840i"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("25%", "25%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("50%", "50%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("75%", "75%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("125%", "125%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("150%", "150%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("200%", "200%"));
            VideoEncodingList.Add(new KeyValuePair<string, string>("Custom", CustomString));

            ScreenSizeList.Add(new KeyValuePair<string, string>("DefaultSize", DefaultSizeString));
            ScreenSizeList.Add(new KeyValuePair<string, string>("360P", "360p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("480P", "480p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("720P", "720p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1080P", "1080p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("2160P", "2160p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("480I", "480i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("720I", "720i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1280I", "1280i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1920I", "1920i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("3840I", "3840i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("25%", "25%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("50%", "50%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("75%", "75%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("125%", "125%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("150%", "150%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("200%", "200%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("Custom", CustomString));

            BitRateList.Add(new KeyValuePair<string, string>("Default", DefaultString));
            BitRateList.Add(new KeyValuePair<string, string>("256K", "256K"));
            BitRateList.Add(new KeyValuePair<string, string>("384K", "384K"));
            BitRateList.Add(new KeyValuePair<string, string>("512K", "512K"));
            BitRateList.Add(new KeyValuePair<string, string>("768K", "768K"));
            BitRateList.Add(new KeyValuePair<string, string>("1M", "1M"));
            BitRateList.Add(new KeyValuePair<string, string>("1.5M", "1.5M"));
            BitRateList.Add(new KeyValuePair<string, string>("2M", "2M"));
            BitRateList.Add(new KeyValuePair<string, string>("5M", "5M"));
            BitRateList.Add(new KeyValuePair<string, string>("10M", "10M"));
            BitRateList.Add(new KeyValuePair<string, string>("15M", "15M"));
            BitRateList.Add(new KeyValuePair<string, string>("20M", "20M"));
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI(ConversionToolsWindow conversionToolsWindow)
        {
            ConversionToolsWindow = conversionToolsWindow;
            if (IntPtr.Size is 8)
            {
                User32Library.SetWindowLongPtr((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, ConversionToolsWindow.AppWindow.Id.Value);
            }
            else
            {
                User32Library.SetWindowLong((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, ConversionToolsWindow.AppWindow.Id.Value);
            }
            overlappedPresenter = OverlappedPresenter.CreateForDialog();
            ExtendsContentIntoTitleBar = true;
            overlappedPresenter.IsResizable = false;
            overlappedPresenter.IsMinimizable = false;
            overlappedPresenter.IsMaximizable = false;
            overlappedPresenter.IsModal = true;
            AppWindow.SetPresenter(overlappedPresenter);
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(768 * dpi);
            int height = Convert.ToInt32(560 * dpi);
            User32Library.GetWindowRect((nint)ConversionToolsWindow.AppWindow.Id.Value, out RECT parentRect);
            int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
            int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);
            SelectedItem = VideoOutputConfigurationSelectorBar.Items[0];

            // 挂载相应的事件
            AlwaysShowBackdropService.PropertyChanged += OnServicePropertyChanged;
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            videoFormatConversionWindowSubClassProc = new SUBCLASSPROC(VideoFormatConversionWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, videoFormatConversionWindowSubClassProc, 0, 0);

            SetWindowTheme();
            SetSystemBackdrop();
            AppWindow.Closing += (sender, args) =>
            {
                ConversionToolsWindow.Activate();
                ConversionToolsWindow = null;
            };
        }

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
        /// 获取选中的屏幕大小项
        /// </summary>
        private Visibility GetSelectedScreenSize(string selectedScreenSize, string comparedScreenSize)
        {
            return string.Equals(selectedScreenSize, comparedScreenSize, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
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
