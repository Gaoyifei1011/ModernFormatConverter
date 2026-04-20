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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
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
        private readonly string AllFilesString = ResourceService.VideoFormatConversionResource.GetString("AllFiles");
        private readonly string BorderAndShadowString = ResourceService.VideoFormatConversionResource.GetString("BorderAndShadow");
        private readonly string CopyString = ResourceService.VideoFormatConversionResource.GetString("Copy");
        private readonly string CustomString = ResourceService.VideoFormatConversionResource.GetString("Custom");
        private readonly string DefaultString = ResourceService.VideoFormatConversionResource.GetString("Default");
        private readonly string DefaultSizeString = ResourceService.VideoFormatConversionResource.GetString("DefaultSize");
        private readonly string LargeString = ResourceService.VideoFormatConversionResource.GetString("Large");
        private readonly string MonoString = ResourceService.VideoFormatConversionResource.GetString("Mono");
        private readonly string NoneString = ResourceService.VideoFormatConversionResource.GetString("None");
        private readonly string NormalString = ResourceService.VideoFormatConversionResource.GetString("Normal");
        private readonly string NoRotateString = ResourceService.VideoFormatConversionResource.GetString("NoRotate");
        private readonly string QuadString = ResourceService.VideoFormatConversionResource.GetString("Quad");
        private readonly string RotateLeftString = ResourceService.VideoFormatConversionResource.GetString("RotateLeft");
        private readonly string RotateRightString = ResourceService.VideoFormatConversionResource.GetString("RotateRight");
        private readonly string SecondString = ResourceService.VideoFormatConversionResource.GetString("Second");
        private readonly string SelectFileString = ResourceService.VideoFormatConversionResource.GetString("SelectFile");
        private readonly string SmallString = ResourceService.VideoFormatConversionResource.GetString("Small");
        private readonly string SolidColorBackgroundString = ResourceService.VideoFormatConversionResource.GetString("SolidColorBackground");
        private readonly string StereoString = ResourceService.VideoFormatConversionResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.VideoFormatConversionResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.VideoFormatConversionResource.GetString("Stereo71");
        private readonly string SubtitleString = ResourceService.VideoFormatConversionResource.GetString("Subtitle");
        private readonly string UnsideDownString = ResourceService.VideoFormatConversionResource.GetString("UnsideDown");
        private readonly List<DictionaryEntry> GPUList = [];
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

        private DictionaryEntry _selectedFormatConversionType;

        public DictionaryEntry SelectedFormatConversionType
        {
            get { return _selectedFormatConversionType; }

            set
            {
                _selectedFormatConversionType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormatConversionType)));
            }
        }

        private DictionaryEntry _selectedSizeLimitation;

        public DictionaryEntry SelectedSizeLimitation
        {
            get { return _selectedSizeLimitation; }

            set
            {
                _selectedSizeLimitation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSizeLimitation)));
            }
        }

        private DictionaryEntry _selectedVideoEncoding;

        public DictionaryEntry SelectedVideoEncoding
        {
            get { return _selectedVideoEncoding; }

            set
            {
                _selectedVideoEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoEncoding)));
            }
        }

        private DictionaryEntry _selectedScreenSize;

        public DictionaryEntry SelectedScreenSize
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

        private DictionaryEntry _selectedVideoBitRate;

        public DictionaryEntry SelectedVideoBitRate
        {
            get { return _selectedVideoBitRate; }

            set
            {
                _selectedVideoBitRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoBitRate)));
            }
        }

        private bool _isCRFSupported;

        public bool IsCRFSupported
        {
            get { return _isCRFSupported; }

            set
            {
                _isCRFSupported = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCRFSupported)));
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

        private DictionaryEntry _selectedGPU;

        public DictionaryEntry SelectedGPU
        {
            get { return _selectedGPU; }

            set
            {
                _selectedGPU = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGPU)));
            }
        }

        private DictionaryEntry _selectedFramePerSecond;

        public DictionaryEntry SelectedFramePerSecond
        {
            get { return _selectedFramePerSecond; }

            set
            {
                _selectedFramePerSecond = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFramePerSecond)));
            }
        }

        private DictionaryEntry _selectedAspectRatio;

        public DictionaryEntry SelectedAspectRatio
        {
            get { return _selectedAspectRatio; }

            set
            {
                _selectedAspectRatio = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAspectRatio)));
            }
        }

        private bool _isSecondaryEncodingEnabled;

        public bool IsSecondaryEncodingEnabled
        {
            get { return _isSecondaryEncodingEnabled; }

            set
            {
                _isSecondaryEncodingEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSecondaryEncodingEnabled)));
            }
        }

        private bool _secondaryEncoding;

        public bool SecondaryEncoding
        {
            get { return _secondaryEncoding; }

            set
            {
                _secondaryEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryEncoding)));
            }
        }

        private DictionaryEntry _selectedKeyFrameInterval;

        public DictionaryEntry SelectedKeyFrameInterval
        {
            get { return _selectedKeyFrameInterval; }

            set
            {
                _selectedKeyFrameInterval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrameInterval)));
            }
        }

        private bool _deInterlace;

        public bool DeInterlace
        {
            get { return _deInterlace; }

            set
            {
                _deInterlace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeInterlace)));
            }
        }

        private DictionaryEntry _selectedRotation;

        public DictionaryEntry SelectedRotation
        {
            get { return _selectedRotation; }

            set
            {
                _selectedRotation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRotation)));
            }
        }

        private bool _mirrorReversal;

        public bool MirrorReversal
        {
            get { return _mirrorReversal; }

            set
            {
                _mirrorReversal = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MirrorReversal)));
            }
        }

        private DictionaryEntry _selectedVideoFadeInEffect;

        public DictionaryEntry SelectedVideoFadeInEffect
        {
            get { return _selectedVideoFadeInEffect; }

            set
            {
                _selectedVideoFadeInEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeInEffect)));
            }
        }

        private DictionaryEntry _selectedVideoFadeOutEffect;

        public DictionaryEntry SelectedVideoFadeOutEffect
        {
            get { return _selectedVideoFadeOutEffect; }

            set
            {
                _selectedVideoFadeOutEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeOutEffect)));
            }
        }

        private bool _isAudioConfigurationSupported;

        public bool IsAudioConfigurationSupported
        {
            get { return _isAudioConfigurationSupported; }

            set
            {
                _isAudioConfigurationSupported = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAudioConfigurationSupported)));
            }
        }

        private DictionaryEntry _selectedAudioEncoding;

        public DictionaryEntry SelectedAudioEncoding
        {
            get { return _selectedAudioEncoding; }

            set
            {
                _selectedAudioEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioEncoding)));
            }
        }

        private DictionaryEntry _selectedSamplingRate;

        public DictionaryEntry SelectedSamplingRate
        {
            get { return _selectedSamplingRate; }

            set
            {
                _selectedSamplingRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSamplingRate)));
            }
        }

        private DictionaryEntry _selectedAudioBitRate;

        public DictionaryEntry SelectedAudioBitRate
        {
            get { return _selectedAudioBitRate; }

            set
            {
                _selectedAudioBitRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioBitRate)));
            }
        }

        private DictionaryEntry _selectedSoundTrack;

        public DictionaryEntry SelectedSoundTrack
        {
            get { return _selectedSoundTrack; }

            set
            {
                _selectedSoundTrack = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSoundTrack)));
            }
        }

        private bool _closeSoundEffect;

        public bool CloseSoundEffect
        {
            get { return _closeSoundEffect; }

            set
            {
                _closeSoundEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloseSoundEffect)));
            }
        }

        private DictionaryEntry _selectedVolume;

        public DictionaryEntry SelectedVolume
        {
            get { return _selectedVolume; }

            set
            {
                _selectedVolume = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVolume)));
            }
        }

        private bool _preserveAllSourceInputAudioStream;

        public bool PreserveAllSourceInputAudioStream
        {
            get { return _preserveAllSourceInputAudioStream; }

            set
            {
                _preserveAllSourceInputAudioStream = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreserveAllSourceInputAudioStream)));
            }
        }

        private DictionaryEntry _selectedAudioFadeInEffect;

        public DictionaryEntry SelectedAudioFadeInEffect
        {
            get { return _selectedAudioFadeInEffect; }

            set
            {
                _selectedAudioFadeInEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioFadeInEffect)));
            }
        }

        private DictionaryEntry _selectedAudioFadeOutEffect;

        public DictionaryEntry SelectedAudioFadeOutEffect
        {
            get { return _selectedAudioFadeOutEffect; }

            set
            {
                _selectedAudioFadeOutEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioFadeOutEffect)));
            }
        }

        private bool _echo;

        public bool Echo
        {
            get { return _echo; }

            set
            {
                _echo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Echo)));
            }
        }

        private bool _deNoise;

        public bool DeNoise
        {
            get { return _deNoise; }

            set
            {
                _deNoise = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeNoise)));
            }
        }

        private bool _reverse;

        public bool Reverse
        {
            get { return _reverse; }

            set
            {
                _reverse = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reverse)));
            }
        }

        private bool _isPreserveAllSourceInputSubtitleStreamEnabled;

        public bool IsPreserveAllSourceInputSubtitleStreamEnabled
        {
            get { return _isPreserveAllSourceInputSubtitleStreamEnabled; }

            set
            {
                _isPreserveAllSourceInputSubtitleStreamEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPreserveAllSourceInputSubtitleStreamEnabled)));
            }
        }

        private bool _preserveAllSourceInputSubtitleStream;

        public bool PreserveAllSourceInputSubtitleStream
        {
            get { return _preserveAllSourceInputSubtitleStream; }

            set
            {
                _preserveAllSourceInputSubtitleStream = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreserveAllSourceInputSubtitleStream)));
            }
        }

        private string _AdditionalSubtitlePath;

        public string AdditionalSubtitlePath
        {
            get { return _AdditionalSubtitlePath; }

            set
            {
                _AdditionalSubtitlePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdditionalSubtitlePath)));
            }
        }

        private string _subtitleNestType;

        public string SubtitleNestType
        {
            get { return _subtitleNestType; }

            set
            {
                _subtitleNestType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubtitleNestType)));
            }
        }

        private DictionaryEntry _selectedSubtitleNestType;

        public DictionaryEntry SelectedSubtitleNestType
        {
            get { return _selectedSubtitleNestType; }

            set
            {
                _selectedSubtitleNestType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubtitleNestType)));
            }
        }

        private string _fontName;

        public string FontName
        {
            get { return _fontName; }

            set
            {
                _fontName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontName)));
            }
        }

        private DictionaryEntry _selectedFontSize;

        public DictionaryEntry SelectedFontSize
        {
            get { return _selectedFontSize; }

            set
            {
                _selectedFontSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontSize)));
            }
        }

        private string _fontColor;

        public string FontColor
        {
            get { return _fontColor; }

            set
            {
                _fontColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
            }
        }

        private DictionaryEntry _selectedFontBorderStyle;

        public DictionaryEntry SelectedFontBorderStyle
        {
            get { return _selectedFontBorderStyle; }

            set
            {
                _selectedFontBorderStyle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontBorderStyle)));
            }
        }

        private DictionaryEntry _selectedCounterLineSize;

        public DictionaryEntry SelectedCounterLineSize
        {
            get { return _selectedCounterLineSize; }

            set
            {
                _selectedCounterLineSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCounterLineSize)));
            }
        }

        private string _counterLineColor;

        public string CounterLineColor
        {
            get { return _counterLineColor; }

            set
            {
                _counterLineColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CounterLineColor)));
            }
        }

        private DictionaryEntry _selectedShadowSize;

        public DictionaryEntry SelectedShadowSize
        {
            get { return _selectedShadowSize; }

            set
            {
                _selectedShadowSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedShadowSize)));
            }
        }

        public List<DictionaryEntry> FormatConversionTypeList { get; } =
        [
            new DictionaryEntry(){ Key = "MP4", Value = ".mp4" },
            new DictionaryEntry(){ Key = "MKV", Value = ".mkv" },
            new DictionaryEntry(){ Key = "GIF", Value = ".gif" },
            new DictionaryEntry(){ Key = "WebM", Value = ".webm" },
            new DictionaryEntry(){ Key = "AVI", Value = ".avi" },
            new DictionaryEntry(){ Key = "FLV", Value = ".flv" },
            new DictionaryEntry(){ Key = "MOV", Value = ".mov" },
            new DictionaryEntry(){ Key = "M3U8", Value = ".m3u8" },
            new DictionaryEntry(){ Key = "WMV", Value = ".wmv" },
            new DictionaryEntry(){ Key = "3GP", Value = ".3gp" },
            new DictionaryEntry(){ Key = "3G2", Value = ".3g2" },
            new DictionaryEntry(){ Key = "MPG", Value = ".mpg" },
            new DictionaryEntry(){ Key = "VOB", Value = ".vob" },
            new DictionaryEntry(){ Key = "OGG", Value = ".ogg" },
            new DictionaryEntry(){ Key = "SWF", Value = ".swf" },
        ];

        public WinRTObservableCollection<DictionaryEntry> SizeLimitationCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> VideoEncodingCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> ScreenSizeCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> VideoBitRateCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> GPUCollection { get; } = [];

        public List<DictionaryEntry> FramePerSecondList { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> AspectRatioCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> KeyFrameIntervalCollection { get; } = [];

        public List<DictionaryEntry> RotationList { get; } = [];

        public List<DictionaryEntry> VideoFadeInEffectList { get; } = [];

        public List<DictionaryEntry> VideoFadeOutEffectList { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> AudioEncodingCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> SamplingRateCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> AudioBitRateCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> SoundTrackCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> VolumeCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> AudioFadeInEffectCollection { get; } = [];

        public WinRTObservableCollection<DictionaryEntry> AudioFadeOutEffectCollection { get; } = [];

        public List<DictionaryEntry> SubtitleNestTypeList { get; } = [];

        public List<DictionaryEntry> FontSizeList { get; } = [];

        public List<DictionaryEntry> FontBorderStyleList { get; } = [];

        public List<DictionaryEntry> CounterLineSizeList { get; } = [];

        public List<DictionaryEntry> ShadowSizeList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoFormatConversionWindow(ConversionToolsWindow conversionToolsWindow, VideoFormatConversionModel videoFormatConversion = null)
        {
            InitializeData(videoFormatConversion);
            InitializeComponent();
            InitializeUI(conversionToolsWindow);
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
            if (VideoFormatConversionScroll.IsLoaded && !Equals(SelectedItem, sender.SelectedItem))
            {
                SelectedItem = sender.SelectedItem;
                int index = sender.Items.IndexOf(SelectedItem);

                switch (index)
                {
                    case 0:
                        {
                            double currentScrollPosition = VideoFormatConversionScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = VideoHeader.TransformToVisual(VideoFormatConversionScroll).TransformPoint(currentPoint);
                            VideoFormatConversionScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 1:
                        {
                            double currentScrollPosition = VideoFormatConversionScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = AudioHeader.TransformToVisual(VideoFormatConversionScroll).TransformPoint(currentPoint);
                            VideoFormatConversionScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 2:
                        {
                            double currentScrollPosition = VideoFormatConversionScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = SubtitleHeader.TransformToVisual(VideoFormatConversionScroll).TransformPoint(currentPoint);
                            VideoFormatConversionScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                }
            }
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
            double currentScrollPosition = VideoFormatConversionScroll.VerticalOffset;
            Point currentPoint = new(0, (int)currentScrollPosition);
            Point audioHeaderTargetPosition = AudioHeader.TransformToVisual(VideoFormatConversionScroll).TransformPoint(currentPoint);
            Point subtitleHeaderTargetPosition = SubtitleHeader.TransformToVisual(VideoFormatConversionScroll).TransformPoint(currentPoint);

            if (currentScrollPosition >= subtitleHeaderTargetPosition.Y)
            {
                SelectedItem = VideoFormatConversionSelectorBar.Items[2];
            }
            else if (currentScrollPosition >= audioHeaderTargetPosition.Y && currentScrollPosition < subtitleHeaderTargetPosition.Y)
            {
                SelectedItem = VideoFormatConversionSelectorBar.Items[1];
            }
            else
            {
                SelectedItem = VideoFormatConversionSelectorBar.Items[0];
            }
        }

        /// <summary>
        /// 格式转换类型菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFormatConversionTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry formatConversionType && !Equals(SelectedFormatConversionType, formatConversionType))
            {
                SelectedFormatConversionType = formatConversionType;

                ResetVideoEncoding();
                SelectedVideoEncoding = VideoEncodingCollection[0];

                ResetSizeLimitation();
                SelectedSizeLimitation = SizeLimitationCollection[0];

                ResetScreenSize();
                SelectedScreenSize = ScreenSizeCollection[0];

                ResetVideoBitRate();
                SelectedVideoBitRate = VideoBitRateCollection[0];

                ResetCRF();
                ResetGPU();
                SelectedGPU = GPUCollection[0];

                ResetAspectRatio();
                SelectedAspectRatio = AspectRatioCollection[0];

                ResetSecondaryEncoding();

                ResetKeyFrameInterval();
                SelectedKeyFrameInterval = KeyFrameIntervalCollection[0];

                IsAudioConfigurationSupported = !Equals(SelectedFormatConversionType, FormatConversionTypeList[2]);

                ResetAudioEncoding();
                SelectedAudioEncoding = AudioEncodingCollection[0];

                ResetSamplingRate();
                SelectedSamplingRate = SamplingRateCollection[0];

                ResetAudioBitRate();
                SelectedAudioBitRate = AudioBitRateCollection[0];

                ResetSoundTrack();
                SelectedSoundTrack = SoundTrackCollection[0];

                ResetVolume();
                SelectedVolume = VolumeCollection[5];

                ResetAudioFadeInEffect();
                SelectedAudioFadeInEffect = AudioFadeInEffectCollection[0];

                ResetAudioFadeOutEffect();
                SelectedAudioFadeOutEffect = AudioFadeOutEffectCollection[0];

                ResetPreserveAllSourceInputSubtitleStream();

                if (!IsAudioConfigurationSupported)
                {
                    CloseSoundEffect = false;
                    PreserveAllSourceInputAudioStream = false;
                    Echo = false;
                    DeNoise = false;
                    Reverse = false;
                }
            }
        }

        /// <summary>
        /// 视频编码菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoEncodingSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry videoEncoding && !Equals(SelectedVideoEncoding, videoEncoding))
            {
                SelectedVideoEncoding = videoEncoding;

                ResetSizeLimitation();
                SelectedSizeLimitation = SizeLimitationCollection[0];

                ResetCRF();

                ResetGPU();
                SelectedGPU = GPUCollection[0];

                ResetSecondaryEncoding();
            }
        }

        /// <summary>
        /// 大小限制菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSizeLimitationSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry sizeLimitation && !Equals(SelectedSizeLimitation, sizeLimitation))
            {
                SelectedSizeLimitation = sizeLimitation;
            }
        }

        /// <summary>
        /// 屏幕大小菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnScreenSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry screenSize && !Equals(SelectedScreenSize, screenSize))
            {
                SelectedScreenSize = screenSize;
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
        /// 视频比特率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoBitRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry videoBitRate && !Equals(SelectedVideoBitRate, videoBitRate))
            {
                SelectedVideoBitRate = videoBitRate;
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
        /// CRF 发生变化时触发的事件
        /// </summary>
        private void OnCRFValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    CRF = Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(OnCRFValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// GPU 菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnGPUSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry gpu && !Equals(SelectedGPU, gpu))
            {
                SelectedGPU = gpu;
            }
        }

        /// <summary>
        /// 每秒帧数菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFramePerSecondSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry framePerSecond && !Equals(SelectedFramePerSecond, framePerSecond))
            {
                SelectedFramePerSecond = framePerSecond;
            }
        }

        /// <summary>
        /// 宽高比菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAspectRatioSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry aspectRatio && !Equals(SelectedFramePerSecond, aspectRatio))
            {
                SelectedAspectRatio = aspectRatio;
            }
        }

        /// <summary>
        /// 是否启用二次编码
        /// </summary>
        private void OnSecondaryEncodingToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                SecondaryEncoding = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 关键帧间隔菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnKeyFrameIntervalSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry keyFrameInterval && !Equals(SelectedFramePerSecond, keyFrameInterval))
            {
                SelectedKeyFrameInterval = keyFrameInterval;
            }
        }

        /// <summary>
        /// 是否启用反交错
        /// </summary>
        private void OnDeInterlaceToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                DeInterlace = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 旋转菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnRotationSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry rotation && !Equals(SelectedRotation, rotation))
            {
                SelectedRotation = rotation;
            }
        }

        /// <summary>
        /// 是否启用镜像反转
        /// </summary>
        private void OnMirrorReversalToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                MirrorReversal = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 视频淡入效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoFadeInEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry videoFadeInEffect && !Equals(SelectedVideoFadeInEffect, videoFadeInEffect))
            {
                SelectedVideoFadeInEffect = videoFadeInEffect;
            }
        }

        /// <summary>
        /// 视频淡出效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoFadeOutEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry videoFadeOutEffect && !Equals(SelectedVideoFadeOutEffect, videoFadeOutEffect))
            {
                SelectedVideoFadeOutEffect = videoFadeOutEffect;
            }
        }

        /// <summary>
        /// 音频编码效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioEncodingSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry audioEncoding && !Equals(SelectedAudioEncoding, audioEncoding))
            {
                SelectedAudioEncoding = audioEncoding;

                ResetSamplingRate();

                SelectedSamplingRate = SamplingRateCollection[0];
            }
        }

        /// <summary>
        /// 采样率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSamplingRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry samplingRate && !Equals(SelectedSamplingRate, samplingRate))
            {
                SelectedSamplingRate = samplingRate;
            }
        }

        /// <summary>
        /// 音频比特率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioBitRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry audioBitRate && !Equals(SelectedAudioBitRate, audioBitRate))
            {
                SelectedAudioBitRate = audioBitRate;
            }
        }

        /// <summary>
        /// 声道菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSoundTrackSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry soundTrack && !Equals(SelectedSoundTrack, soundTrack))
            {
                SelectedSoundTrack = soundTrack;
            }
        }

        /// <summary>
        /// 是否关闭音效
        /// </summary>
        private void OnCloseSoundEffectToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                CloseSoundEffect = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 音量菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVolumeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry volume && !Equals(SelectedVolume, volume))
            {
                SelectedVolume = volume;
            }
        }

        /// <summary>
        /// 是否保留所有源音频输入流
        /// </summary>
        private void OnPreserveAllSourceInputAudioStreamToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                PreserveAllSourceInputAudioStream = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 音频淡入效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioFadeInEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry audioFadeInEffect && !Equals(SelectedAudioFadeInEffect, audioFadeInEffect))
            {
                SelectedAudioFadeInEffect = audioFadeInEffect;
            }
        }

        /// <summary>
        /// 音频淡出效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioFadeOutEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry audioFadeOutEffect && !Equals(SelectedAudioFadeOutEffect, audioFadeOutEffect))
            {
                SelectedAudioFadeOutEffect = audioFadeOutEffect;
            }
        }

        /// <summary>
        /// 是否启用回声
        /// </summary>
        private void OnEchoToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                Echo = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 是否启用降噪
        /// </summary>
        private void OnDeNoiseToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                DeNoise = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 是否启用反向
        /// </summary>
        private void OnReverseToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                Reverse = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 是否保留所有源字幕输入流
        /// </summary>
        private void OnPreserveAllSourceInputSubtitleStreamToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                PreserveAllSourceInputSubtitleStream = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 打开附加字幕目录
        /// </summary>
        private void OnAdditionalSubtitlePathClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(Path.GetDirectoryName(AdditionalSubtitlePath));
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoFormatConversionWindow), nameof(OnAdditionalSubtitlePathClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 选择附加字幕
        /// </summary>
        private void OnSelectAdditionalSubtitleClicked(object sender, RoutedEventArgs args)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString,
                Filter = string.Format("{0} (*.srt;*.ass;*.ssa)|*.srt;*.ass;*.ssa|{1} (*.*)|*.*", SubtitleString, AllFilesString)
            };
            if (openFileDialog.ShowDialog() is System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName) && File.Exists(openFileDialog.FileName))
            {
                AdditionalSubtitlePath = openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 字幕嵌入类型菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSubtitleNestTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry subtitleNestType && !Equals(SelectedSubtitleNestType, subtitleNestType))
            {
                SelectedSubtitleNestType = subtitleNestType;
            }
        }

        /// <summary>
        /// 修改字体名称
        /// </summary>
        private void OnChangeFontNameClicked(object sender, RoutedEventArgs args)
        {
            System.Windows.Forms.FontDialog fontDialog = new()
            {
                Font = new(FontName, System.Drawing.SystemFonts.DefaultFont.Size)
            };
            if (fontDialog.ShowDialog() is System.Windows.Forms.DialogResult.OK && fontDialog.Font is not null)
            {
                FontName = fontDialog.Font.Name;
            }
            fontDialog.Dispose();
        }

        /// <summary>
        /// 字体名称菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFontSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry fontSize && !Equals(SelectedFontSize, fontSize))
            {
                SelectedFontSize = fontSize;
            }
        }

        /// <summary>
        /// 修改字体颜色
        /// </summary>
        private void OnChangeFontColorClicked(object sender, RoutedEventArgs args)
        {
            System.Windows.Forms.ColorDialog colorDialog = new()
            {
                Color = System.Drawing.Color.FromName(FontColor)
            };
            if (colorDialog.ShowDialog() is System.Windows.Forms.DialogResult.OK && !Equals(colorDialog.Color, System.Drawing.Color.Empty))
            {
                FontColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
            colorDialog.Dispose();
        }

        /// <summary>
        /// 字体边框风格菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFontBorderStyleSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry fontBorderStyle && !Equals(SelectedFontBorderStyle, fontBorderStyle))
            {
                SelectedFontBorderStyle = fontBorderStyle;
            }
        }

        /// <summary>
        /// 轮廓线大小菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnCounterLineSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry counterLineSize && !Equals(SelectedCounterLineSize, counterLineSize))
            {
                SelectedCounterLineSize = counterLineSize;
            }
        }

        /// <summary>
        /// 修改轮廓线颜色
        /// </summary>
        private void OnChangeCounterLineColorClicked(object sender, RoutedEventArgs args)
        {
            System.Windows.Forms.ColorDialog colorDialog = new()
            {
                Color = System.Drawing.Color.FromName(CounterLineColor)
            };
            if (colorDialog.ShowDialog() is System.Windows.Forms.DialogResult.OK && !Equals(colorDialog.Color, System.Drawing.Color.Empty))
            {
                CounterLineColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
            colorDialog.Dispose();
        }

        /// <summary>
        /// 阴影大小菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnShadowSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry shadowSize && !Equals(SelectedShadowSize, shadowSize))
            {
                SelectedShadowSize = shadowSize;
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

        #endregion 第七部分：窗口过程

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(VideoFormatConversionModel videoFormatConversion = null)
        {
            SelectedFormatConversionType = videoFormatConversion is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.FormatConversionType)) is DictionaryEntry selectedFormatConversionType && selectedFormatConversionType.Key is not null ? selectedFormatConversionType : FormatConversionTypeList[0];

            ResetVideoEncoding();
            SelectedVideoEncoding = videoFormatConversion is not null && VideoEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.VideoEncoding)) is DictionaryEntry selectedVideoEncoding && selectedVideoEncoding.Key is not null ? selectedVideoEncoding : VideoEncodingCollection[0];

            ResetSizeLimitation();
            SelectedSizeLimitation = videoFormatConversion is not null && SizeLimitationCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.SizeLimitation)) is DictionaryEntry selectedSizeLimitation && selectedSizeLimitation.Key is not null ? selectedSizeLimitation : SizeLimitationCollection[0];

            ResetScreenSize();
            SelectedScreenSize = videoFormatConversion is not null && ScreenSizeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.ScreenSize)) is DictionaryEntry selectedScreenSize && selectedScreenSize.Key is not null ? selectedScreenSize : ScreenSizeCollection[0];

            ResetVideoBitRate();
            SelectedVideoBitRate = videoFormatConversion is not null && VideoBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.VideoBitRate)) is DictionaryEntry selectedVideoBitRate && selectedVideoBitRate.Key is not null ? selectedVideoBitRate : VideoBitRateCollection[0];

            ResetCRF();
            if (IsCRFSupported && videoFormatConversion is not null)
            {
                UseCRF = videoFormatConversion.CRF is not -1;
                CRF = videoFormatConversion.CRF is not -1 ? videoFormatConversion.CRF : 10;
            }

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

            GPUList.Add(new DictionaryEntry() { Key = "None", Value = NoneString });

            if (dxgiAdapterList.Contains(32902))
            {
                GPUList.Add(new DictionaryEntry() { Key = "INTEL", Value = "INTEL" });
            }

            if (dxgiAdapterList.Contains(4318))
            {
                GPUList.Add(new DictionaryEntry() { Key = "NVIDIA", Value = "NVIDIA" });
            }

            if (dxgiAdapterList.Contains(4098))
            {
                GPUList.Add(new DictionaryEntry() { Key = "AMD", Value = "AMD" });
            }

            ResetGPU();
            SelectedGPU = videoFormatConversion is not null && GPUCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.GPU)) is DictionaryEntry selectedGPU && selectedGPU.Key is not null ? selectedGPU : GPUCollection[0];

            FramePerSecondList.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "12", Value = "12" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "15", Value = "15" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "18", Value = "18" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "20", Value = "20" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "23.976", Value = "23.976" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "24", Value = "24" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "25", Value = "25" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "29.97", Value = "29.97" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "30", Value = "30" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "50", Value = "50" });
            FramePerSecondList.Add(new DictionaryEntry() { Key = "60", Value = "60" });
            SelectedFramePerSecond = videoFormatConversion is not null && FramePerSecondList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.FramePerSecond)) is DictionaryEntry selectedFramePerSecond && selectedFramePerSecond.Key is not null ? selectedFramePerSecond : FramePerSecondList[0];

            ResetAspectRatio();
            SelectedAspectRatio = videoFormatConversion is not null && AspectRatioCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.AspectRatio)) is DictionaryEntry selectedAspectRatio && selectedAspectRatio.Key is not null ? selectedAspectRatio : AspectRatioCollection[0];

            ResetSecondaryEncoding();
            if (IsSecondaryEncodingEnabled && videoFormatConversion is not null)
            {
                SecondaryEncoding = videoFormatConversion.SecondaryEncoding;
            }

            ResetKeyFrameInterval();
            SelectedKeyFrameInterval = videoFormatConversion is not null && KeyFrameIntervalCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.KeyFrameInterval)) is DictionaryEntry selectedKeyFrameInterval && selectedKeyFrameInterval.Key is not null ? selectedKeyFrameInterval : KeyFrameIntervalCollection[0];

            if (videoFormatConversion is not null)
            {
                DeInterlace = videoFormatConversion.DeInterlace;
            }

            RotationList.Add(new DictionaryEntry() { Key = Rotation.Rotate0, Value = NoRotateString });
            RotationList.Add(new DictionaryEntry() { Key = Rotation.Rotate90, Value = RotateRightString });
            RotationList.Add(new DictionaryEntry() { Key = Rotation.Rotate180, Value = UnsideDownString });
            RotationList.Add(new DictionaryEntry() { Key = Rotation.Rotate270, Value = RotateLeftString });
            SelectedRotation = videoFormatConversion is not null && RotationList.Find(item => Equals((Rotation)item.Key, videoFormatConversion.Rotation)) is DictionaryEntry selectedRotation && selectedRotation.Key is not null ? selectedRotation : RotationList[0];

            if (videoFormatConversion is not null)
            {
                MirrorReversal = videoFormatConversion.MirrorReversal;
            }

            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "1", Value = "1" + SecondString });
            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "2", Value = "2" + SecondString });
            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "3", Value = "3" + SecondString });
            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "4", Value = "4" + SecondString });
            VideoFadeInEffectList.Add(new DictionaryEntry() { Key = "5", Value = "5" + SecondString });
            SelectedVideoFadeInEffect = videoFormatConversion is not null && VideoFadeInEffectList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.VideoFadeInEffect)) is DictionaryEntry selectedVideoFadeInEffect && selectedVideoFadeInEffect.Key is not null ? selectedVideoFadeInEffect : VideoFadeInEffectList[0];

            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "1", Value = "1" + SecondString });
            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "2", Value = "2" + SecondString });
            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "3", Value = "3" + SecondString });
            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "4", Value = "4" + SecondString });
            VideoFadeOutEffectList.Add(new DictionaryEntry() { Key = "5", Value = "5" + SecondString });

            SelectedVideoFadeOutEffect = videoFormatConversion is not null && VideoFadeOutEffectList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.VideoFadeOutEffect)) is DictionaryEntry selectedVideoFadeOutEffect && selectedVideoFadeOutEffect.Key is not null ? selectedVideoFadeOutEffect : VideoFadeOutEffectList[0];

            ResetAudioEncoding();
            SelectedAudioEncoding = videoFormatConversion is not null && AudioEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.AudioEncoding)) is DictionaryEntry selectedAudioEncoding && selectedAudioEncoding.Key is not null ? selectedAudioEncoding : AudioEncodingCollection[0];

            ResetSamplingRate();
            SelectedSamplingRate = videoFormatConversion is not null && SamplingRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.SamplingRate)) is DictionaryEntry selectedSamplingRate && selectedSamplingRate.Key is not null ? selectedSamplingRate : SamplingRateCollection[0];

            ResetAudioBitRate();
            SelectedAudioBitRate = videoFormatConversion is not null && AudioBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.AudioBitRate)) is DictionaryEntry selectedAudioBitRate && selectedAudioBitRate.Key is not null ? selectedAudioBitRate : AudioBitRateCollection[0];

            ResetSoundTrack();
            SelectedSoundTrack = videoFormatConversion is not null && SoundTrackCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.SoundTrack)) is DictionaryEntry selectedSoundTrack && selectedSoundTrack.Key is not null ? selectedSoundTrack : SoundTrackCollection[0];

            if (videoFormatConversion is not null)
            {
                CloseSoundEffect = videoFormatConversion.CloseSoundEffect;
            }

            ResetVolume();
            SelectedVolume = videoFormatConversion is not null && VolumeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.Volume)) is DictionaryEntry selectedVolume && selectedVolume.Key is not null ? selectedVolume : VolumeCollection[5];

            if (videoFormatConversion is not null)
            {
                PreserveAllSourceInputAudioStream = videoFormatConversion.PreserveAllSourceInputAudioStream;
            }

            ResetAudioFadeInEffect();
            SelectedAudioFadeInEffect = videoFormatConversion is not null && AudioFadeInEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.AudioFadeInEffect)) is DictionaryEntry selectedAudioFadeInEffect && selectedAudioFadeInEffect.Key is not null ? selectedAudioFadeInEffect : AudioFadeInEffectCollection[0];

            ResetAudioFadeOutEffect();
            SelectedAudioFadeOutEffect = videoFormatConversion is not null && AudioFadeOutEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.AudioFadeOutEffect)) is DictionaryEntry selectedAudioFadeOutEffect && selectedAudioFadeOutEffect.Key is not null ? selectedAudioFadeOutEffect : AudioFadeOutEffectCollection[0];

            if (videoFormatConversion is not null)
            {
                Echo = videoFormatConversion.Echo;
                DeNoise = videoFormatConversion.DeNoise;
                Reverse = videoFormatConversion.Reverse;
            }

            ResetPreserveAllSourceInputSubtitleStream();
            if (videoFormatConversion is not null)
            {
                if (IsPreserveAllSourceInputSubtitleStreamEnabled)
                {
                    PreserveAllSourceInputSubtitleStream = videoFormatConversion.PreserveAllSourceInputSubtitleStream;
                }

                AdditionalSubtitlePath = videoFormatConversion.AdditionalSubtitlePath;
            }

            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "Embedded", Value = "Embedded" });
            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "Ansi", Value = "Ansi" });
            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "Unicode", Value = "Unicode" });
            SubtitleNestTypeList.Add(new DictionaryEntry() { Key = "UTF8", Value = "UTF8" });
            SelectedSubtitleNestType = videoFormatConversion is not null && SubtitleNestTypeList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.SubtitleNestType)) is DictionaryEntry selectedSubtitleNestType && selectedSubtitleNestType.Key is not null ? selectedSubtitleNestType : SubtitleNestTypeList[0];

            FontName = videoFormatConversion is not null && !string.IsNullOrEmpty(videoFormatConversion.FontName) ? videoFormatConversion.FontName : FontName = System.Drawing.SystemFonts.DefaultFont.Name;

            FontSizeList.Add(new DictionaryEntry() { Key = 1, Value = string.Format("{0} {1}", 1, SmallString) });
            FontSizeList.Add(new DictionaryEntry() { Key = 2, Value = "2" });
            FontSizeList.Add(new DictionaryEntry() { Key = 3, Value = string.Format("{0} {1}", 3, NormalString) });
            FontSizeList.Add(new DictionaryEntry() { Key = 4, Value = "4" });
            FontSizeList.Add(new DictionaryEntry() { Key = 5, Value = string.Format("{0} {1}", 5, LargeString) });
            SelectedFontSize = videoFormatConversion is not null && FontSizeList.Find(item => Equals(Convert.ToInt32(item.Key), videoFormatConversion.FontSize)) is DictionaryEntry selectedFontSize && selectedFontSize.Key is not null ? selectedFontSize : FontSizeList[0];

            System.Windows.Media.Color accentColor = System.Windows.SystemParameters.WindowGlassColor;
            FontColor = videoFormatConversion is not null && !string.IsNullOrEmpty(videoFormatConversion.FontColor) ? videoFormatConversion.FontColor : accentColor.ToString();

            FontBorderStyleList.Add(new DictionaryEntry() { Key = "BorderAndShadow", Value = BorderAndShadowString });
            FontBorderStyleList.Add(new DictionaryEntry() { Key = "SolidColorBackground", Value = SolidColorBackgroundString });
            SelectedFontBorderStyle = videoFormatConversion is not null && FontBorderStyleList.Find(item => string.Equals(Convert.ToString(item.Key), videoFormatConversion.FontBorderStyle)) is DictionaryEntry selectedFontBorderStyle && selectedFontBorderStyle.Key is not null ? selectedFontBorderStyle : FontBorderStyleList[0];

            CounterLineSizeList.Add(new DictionaryEntry() { Key = 0, Value = "0" });
            CounterLineSizeList.Add(new DictionaryEntry() { Key = 1, Value = "1" });
            CounterLineSizeList.Add(new DictionaryEntry() { Key = 2, Value = "2" });
            CounterLineSizeList.Add(new DictionaryEntry() { Key = 3, Value = "3" });
            CounterLineSizeList.Add(new DictionaryEntry() { Key = 4, Value = "4" });
            SelectedCounterLineSize = videoFormatConversion is not null && CounterLineSizeList.Find(item => Equals(Convert.ToInt32(item.Key), videoFormatConversion.CounterLineSize)) is DictionaryEntry selectedCounterLineSize && selectedCounterLineSize.Key is not null ? selectedCounterLineSize : CounterLineSizeList[0];

            CounterLineColor = videoFormatConversion is not null && !string.IsNullOrEmpty(videoFormatConversion.CounterLineColor) ? videoFormatConversion.CounterLineColor : accentColor.ToString();

            ShadowSizeList.Add(new DictionaryEntry() { Key = 0, Value = "0" });
            ShadowSizeList.Add(new DictionaryEntry() { Key = 1, Value = "1" });
            ShadowSizeList.Add(new DictionaryEntry() { Key = 2, Value = "2" });
            ShadowSizeList.Add(new DictionaryEntry() { Key = 3, Value = "3" });
            ShadowSizeList.Add(new DictionaryEntry() { Key = 4, Value = "4" });
            SelectedShadowSize = videoFormatConversion is not null && ShadowSizeList.Find(item => Equals(Convert.ToInt32(item.Key), videoFormatConversion.ShadowSize)) is DictionaryEntry selectedShadowSize && selectedShadowSize.Key is not null ? selectedShadowSize : ShadowSizeList[0];
        }

        /// <summary>
        /// 重置视频编码列表选项
        /// </summary>
        private void ResetVideoEncoding()
        {
            VideoEncodingCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "VVC", Value = "VVC(H266)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AV1", Value = "AV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "VP9", Value = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "GIF", Value = "GIF" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AV1", Value = "AV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "VP8", Value = "VP8" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "VP9", Value = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MSMPEG4V2", Value = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MSMPEG4V3", Value = "MSMPEG4V3" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "WMV2", Value = "WMV2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "FLV1", Value = "FLV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG2", Value = "MPEG2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MJPEG", Value = "MJPEG" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[5]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "FLV1", Value = "FLV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AV1", Value = "AV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "VP9", Value = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[6]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[8]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MSMPEG4V2", Value = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "WMV2", Value = "WMV2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "H263", Value = "H263" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG2", Value = "MPEG2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG2", Value = "MPEG2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[13]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "Theora", Value = "Theora" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "HEVC", Value = "HEVC(H265)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "AVC", Value = "AVC(H264)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_DivX", Value = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG4_Xvid", Value = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MSMPEG4V2", Value = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MSMPEG4V3", Value = "MSMPEG4V3" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "WMV2", Value = "WMV2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "FLV1", Value = "FLV1" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MPEG2", Value = "MPEG2" });
                VideoEncodingCollection.Add(new DictionaryEntry() { Key = "MJPEG", Value = "MJPEG" });
            }
        }

        /// <summary>
        /// 重置大小限制
        /// </summary>
        private void ResetSizeLimitation()
        {
            SizeLimitationCollection.Clear();

            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.Key);

            if (string.IsNullOrEmpty(selectedVideoEncoding))
            {
                SizeLimitationCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
            else
            {
                if (string.Equals(selectedVideoEncoding, "Copy"))
                {
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
                }
                else
                {
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "10", Value = "10MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "15", Value = "15MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "20", Value = "20MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "25", Value = "25MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "30", Value = "30MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "35", Value = "35MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "40", Value = "40MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "50", Value = "50MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "60", Value = "60MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "70", Value = "70MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "80", Value = "80MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "90", Value = "90MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "100", Value = "100MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "150", Value = "150MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "200", Value = "200MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "300", Value = "300MB" });
                    SizeLimitationCollection.Add(new DictionaryEntry() { Key = "500", Value = "500MB" });
                }
            }
        }

        /// <summary>
        /// 重置屏幕大小选项
        /// </summary>
        private void ResetScreenSize()
        {
            ScreenSizeCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]))
            {
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "DefaultSize", Value = DefaultSizeString });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "360P", Value = "360p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "480P", Value = "480p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "720P", Value = "720p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "1080P", Value = "1080p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "2160P", Value = "2160p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "480I", Value = "480i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "720I", Value = "720i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "1280I", Value = "1280i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "1920I", Value = "1920i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "3840I", Value = "3840i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "25%", Value = "25%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "50%", Value = "50%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "75%", Value = "75%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "125%", Value = "125%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "150%", Value = "150%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "200%", Value = "200%" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "Custom", Value = CustomString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "96P", Value = "96p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "144P", Value = "144p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "240P", Value = "240p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "480P", Value = "480p" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "320I", Value = "320i" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "640I", Value = "640i" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "176×144", Value = "176×144" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "352×288", Value = "352×288" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "128×96", Value = "128×96" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "176×144", Value = "176×144" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "320×240", Value = "320×240" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "352×288", Value = "352×288" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "400×240", Value = "400×240" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "480×320", Value = "480×320" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "176×144", Value = "176×144" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "320×240", Value = "320×240" });
                ScreenSizeCollection.Add(new DictionaryEntry() { Key = "640×480", Value = "640×480" });
            }
        }

        /// <summary>
        /// 重置视频分辨率选项
        /// </summary>
        private void ResetVideoBitRate()
        {
            VideoBitRateCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "256K", Value = "256K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "384K", Value = "384K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "512K", Value = "512K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "768K", Value = "768K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "1M", Value = "1M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "1.5M", Value = "1.5M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "2M", Value = "2M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "5M", Value = "5M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "10M", Value = "10M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "15M", Value = "15M" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "20M", Value = "20M" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "96K", Value = "96K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "128K", Value = "128K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "160K", Value = "160K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "192K", Value = "192K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "256K", Value = "256K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "384K", Value = "384K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "128K", Value = "128K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "160K", Value = "160K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "192K", Value = "192K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "256K", Value = "256K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "384K", Value = "384K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "512K", Value = "512K" });
                VideoBitRateCollection.Add(new DictionaryEntry() { Key = "768K", Value = "768K" });
            }
        }

        /// <summary>
        /// 重置固定速率系数
        /// </summary>
        private void ResetCRF()
        {
            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.Key);
            if (string.Equals(Convert.ToString(selectedVideoEncoding), "HEVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AV1"))
            {
                IsCRFSupported = true;
            }
            else
            {
                IsCRFSupported = false;
                UseCRF = false;
                if (!UseCRF)
                {
                    CRF = 10;
                }
            }
        }

        /// <summary>
        /// 重置 GPU
        /// </summary>
        private void ResetGPU()
        {
            GPUCollection.Clear();

            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.Key);
            if (string.Equals(Convert.ToString(selectedVideoEncoding), "HEVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AV1"))
            {
                foreach (DictionaryEntry gpu in GPUList)
                {
                    GPUCollection.Add(gpu);
                }
            }
            else
            {
                GPUCollection.Add(GPUList[0]);
            }
        }

        /// <summary>
        /// 重置宽高比
        /// </summary>
        private void ResetAspectRatio()
        {
            AspectRatioCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "4:3", Value = "4:3" });
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "16:9", Value = "16:9" });
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "3:2", Value = "3:2" });
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "5:4", Value = "5:4" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AspectRatioCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
            }
        }

        /// <summary>
        /// 重置二次编码
        /// </summary>
        private void ResetSecondaryEncoding()
        {
            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.Key);

            if (string.Equals(selectedVideoEncoding, "Copy"))
            {
                IsSecondaryEncodingEnabled = false;
                SecondaryEncoding = false;
            }
            else
            {
                IsSecondaryEncodingEnabled = true;
            }
        }

        /// <summary>
        /// 重置关键帧间隔
        /// </summary>
        private void ResetKeyFrameInterval()
        {
            KeyFrameIntervalCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "1", Value = "1" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "2", Value = "2" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "3", Value = "3" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "4", Value = "4" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "8", Value = "5" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "6", Value = "6" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "7", Value = "7" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "8", Value = "8" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "9", Value = "9" });
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "10", Value = "10" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                KeyFrameIntervalCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
            }
        }

        /// <summary>
        /// 重置音频编码
        /// </summary>
        private void ResetAudioEncoding()
        {
            AudioEncodingCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AAC", Value = "AAC" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AC3", Value = "AC3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AAC", Value = "AAC" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AC3", Value = "AC3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3", Value = "MP3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3-VBR", Value = "MP3-VBR" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "OPUS", Value = "OPUS" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Vorbis", Value = "Vorbis" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AC3", Value = "AC3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP2", Value = "MP2" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3", Value = "MP3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3-VBR", Value = "MP3-VBR" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "WMAv2", Value = "WMAv2" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "PCM", Value = "PCM" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AAC", Value = "AAC" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3", Value = "MP3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[8]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "WMAv2", Value = "WMAv2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AMR_NB", Value = "AMR_NB" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AMR_WB", Value = "AMR_WB" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AMR_NB", Value = "AMR_NB" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AMR_WB", Value = "AMR_WB" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AAC", Value = "AAC" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AC3", Value = "AC3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[13]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Vorbis", Value = "Vorbis" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "Copy", Value = CopyString });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "AC3", Value = "AC3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP2", Value = "MP2" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3", Value = "MP3" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "MP3-VBR", Value = "MP3-VBR" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "WMAv2", Value = "WMAv2" });
                AudioEncodingCollection.Add(new DictionaryEntry() { Key = "PCM", Value = "PCM" });
            }
        }

        /// <summary>
        /// 重置采样率
        /// </summary>
        private void ResetSamplingRate()
        {
            SamplingRateCollection.Clear();

            string selectedAudioEncoding = Convert.ToString(SelectedAudioEncoding.Key);
            if (string.Equals(selectedAudioEncoding, "Copy") ||
               string.Equals(selectedAudioEncoding, "AAC") ||
               string.Equals(selectedAudioEncoding, "AC3") ||
               string.Equals(selectedAudioEncoding, "MP2") ||
               string.Equals(selectedAudioEncoding, "MP3") ||
               string.Equals(selectedAudioEncoding, "MP3-VBR") ||
               string.Equals(selectedAudioEncoding, "AC3") ||
               string.Equals(selectedAudioEncoding, "WMAv2") ||
               string.Equals(selectedAudioEncoding, "PCM"))
            {
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "22050", Value = "22050" });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "24000", Value = "24000" });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "44100", Value = "44100" });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "48000", Value = "48000" });
            }
            else if (string.Equals(selectedAudioEncoding, "OPUS") || string.Equals(selectedAudioEncoding, "Vorbis"))
            {
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "24000", Value = "24000" });
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "48000", Value = "48000" });
            }
            else if (string.Equals(selectedAudioEncoding, "AMR_NB") || string.Equals(selectedAudioEncoding, "AMR_WB"))
            {
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "8000", Value = "8000" });
            }
            else if (string.Equals(selectedAudioEncoding, "None"))
            {
                SamplingRateCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
        }

        /// <summary>
        /// 重置音频比特率
        /// </summary>
        private void ResetAudioBitRate()
        {
            AudioBitRateCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "24K", Value = "24K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "32K", Value = "32K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "64K", Value = "64K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "128K", Value = "128K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "192K", Value = "192K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "224K", Value = "224K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "256K", Value = "256K" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "320K", Value = "320K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "12.20", Value = "12.20" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "10.20", Value = "10.20" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "7.40", Value = "7.40" });
                AudioBitRateCollection.Add(new DictionaryEntry() { Key = "4.75", Value = "4.75" });
            }
        }

        /// <summary>
        /// 重置声道
        /// </summary>
        private void ResetSoundTrack()
        {
            SoundTrackCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "Default", Value = DefaultString });
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "1", Value = string.Format("{0} {1}", 1, MonoString) });
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "2", Value = string.Format("{0} {1}", 2, StereoString) });
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "4", Value = string.Format("{0} {1}", 4, QuadString) });
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "6", Value = string.Format("{0} {1}", 6, Stereo51String) });
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "8", Value = string.Format("{0} {1}", 8, Stereo71String) });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                SoundTrackCollection.Add(new DictionaryEntry() { Key = "1", Value = string.Format("{0} {1}", 1, MonoString) });
            }
        }

        /// <summary>
        /// 重置音量
        /// </summary>
        private void ResetVolume()
        {
            VolumeCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                VolumeCollection.Add(new DictionaryEntry() { Key = "10%", Value = "10%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "25%", Value = "25%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "50%", Value = "50%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "75%", Value = "75%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "100%", Value = "100%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "150%", Value = "150%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "200%", Value = "200%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "300%", Value = "300%" });
                VolumeCollection.Add(new DictionaryEntry() { Key = "400%", Value = "400%" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VolumeCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
        }

        /// <summary>
        /// 重置音频淡入效果
        /// </summary>
        private void ResetAudioFadeInEffect()
        {
            AudioFadeInEffectCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "1", Value = "1" + SecondString });
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "2", Value = "2" + SecondString });
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "3", Value = "3" + SecondString });
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "4", Value = "4" + SecondString });
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "5", Value = "5" + SecondString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioFadeInEffectCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
        }

        /// <summary>
        /// 重置音量
        /// </summary>
        private void ResetAudioFadeOutEffect()
        {
            AudioFadeOutEffectCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "1", Value = "1" + SecondString });
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "2", Value = "2" + SecondString });
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "3", Value = "3" + SecondString });
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "4", Value = "4" + SecondString });
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "5", Value = "5" + SecondString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioFadeOutEffectCollection.Add(new DictionaryEntry() { Key = "None", Value = NoneString });
            }
        }

        /// <summary>
        /// 重置保留所有字幕源输入流
        /// </summary>
        private void ResetPreserveAllSourceInputSubtitleStream()
        {
            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[13]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                IsPreserveAllSourceInputSubtitleStreamEnabled = false;
                PreserveAllSourceInputSubtitleStream = false;
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                IsPreserveAllSourceInputSubtitleStreamEnabled = true;
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
            SelectedItem = VideoFormatConversionSelectorBar.Items[0];

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
        /// 获取是否选中了自定义屏幕项
        /// </summary>
        private Visibility GetIsCustomScreenSizeSelected(DictionaryEntry selectedScreenSize)
        {
            return string.Equals(Convert.ToString(selectedScreenSize.Key), "Custom") ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility CheckAdditionalSubtitlePath(string additionalSubtitlePath)
        {
            return string.IsNullOrEmpty(additionalSubtitlePath) ? Visibility.Collapsed : Visibility.Visible;
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
