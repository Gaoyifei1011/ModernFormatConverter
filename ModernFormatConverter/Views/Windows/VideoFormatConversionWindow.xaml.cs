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
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.Dxgi;
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

        private KeyValuePairModel _selectedFormatConversionType;

        public KeyValuePairModel SelectedFormatConversionType
        {
            get { return _selectedFormatConversionType; }

            set
            {
                _selectedFormatConversionType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormatConversionType)));
            }
        }

        private KeyValuePairModel _selectedSizeLimitation;

        public KeyValuePairModel SelectedSizeLimitation
        {
            get { return _selectedSizeLimitation; }

            set
            {
                _selectedSizeLimitation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSizeLimitation)));
            }
        }

        private KeyValuePairModel _selectedVideoEncoding;

        public KeyValuePairModel SelectedVideoEncoding
        {
            get { return _selectedVideoEncoding; }

            set
            {
                _selectedVideoEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoEncoding)));
            }
        }

        private KeyValuePairModel _selectedScreenSize;

        public KeyValuePairModel SelectedScreenSize
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

        private KeyValuePairModel _selectedVideoBitRate;

        public KeyValuePairModel SelectedVideoBitRate
        {
            get { return _selectedVideoBitRate; }

            set
            {
                _selectedVideoBitRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoBitRate)));
            }
        }

        private bool _useCRF;

        public bool UseCRF
        {
            get { return _useCRF; }

            set
            {
                _useCRF = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseCRF)));
            }
        }

        private int _crf = 10;

        public int CRF
        {
            get { return _crf; }

            set
            {
                _crf = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CRF)));
            }
        }

        private KeyValuePairModel _selectedGPU;

        public KeyValuePairModel SelectedGPU
        {
            get { return _selectedGPU; }

            set
            {
                _selectedGPU = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGPU)));
            }
        }

        public List<KeyValuePairModel> FormatConversionTypeList { get; } =
        [
            new KeyValuePairModel(){ Key = "MP4", Value = ".mp4" },
            new KeyValuePairModel(){ Key = "MKV", Value = ".mkv" },
            new KeyValuePairModel(){ Key = "GIF", Value = ".gif" },
            new KeyValuePairModel(){ Key = "WebM", Value = ".webm" },
            new KeyValuePairModel(){ Key = "AVI", Value = ".avi" },
            new KeyValuePairModel(){ Key = "FLV", Value = ".flv" },
            new KeyValuePairModel(){ Key = "MOV", Value = ".mov" },
            new KeyValuePairModel(){ Key = "M3U8", Value = ".m3u8" },
            new KeyValuePairModel(){ Key = "3GP", Value = ".3gp" },
            new KeyValuePairModel(){ Key = "3G2", Value = ".3g2" },
            new KeyValuePairModel(){ Key = "MPG", Value = ".mpg" },
            new KeyValuePairModel(){ Key = "VOB", Value = ".vob" },
            new KeyValuePairModel(){ Key = "OGG", Value = ".ogg" },
            new KeyValuePairModel(){ Key = "SWF", Value = ".swf" },
        ];

        public List<KeyValuePairModel> SizeLimitationList { get; } = [];

        public List<KeyValuePairModel> VideoEncodingList { get; } = [];

        public List<KeyValuePairModel> ScreenSizeList { get; } = [];

        public List<KeyValuePairModel> VideoBitRateList { get; } = [];

        public List<KeyValuePairModel> GPUList { get; } = [];

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
                SelectedFormatConversionType.IsChecked = true;
                SelectedSizeLimitation = SizeLimitationList[0];
                SelectedSizeLimitation.IsChecked = true;
                SelectedVideoEncoding = VideoEncodingList[0];
                SelectedVideoEncoding.IsChecked = true;
                SelectedScreenSize = ScreenSizeList[0];
                SelectedScreenSize.IsChecked = true;
                SelectedVideoBitRate = VideoBitRateList[0];
                SelectedVideoBitRate.IsChecked = true;
                SelectedGPU = GPUList[0];
                SelectedGPU.IsChecked = true;
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
                SelectedFormatConversionType.IsChecked = true;
                SelectedSizeLimitation = SizeLimitationList[0];
                SelectedSizeLimitation.IsChecked = true;
                SelectedVideoEncoding = VideoEncodingList[0];
                SelectedVideoEncoding.IsChecked = true;
                SelectedScreenSize = ScreenSizeList[0];
                SelectedScreenSize.IsChecked = true;
                SelectedVideoBitRate = VideoBitRateList[0];
                SelectedVideoBitRate.IsChecked = true;
                SelectedGPU = GPUList[0];
                SelectedGPU.IsChecked = true;
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

        #region 第四部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 修改格式转换类型
        /// </summary>
        private void OnFormatConversionTypeExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (FormatConversionTypeFlyout.IsOpen)
            {
                FormatConversionTypeFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel formatConversionType)
            {
                foreach (KeyValuePairModel formatConversionTypeItem in FormatConversionTypeList)
                {
                    formatConversionTypeItem.IsChecked = false;
                    if (string.Equals(formatConversionType.Key, formatConversionTypeItem.Key))
                    {
                        SelectedFormatConversionType = formatConversionTypeItem;
                        formatConversionTypeItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改大小限制
        /// </summary>
        private void OnSizeLimitationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (SizeLimitationFlyout.IsOpen)
            {
                SizeLimitationFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel sizeLimitation)
            {
                foreach (KeyValuePairModel sizeLimitationItem in SizeLimitationList)
                {
                    sizeLimitationItem.IsChecked = false;
                    if (string.Equals(sizeLimitation.Key, sizeLimitationItem.Key))
                    {
                        SelectedSizeLimitation = sizeLimitationItem;
                        sizeLimitationItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改视频编码
        /// </summary>
        private void OnVideoEncodingExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (VideoEncodingFlyout.IsOpen)
            {
                VideoEncodingFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel videoEncoding)
            {
                foreach (KeyValuePairModel videoEncodingItem in VideoEncodingList)
                {
                    videoEncodingItem.IsChecked = false;
                    if (string.Equals(videoEncoding.Key, videoEncodingItem.Key))
                    {
                        SelectedVideoEncoding = videoEncodingItem;
                        videoEncodingItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改屏幕大小
        /// </summary>
        private void OnScreenSizeExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (ScreenSizeFlyout.IsOpen)
            {
                ScreenSizeFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel screenSize)
            {
                foreach (KeyValuePairModel screenSizeItem in ScreenSizeList)
                {
                    screenSizeItem.IsChecked = false;
                    if (string.Equals(screenSize.Key, screenSizeItem.Key))
                    {
                        SelectedScreenSize = screenSizeItem;
                        screenSizeItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改视频比特率
        /// </summary>
        private void OnVideoBitRateExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (VideoBitRateFlyout.IsOpen)
            {
                VideoBitRateFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel videoBitRate)
            {
                foreach (KeyValuePairModel videoBitRateItem in VideoBitRateList)
                {
                    videoBitRateItem.IsChecked = false;
                    if (string.Equals(videoBitRate.Key, videoBitRateItem.Key))
                    {
                        SelectedVideoBitRate = videoBitRateItem;
                        videoBitRateItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改 GPU
        /// </summary>
        private void OnGPUExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (GPUFlyout.IsOpen)
            {
                GPUFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel gpu)
            {
                foreach (KeyValuePairModel gpuItem in GPUList)
                {
                    gpuItem.IsChecked = false;
                    if (string.Equals(gpu.Key, gpuItem.Key))
                    {
                        SelectedGPU = gpuItem;
                        gpuItem.IsChecked = true;
                    }
                }
            }
        }

        #endregion 第四部分：ExecuteCommand 命令调用时挂载的事件

        #region 第五部分：内容挂载的事件

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
            Close();
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
        /// 格式转换类型菜单打开时自动定位到选中项
        /// </summary>
        private void OnFormatConversionTypeOpened(object sender, object args)
        {
            foreach (KeyValuePairModel formatConversionType in FormatConversionTypeList)
            {
                if (formatConversionType.IsChecked)
                {
                    FormatConversionTypeListView.ScrollIntoView(formatConversionType);
                    break;
                }
            }
        }

        /// <summary>
        /// 大小限制菜单打开时自动定位到选中项
        /// </summary>
        private void OnSizeLimitationOpened(object sender, object args)
        {
            foreach (KeyValuePairModel sizeLimitation in SizeLimitationList)
            {
                if (sizeLimitation.IsChecked)
                {
                    SizeLimitationListView.ScrollIntoView(sizeLimitation);
                    break;
                }
            }
        }

        /// <summary>
        /// 视频编码菜单打开时自动定位到选中项
        /// </summary>
        private void OnVideoEncodingOpened(object sender, object args)
        {
            foreach (KeyValuePairModel videoEncoding in VideoEncodingList)
            {
                if (videoEncoding.IsChecked)
                {
                    VideoEncodingListView.ScrollIntoView(videoEncoding);
                    break;
                }
            }
        }

        /// <summary>
        /// 屏幕大小菜单打开时自动定位到选中项
        /// </summary>
        private void OnScreenSizeOpened(object sender, object args)
        {
            foreach (KeyValuePairModel screenSize in ScreenSizeList)
            {
                if (screenSize.IsChecked)
                {
                    ScreenSizeListView.ScrollIntoView(screenSize);
                    break;
                }
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

        /// <summary>
        /// 视频选择率菜单打开时自动定位到选中项
        /// </summary>
        private void OnVideoBitRateOpened(object sender, object args)
        {
            foreach (KeyValuePairModel videoBitRate in VideoBitRateList)
            {
                if (videoBitRate.IsChecked)
                {
                    VideoBitRateListView.ScrollIntoView(videoBitRate);
                    break;
                }
            }
        }

        /// <summary>
        /// 是否启用 CRF
        /// </summary>
        private void OnUseCRFToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                UseCRF = toggleSwitch.IsOn;
                if (!UseCRF)
                {
                    CRF = 10;
                }
            }
        }

        /// <summary>
        /// 自定义屏幕宽度发生变化时触发的事件
        /// </summary>
        private void OnCRFValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    if (args.NewValue < 10)
                    {
                        CRF = 10;
                    }
                    else if (args.NewValue > 50)
                    {
                        CRF = 50;
                    }
                    else
                    {
                        CRF = Convert.ToInt32(args.NewValue);
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(OnCRFValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// GPU 菜单打开时自动定位到选中项
        /// </summary>
        private void OnGPUOpened(object sender, object args)
        {
            foreach (KeyValuePairModel gpu in GPUList)
            {
                if (gpu.IsChecked)
                {
                    VideoBitRateListView.ScrollIntoView(gpu);
                    break;
                }
            }
        }

        #endregion 第五部分：内容挂载的事件

        #region 第六部分：自定义事件

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

        #endregion 第六部分：自定义事件

        #region 第七部分：窗口及内容属性设置

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
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(VideoFormatConversionPage, "BackgroundDefault", false);
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

        #endregion 第七部分：窗口及内容属性设置

        #region 第八部分：窗口过程

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

                            if (VideoFormatConversionPage.IsLoaded)
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
                // 窗口销毁后触发的消息
                case WindowMessage.WM_DESTROY:
                    {
                        AlwaysShowBackdropService.PropertyChanged -= OnServicePropertyChanged;
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        BackdropService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, videoFormatConversionWindowSubClassProc, 0);
                        // TODO：未完成，目前仅测试
                        if (!taskCompletionSource.Task.IsCompleted)
                        {
                            taskCompletionSource.TrySetResult(ContentDialogResult.None);
                        }
                        ConversionToolsWindow.Activate();
                        ConversionToolsWindow = null;
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
                                    Position = new Point(0, 30),
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

        #endregion 第八部分：窗口过程

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "10", Value = "10MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "15", Value = "15MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "20", Value = "20MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "25", Value = "25MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "30", Value = "30MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "35", Value = "35MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "40", Value = "40MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "50", Value = "50MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "60", Value = "60MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "70", Value = "70MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "80", Value = "80MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "90", Value = "90MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "100", Value = "100MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "150", Value = "150MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "200", Value = "200MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "300", Value = "300MB" });
            SizeLimitationList.Add(new KeyValuePairModel() { Key = "500", Value = "500MB" });

            VideoEncodingList.Add(new KeyValuePairModel() { Key = "Copy", Value = CopyString });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "VVC", Value = "VVC(H266)" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "HEVC", Value = "HEVC(H265)" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "AVC", Value = "AVC(H264)" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "AV1", Value = "AV1" });
            VideoEncodingList.Add(new KeyValuePairModel() { Key = "VP9", Value = "VP9" });

            ScreenSizeList.Add(new KeyValuePairModel() { Key = "DefaultSize", Value = DefaultSizeString });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "360P", Value = "360p" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "480P", Value = "480p" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "720P", Value = "720p" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "1080P", Value = "1080p" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "2160P", Value = "2160p" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "480I", Value = "480i" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "720I", Value = "720i" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "1280I", Value = "1280i" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "1920I", Value = "1920i" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "3840I", Value = "3840i" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "25%", Value = "25%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "50%", Value = "50%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "75%", Value = "75%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "125%", Value = "125%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "150%", Value = "150%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "200%", Value = "200%" });
            ScreenSizeList.Add(new KeyValuePairModel() { Key = "Custom", Value = CustomString });

            VideoBitRateList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "256K", Value = "256K" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "384K", Value = "384K" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "512K", Value = "512K" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "768K", Value = "768K" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "1M", Value = "1M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "1.5M", Value = "1.5M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "2M", Value = "2M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "5M", Value = "5M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "10M", Value = "10M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "15M", Value = "15M" });
            VideoBitRateList.Add(new KeyValuePairModel() { Key = "20M", Value = "20M" });

            uint iAdapterNum = 0;
            Guid CLSID_DxgiFactory = new("7B7166EC-21C7-44AE-B21A-C9AE321AE369");
            int DXGI_ERROR_NOT_FOUND = unchecked((int)0x887A0002);
            List<uint> dxgiAdapterList = [];

            if (DxgiLibrary.CreateDXGIFactory(CLSID_DxgiFactory, out IDXGIFactory dxgiFactory) is 0)
            {
                while (true)
                {
                    if (dxgiFactory.EnumAdapters(iAdapterNum, out IDXGIAdapter dxgiAdapter) != DXGI_ERROR_NOT_FOUND)
                    {
                        dxgiAdapter.GetDesc(out DXGI_ADAPTER_DESC dxgiAdapterDesc);
                        dxgiAdapterList.Add(dxgiAdapterDesc.VendorId);
                        iAdapterNum++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            GPUList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });

            if (dxgiAdapterList.Contains(32902))
            {
                GPUList.Add(new KeyValuePairModel() { Key = "INTEL", Value = "INTEL" });
            }

            if (dxgiAdapterList.Contains(4318))
            {
                GPUList.Add(new KeyValuePairModel() { Key = "NVIDIA", Value = "NVIDIA" });
            }

            if (dxgiAdapterList.Contains(4098))
            {
                GPUList.Add(new KeyValuePairModel() { Key = "AMD", Value = "AMD" });
            }
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
