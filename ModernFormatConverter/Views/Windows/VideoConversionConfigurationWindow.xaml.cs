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
    /// 视频转换配置窗口
    /// </summary>
    public sealed partial class VideoConversionConfigurationWindow : Window, INotifyPropertyChanged
    {
        private readonly string AllFilesString = ResourceService.VideoConversionConfigurationResource.GetString("AllFiles");
        private readonly string BorderAndShadowString = ResourceService.VideoConversionConfigurationResource.GetString("BorderAndShadow");
        private readonly string CopyString = ResourceService.VideoConversionConfigurationResource.GetString("Copy");
        private readonly string CustomString = ResourceService.VideoConversionConfigurationResource.GetString("Custom");
        private readonly string DefaultString = ResourceService.VideoConversionConfigurationResource.GetString("Default");
        private readonly string DefaultSizeString = ResourceService.VideoConversionConfigurationResource.GetString("DefaultSize");
        private readonly string LargeString = ResourceService.VideoConversionConfigurationResource.GetString("Large");
        private readonly string MonoString = ResourceService.VideoConversionConfigurationResource.GetString("Mono");
        private readonly string NoneString = ResourceService.VideoConversionConfigurationResource.GetString("None");
        private readonly string NormalString = ResourceService.VideoConversionConfigurationResource.GetString("Normal");
        private readonly string NoRotateString = ResourceService.VideoConversionConfigurationResource.GetString("NoRotate");
        private readonly string QuadString = ResourceService.VideoConversionConfigurationResource.GetString("Quad");
        private readonly string RotateLeftString = ResourceService.VideoConversionConfigurationResource.GetString("RotateLeft");
        private readonly string RotateRightString = ResourceService.VideoConversionConfigurationResource.GetString("RotateRight");
        private readonly string SecondString = ResourceService.VideoConversionConfigurationResource.GetString("Second");
        private readonly string SelectFileString = ResourceService.VideoConversionConfigurationResource.GetString("SelectFile");
        private readonly string SmallString = ResourceService.VideoConversionConfigurationResource.GetString("Small");
        private readonly string SolidColorBackgroundString = ResourceService.VideoConversionConfigurationResource.GetString("SolidColorBackground");
        private readonly string StereoString = ResourceService.VideoConversionConfigurationResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.VideoConversionConfigurationResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.VideoConversionConfigurationResource.GetString("Stereo71");
        private readonly string SubtitleString = ResourceService.VideoConversionConfigurationResource.GetString("Subtitle");
        private readonly string UnsideDownString = ResourceService.VideoConversionConfigurationResource.GetString("UnsideDown");
        private readonly List<ComboBoxItemModel> GPUList = [];
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC videoConversionConfigurationWindowSubClassProc;
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
                if (!Equals(_windowSystemBackdrop, value))
                {
                    _windowSystemBackdrop = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowSystemBackdrop)));
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

        private SelectorBarItem _selectedItem;

        public SelectorBarItem SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
                }
            }
        }

        private VideoConversionTypeKind _selectedVideoConversionTypeKind;

        public VideoConversionTypeKind SelectedVideoConversionTypeKind
        {
            get { return _selectedVideoConversionTypeKind; }

            set
            {
                if (!Equals(_selectedVideoConversionTypeKind, value))
                {
                    _selectedVideoConversionTypeKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoConversionTypeKind)));
                }
            }
        }

        private ComboBoxItemModel _selectedFormatConversionType;

        public ComboBoxItemModel SelectedFormatConversionType
        {
            get { return _selectedFormatConversionType; }

            set
            {
                if (!Equals(_selectedFormatConversionType, value))
                {
                    _selectedFormatConversionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormatConversionType)));
                }
            }
        }

        private ComboBoxItemModel _selectedSizeLimitation;

        public ComboBoxItemModel SelectedSizeLimitation
        {
            get { return _selectedSizeLimitation; }

            set
            {
                if (!Equals(_selectedSizeLimitation, value))
                {
                    _selectedSizeLimitation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSizeLimitation)));
                }
            }
        }

        private ComboBoxItemModel _selectedVideoEncoding;

        public ComboBoxItemModel SelectedVideoEncoding
        {
            get { return _selectedVideoEncoding; }

            set
            {
                if (!Equals(_selectedVideoEncoding, value))
                {
                    _selectedVideoEncoding = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoEncoding)));
                }
            }
        }

        private ComboBoxItemModel _selectedScreenSize;

        public ComboBoxItemModel SelectedScreenSize
        {
            get { return _selectedScreenSize; }

            set
            {
                if (!Equals(_selectedScreenSize, value))
                {
                    _selectedScreenSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedScreenSize)));
                }
            }
        }

        private int _screenWidth;

        public int ScreenWidth
        {
            get { return _screenWidth; }

            set
            {
                if (!Equals(_screenWidth, value))
                {
                    _screenWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenWidth)));
                }
            }
        }

        private int _screenHeight;

        public int ScreenHeight
        {
            get { return _screenHeight; }

            set
            {
                if (!Equals(_screenHeight, value))
                {
                    _screenHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenHeight)));
                }
            }
        }

        private ComboBoxItemModel _selectedVideoBitRate;

        public ComboBoxItemModel SelectedVideoBitRate
        {
            get { return _selectedVideoBitRate; }

            set
            {
                if (!Equals(_selectedVideoBitRate, value))
                {
                    _selectedVideoBitRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoBitRate)));
                }
            }
        }

        private bool _isCRFSupported;

        public bool IsCRFSupported
        {
            get { return _isCRFSupported; }

            set
            {
                if (!Equals(_isCRFSupported, value))
                {
                    _isCRFSupported = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCRFSupported)));
                }
            }
        }

        private bool _useCRF;

        public bool UseCRF
        {
            get { return _useCRF; }

            set
            {
                if (!Equals(_useCRF, value))
                {
                    _useCRF = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseCRF)));
                }
            }
        }

        private int _crf = 10;

        public int CRF
        {
            get { return _crf; }

            set
            {
                if (!Equals(_crf, value))
                {
                    _crf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CRF)));
                }
            }
        }

        private ComboBoxItemModel _selectedGPU;

        public ComboBoxItemModel SelectedGPU
        {
            get { return _selectedGPU; }

            set
            {
                if (!Equals(_selectedGPU, value))
                {
                    _selectedGPU = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGPU)));
                }
            }
        }

        private ComboBoxItemModel _selectedFramePerSecond;

        public ComboBoxItemModel SelectedFramePerSecond
        {
            get { return _selectedFramePerSecond; }

            set
            {
                if (!Equals(_selectedFramePerSecond, value))
                {
                    _selectedFramePerSecond = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFramePerSecond)));
                }
            }
        }

        private ComboBoxItemModel _selectedAspectRatio;

        public ComboBoxItemModel SelectedAspectRatio
        {
            get { return _selectedAspectRatio; }

            set
            {
                if (!Equals(_selectedAspectRatio, value))
                {
                    _selectedAspectRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAspectRatio)));
                }
            }
        }

        private bool _isSecondaryEncodingEnabled;

        public bool IsSecondaryEncodingEnabled
        {
            get { return _isSecondaryEncodingEnabled; }

            set
            {
                if (!Equals(_isSecondaryEncodingEnabled, value))
                {
                    _isSecondaryEncodingEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSecondaryEncodingEnabled)));
                }
            }
        }

        private bool _secondaryEncoding;

        public bool SecondaryEncoding
        {
            get { return _secondaryEncoding; }

            set
            {
                if (!Equals(_secondaryEncoding, value))
                {
                    _secondaryEncoding = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondaryEncoding)));
                }
            }
        }

        private ComboBoxItemModel _selectedKeyFrameInterval;

        public ComboBoxItemModel SelectedKeyFrameInterval
        {
            get { return _selectedKeyFrameInterval; }

            set
            {
                if (!Equals(_selectedKeyFrameInterval, value))
                {
                    _selectedKeyFrameInterval = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrameInterval)));
                }
            }
        }

        private bool _deInterlace;

        public bool DeInterlace
        {
            get { return _deInterlace; }

            set
            {
                if (!Equals(_deInterlace, value))
                {
                    _deInterlace = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeInterlace)));
                }
            }
        }

        private ComboBoxItemModel _selectedRotation;

        public ComboBoxItemModel SelectedRotation
        {
            get { return _selectedRotation; }

            set
            {
                if (!Equals(_selectedRotation, value))
                {
                    _selectedRotation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRotation)));
                }
            }
        }

        private bool _mirrorReversal;

        public bool MirrorReversal
        {
            get { return _mirrorReversal; }

            set
            {
                if (!Equals(_mirrorReversal, value))
                {
                    _mirrorReversal = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MirrorReversal)));
                }
            }
        }

        private ComboBoxItemModel _selectedVideoFadeInEffect;

        public ComboBoxItemModel SelectedVideoFadeInEffect
        {
            get { return _selectedVideoFadeInEffect; }

            set
            {
                if (!Equals(_selectedVideoFadeInEffect, value))
                {
                    _selectedVideoFadeInEffect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeInEffect)));
                }
            }
        }

        private ComboBoxItemModel _selectedVideoFadeOutEffect;

        public ComboBoxItemModel SelectedVideoFadeOutEffect
        {
            get { return _selectedVideoFadeOutEffect; }

            set
            {
                if (!Equals(_selectedVideoFadeOutEffect, value))
                {
                    _selectedVideoFadeOutEffect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeOutEffect)));
                }
            }
        }

        private bool _isAudioConfigurationSupported;

        public bool IsAudioConfigurationSupported
        {
            get { return _isAudioConfigurationSupported; }

            set
            {
                if (!Equals(_isAudioConfigurationSupported, value))
                {
                    _isAudioConfigurationSupported = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAudioConfigurationSupported)));
                }
            }
        }

        private ComboBoxItemModel _selectedAudioEncoding;

        public ComboBoxItemModel SelectedAudioEncoding
        {
            get { return _selectedAudioEncoding; }

            set
            {
                if (!Equals(_selectedAudioEncoding, value))
                {
                    _selectedAudioEncoding = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioEncoding)));
                }
            }
        }

        private ComboBoxItemModel _selectedSamplingRate;

        public ComboBoxItemModel SelectedSamplingRate
        {
            get { return _selectedSamplingRate; }

            set
            {
                if (!Equals(_selectedSamplingRate, value))
                {
                    _selectedSamplingRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSamplingRate)));
                }
            }
        }

        private ComboBoxItemModel _selectedAudioBitRate;

        public ComboBoxItemModel SelectedAudioBitRate
        {
            get { return _selectedAudioBitRate; }

            set
            {
                if (!Equals(_selectedAudioBitRate, value))
                {
                    _selectedAudioBitRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioBitRate)));
                }
            }
        }

        private ComboBoxItemModel _selectedSoundTrack;

        public ComboBoxItemModel SelectedSoundTrack
        {
            get { return _selectedSoundTrack; }

            set
            {
                if (!Equals(_selectedSoundTrack, value))
                {
                    _selectedSoundTrack = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSoundTrack)));
                }
            }
        }

        private bool _closeSoundEffect;

        public bool CloseSoundEffect
        {
            get { return _closeSoundEffect; }

            set
            {
                if (!Equals(_closeSoundEffect, value))
                {
                    _closeSoundEffect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloseSoundEffect)));
                }
            }
        }

        private ComboBoxItemModel _selectedVolume;

        public ComboBoxItemModel SelectedVolume
        {
            get { return _selectedVolume; }

            set
            {
                if (!Equals(_selectedVolume, value))
                {
                    _selectedVolume = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVolume)));
                }
            }
        }

        private bool _preserveAllSourceInputAudioStream;

        public bool PreserveAllSourceInputAudioStream
        {
            get { return _preserveAllSourceInputAudioStream; }

            set
            {
                if (!Equals(_preserveAllSourceInputAudioStream, value))
                {
                    _preserveAllSourceInputAudioStream = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreserveAllSourceInputAudioStream)));
                }
            }
        }

        private ComboBoxItemModel _selectedAudioFadeInEffect;

        public ComboBoxItemModel SelectedAudioFadeInEffect
        {
            get { return _selectedAudioFadeInEffect; }

            set
            {
                if (!Equals(_selectedAudioFadeInEffect, value))
                {
                    _selectedAudioFadeInEffect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioFadeInEffect)));
                }
            }
        }

        private ComboBoxItemModel _selectedAudioFadeOutEffect;

        public ComboBoxItemModel SelectedAudioFadeOutEffect
        {
            get { return _selectedAudioFadeOutEffect; }

            set
            {
                if (!Equals(_selectedAudioFadeOutEffect, value))
                {
                    _selectedAudioFadeOutEffect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioFadeOutEffect)));
                }
            }
        }

        private bool _echo;

        public bool Echo
        {
            get { return _echo; }

            set
            {
                if (!Equals(_echo, value))
                {
                    _echo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Echo)));
                }
            }
        }

        private bool _deNoise;

        public bool DeNoise
        {
            get { return _deNoise; }

            set
            {
                if (!Equals(_deNoise, value))
                {
                    _deNoise = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeNoise)));
                }
            }
        }

        private bool _reverse;

        public bool Reverse
        {
            get { return _reverse; }

            set
            {
                if (!Equals(_reverse, value))
                {
                    _reverse = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reverse)));
                }
            }
        }

        private bool _isPreserveAllSourceInputSubtitleStreamEnabled;

        public bool IsPreserveAllSourceInputSubtitleStreamEnabled
        {
            get { return _isPreserveAllSourceInputSubtitleStreamEnabled; }

            set
            {
                if (!Equals(_isPreserveAllSourceInputSubtitleStreamEnabled, value))
                {
                    _isPreserveAllSourceInputSubtitleStreamEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPreserveAllSourceInputSubtitleStreamEnabled)));
                }
            }
        }

        private bool _preserveAllSourceInputSubtitleStream;

        public bool PreserveAllSourceInputSubtitleStream
        {
            get { return _preserveAllSourceInputSubtitleStream; }

            set
            {
                if (!Equals(_preserveAllSourceInputSubtitleStream, value))
                {
                    _preserveAllSourceInputSubtitleStream = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreserveAllSourceInputSubtitleStream)));
                }
            }
        }

        private string _AdditionalSubtitlePath;

        public string AdditionalSubtitlePath
        {
            get { return _AdditionalSubtitlePath; }

            set
            {
                if (!string.Equals(_AdditionalSubtitlePath, value))
                {
                    _AdditionalSubtitlePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdditionalSubtitlePath)));
                }
            }
        }

        private string _subtitleNestType;

        public string SubtitleNestType
        {
            get { return _subtitleNestType; }

            set
            {
                if (!string.Equals(_subtitleNestType, value))
                {
                    _subtitleNestType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubtitleNestType)));
                }
            }
        }

        private ComboBoxItemModel _selectedSubtitleNestType;

        public ComboBoxItemModel SelectedSubtitleNestType
        {
            get { return _selectedSubtitleNestType; }

            set
            {
                if (!Equals(_selectedSubtitleNestType, value))
                {
                    _selectedSubtitleNestType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubtitleNestType)));
                }
            }
        }

        private string _fontName;

        public string FontName
        {
            get { return _fontName; }

            set
            {
                if (!string.Equals(_fontName, value))
                {
                    _fontName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontName)));
                }
            }
        }

        private ComboBoxItemModel _selectedFontSize;

        public ComboBoxItemModel SelectedFontSize
        {
            get { return _selectedFontSize; }

            set
            {
                if (!Equals(_selectedFontSize, value))
                {
                    _selectedFontSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontSize)));
                }
            }
        }

        private string _fontColor;

        public string FontColor
        {
            get { return _fontColor; }

            set
            {
                if (!string.Equals(_fontColor, value))
                {
                    _fontColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
                }
            }
        }

        private ComboBoxItemModel _selectedFontBorderStyle;

        public ComboBoxItemModel SelectedFontBorderStyle
        {
            get { return _selectedFontBorderStyle; }

            set
            {
                if (!Equals(_selectedFontBorderStyle, value))
                {
                    _selectedFontBorderStyle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontBorderStyle)));
                }
            }
        }

        private ComboBoxItemModel _selectedCounterLineSize;

        public ComboBoxItemModel SelectedCounterLineSize
        {
            get { return _selectedCounterLineSize; }

            set
            {
                if (!Equals(_selectedCounterLineSize, value))
                {
                    _selectedCounterLineSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCounterLineSize)));
                }
            }
        }

        private string _counterLineColor;

        public string CounterLineColor
        {
            get { return _counterLineColor; }

            set
            {
                if (!string.Equals(_counterLineColor, value))
                {
                    _counterLineColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CounterLineColor)));
                }
            }
        }

        private ComboBoxItemModel _selectedShadowSize;

        public ComboBoxItemModel SelectedShadowSize
        {
            get { return _selectedShadowSize; }

            set
            {
                if (!Equals(_selectedShadowSize, value))
                {
                    _selectedShadowSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedShadowSize)));
                }
            }
        }

        public List<ComboBoxItemModel> FormatConversionTypeList { get; } =
        [
            new ComboBoxItemModel(){ SelectedValue = "MP4", DisplayMember = ".mp4" },
            new ComboBoxItemModel(){ SelectedValue = "MKV", DisplayMember =  ".mkv" },
            new ComboBoxItemModel(){ SelectedValue = "GIF", DisplayMember =  ".gif" },
            new ComboBoxItemModel(){ SelectedValue = "WebM",DisplayMember =  ".webm" },
            new ComboBoxItemModel(){ SelectedValue = "AVI", DisplayMember =  ".avi" },
            new ComboBoxItemModel(){ SelectedValue = "FLV", DisplayMember =  ".flv" },
            new ComboBoxItemModel(){ SelectedValue = "MOV", DisplayMember =  ".mov" },
            new ComboBoxItemModel(){ SelectedValue = "M3U8",DisplayMember =  ".m3u8" },
            new ComboBoxItemModel(){ SelectedValue = "WMV", DisplayMember =  ".wmv" },
            new ComboBoxItemModel(){ SelectedValue = "3GP", DisplayMember =  ".3gp" },
            new ComboBoxItemModel(){ SelectedValue = "3G2", DisplayMember =  ".3g2" },
            new ComboBoxItemModel(){ SelectedValue = "MPG", DisplayMember =  ".mpg" },
            new ComboBoxItemModel(){ SelectedValue = "VOB", DisplayMember =  ".vob" },
            new ComboBoxItemModel(){ SelectedValue = "OGG", DisplayMember =  ".ogg" },
            new ComboBoxItemModel(){ SelectedValue = "SWF", DisplayMember =  ".swf" },
        ];

        public WinRTObservableCollection<ComboBoxItemModel> SizeLimitationCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VideoEncodingCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> ScreenSizeCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VideoBitRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> GPUCollection { get; } = [];

        public List<ComboBoxItemModel> FramePerSecondList { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AspectRatioCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> KeyFrameIntervalCollection { get; } = [];

        public List<ComboBoxItemModel> RotationList { get; } = [];

        public List<ComboBoxItemModel> VideoFadeInEffectList { get; } = [];

        public List<ComboBoxItemModel> VideoFadeOutEffectList { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioEncodingCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SamplingRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioBitRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SoundTrackCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VolumeCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioFadeInEffectCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioFadeOutEffectCollection { get; } = [];

        public List<ComboBoxItemModel> SubtitleNestTypeList { get; } = [];

        public List<ComboBoxItemModel> FontSizeList { get; } = [];

        public List<ComboBoxItemModel> FontBorderStyleList { get; } = [];

        public List<ComboBoxItemModel> CounterLineSizeList { get; } = [];

        public List<ComboBoxItemModel> ShadowSizeList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoConversionConfigurationWindow(VideoConversionTypeKind videoConversionTypeKind, ConversionToolsWindow conversionToolsWindow, VideoConversionConfigurationModel videoConversionConfiguration = null)
        {
            SelectedVideoConversionTypeKind = videoConversionTypeKind;
            InitializeData(videoConversionConfiguration);
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
            if (VideoConversionConfigurationScroll.IsLoaded && !Equals(SelectedItem, sender.SelectedItem))
            {
                SelectedItem = sender.SelectedItem;
                int index = sender.Items.IndexOf(SelectedItem);

                switch (index)
                {
                    case 0:
                        {
                            double currentScrollPosition = VideoConversionConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = VideoHeader.TransformToVisual(VideoConversionConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionConfigurationScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 1:
                        {
                            double currentScrollPosition = VideoConversionConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = AudioHeader.TransformToVisual(VideoConversionConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionConfigurationScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 2:
                        {
                            double currentScrollPosition = VideoConversionConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = SubtitleHeader.TransformToVisual(VideoConversionConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionConfigurationScroll.ChangeView(null, targetPosition.Y, null);
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
            if (!taskCompletionSource.Task.IsCompleted)
            {
                taskCompletionSource.TrySetResult(ContentDialogResult.Primary);
            }
            Close();
        }

        /// <summary>
        /// 滚动列表视图发生变化时触发的事件
        /// </summary>
        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
        {
            double currentScrollPosition = VideoConversionConfigurationScroll.VerticalOffset;
            Point currentPoint = new(0, (int)currentScrollPosition);
            Point audioHeaderTargetPosition = AudioHeader.TransformToVisual(VideoConversionConfigurationScroll).TransformPoint(currentPoint);
            Point subtitleHeaderTargetPosition = SubtitleHeader.TransformToVisual(VideoConversionConfigurationScroll).TransformPoint(currentPoint);

            if (currentScrollPosition >= subtitleHeaderTargetPosition.Y)
            {
                SelectedItem = VideoConversionConfigurationSelectorBar.Items[2];
            }
            else if (currentScrollPosition >= audioHeaderTargetPosition.Y && currentScrollPosition < subtitleHeaderTargetPosition.Y)
            {
                SelectedItem = VideoConversionConfigurationSelectorBar.Items[1];
            }
            else
            {
                SelectedItem = VideoConversionConfigurationSelectorBar.Items[0];
            }
        }

        /// <summary>
        /// 格式转换类型菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFormatConversionTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel formatConversionType && !Equals(SelectedFormatConversionType, formatConversionType))
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
                SelectedVolume = VolumeCollection[4];

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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel videoEncoding && !Equals(SelectedVideoEncoding, videoEncoding))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel sizeLimitation && !Equals(SelectedSizeLimitation, sizeLimitation))
            {
                SelectedSizeLimitation = sizeLimitation;
            }
        }

        /// <summary>
        /// 屏幕大小菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnScreenSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel screenSize && !Equals(SelectedScreenSize, screenSize))
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
                    ScreenWidth = int.MaxValue;
                    ScreenWidth = newValue < 1 ? 1 : Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionConfigurationWindow), nameof(OnScreenWidthValueChanged), 1, e);
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
                    int newValue = Convert.ToInt32(args.NewValue);
                    ScreenHeight = int.MaxValue;
                    ScreenHeight = newValue < 1 ? 1 : Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionConfigurationWindow), nameof(OnScreenHeightValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 视频比特率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoBitRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel videoBitRate && !Equals(SelectedVideoBitRate, videoBitRate))
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionConfigurationWindow), nameof(OnCRFValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// GPU 菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnGPUSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel gpu && !Equals(SelectedGPU, gpu))
            {
                SelectedGPU = gpu;
            }
        }

        /// <summary>
        /// 每秒帧数菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFramePerSecondSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel framePerSecond && !Equals(SelectedFramePerSecond, framePerSecond))
            {
                SelectedFramePerSecond = framePerSecond;
            }
        }

        /// <summary>
        /// 宽高比菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAspectRatioSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel aspectRatio && !Equals(SelectedFramePerSecond, aspectRatio))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel keyFrameInterval && !Equals(SelectedFramePerSecond, keyFrameInterval))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel rotation && !Equals(SelectedRotation, rotation))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel videoFadeInEffect && !Equals(SelectedVideoFadeInEffect, videoFadeInEffect))
            {
                SelectedVideoFadeInEffect = videoFadeInEffect;
            }
        }

        /// <summary>
        /// 视频淡出效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVideoFadeOutEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel videoFadeOutEffect && !Equals(SelectedVideoFadeOutEffect, videoFadeOutEffect))
            {
                SelectedVideoFadeOutEffect = videoFadeOutEffect;
            }
        }

        /// <summary>
        /// 音频编码效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioEncodingSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel audioEncoding && !Equals(SelectedAudioEncoding, audioEncoding))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel samplingRate && !Equals(SelectedSamplingRate, samplingRate))
            {
                SelectedSamplingRate = samplingRate;
            }
        }

        /// <summary>
        /// 音频比特率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioBitRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel audioBitRate && !Equals(SelectedAudioBitRate, audioBitRate))
            {
                SelectedAudioBitRate = audioBitRate;
            }
        }

        /// <summary>
        /// 声道菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSoundTrackSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel soundTrack && !Equals(SelectedSoundTrack, soundTrack))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel volume && !Equals(SelectedVolume, volume))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel audioFadeInEffect && !Equals(SelectedAudioFadeInEffect, audioFadeInEffect))
            {
                SelectedAudioFadeInEffect = audioFadeInEffect;
            }
        }

        /// <summary>
        /// 音频淡出效果菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAudioFadeOutEffectSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel audioFadeOutEffect && !Equals(SelectedAudioFadeOutEffect, audioFadeOutEffect))
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionConfigurationWindow), nameof(OnAdditionalSubtitlePathClicked), 1, e);
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel subtitleNestType && !Equals(SelectedSubtitleNestType, subtitleNestType))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel fontSize && !Equals(SelectedFontSize, fontSize))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel fontBorderStyle && !Equals(SelectedFontBorderStyle, fontBorderStyle))
            {
                SelectedFontBorderStyle = fontBorderStyle;
            }
        }

        /// <summary>
        /// 轮廓线大小菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnCounterLineSizeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel counterLineSize && !Equals(SelectedCounterLineSize, counterLineSize))
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
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel shadowSize && !Equals(SelectedShadowSize, shadowSize))
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
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
            }
            else
            {
                WindowSystemBackdrop = null;
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
        private nint VideoConversionConfigurationWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
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
                                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionConfigurationWindow), nameof(VideoConversionConfigurationWindowSubClassProc), 1, e);
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
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, videoConversionConfigurationWindowSubClassProc, 0);
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
        private void InitializeData(VideoConversionConfigurationModel videoConversionConfiguration = null)
        {
            SelectedFormatConversionType = videoConversionConfiguration is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.FormatConversionType)) is ComboBoxItemModel selectedFormatConversionType ? selectedFormatConversionType : FormatConversionTypeList[0];

            ResetVideoEncoding();
            SelectedVideoEncoding = videoConversionConfiguration is not null && VideoEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.VideoEncoding)) is ComboBoxItemModel selectedVideoEncoding ? selectedVideoEncoding : VideoEncodingCollection[0];

            ResetSizeLimitation();
            SelectedSizeLimitation = videoConversionConfiguration is not null && SizeLimitationCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.SizeLimitation)) is ComboBoxItemModel selectedSizeLimitation ? selectedSizeLimitation : SizeLimitationCollection[0];

            ResetScreenSize();
            SelectedScreenSize = videoConversionConfiguration is not null && ScreenSizeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.ScreenSize)) is ComboBoxItemModel selectedScreenSize ? selectedScreenSize : ScreenSizeCollection[0];

            ResetVideoBitRate();
            SelectedVideoBitRate = videoConversionConfiguration is not null && VideoBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.VideoBitRate)) is ComboBoxItemModel selectedVideoBitRate ? selectedVideoBitRate : VideoBitRateCollection[0];

            ResetCRF();
            if (IsCRFSupported && videoConversionConfiguration is not null)
            {
                UseCRF = videoConversionConfiguration.CRF is not -1;
                CRF = videoConversionConfiguration.CRF is not -1 ? videoConversionConfiguration.CRF : 10;
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

            GPUList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });

            if (dxgiAdapterList.Contains(32902))
            {
                GPUList.Add(new ComboBoxItemModel() { SelectedValue = "INTEL", DisplayMember = "INTEL" });
            }

            if (dxgiAdapterList.Contains(4318))
            {
                GPUList.Add(new ComboBoxItemModel() { SelectedValue = "NVIDIA", DisplayMember = "NVIDIA" });
            }

            if (dxgiAdapterList.Contains(4098))
            {
                GPUList.Add(new ComboBoxItemModel() { SelectedValue = "AMD", DisplayMember = "AMD" });
            }

            ResetGPU();
            SelectedGPU = videoConversionConfiguration is not null && GPUCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.GPU)) is ComboBoxItemModel selectedGPU ? selectedGPU : GPUCollection[0];

            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "12", DisplayMember = "12" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "15", DisplayMember = "15" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "18", DisplayMember = "18" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "20", DisplayMember = "20" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "23.976", DisplayMember = "23.976" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "24", DisplayMember = "24" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "25", DisplayMember = "25" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "29.97", DisplayMember = "29.97" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "30", DisplayMember = "30" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "50", DisplayMember = "50" });
            FramePerSecondList.Add(new ComboBoxItemModel() { SelectedValue = "60", DisplayMember = "60" });
            SelectedFramePerSecond = videoConversionConfiguration is not null && FramePerSecondList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.FramePerSecond)) is ComboBoxItemModel selectedFramePerSecond ? selectedFramePerSecond : FramePerSecondList[0];

            ResetAspectRatio();
            SelectedAspectRatio = videoConversionConfiguration is not null && AspectRatioCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.AspectRatio)) is ComboBoxItemModel selectedAspectRatio ? selectedAspectRatio : AspectRatioCollection[0];

            ResetSecondaryEncoding();
            if (IsSecondaryEncodingEnabled && videoConversionConfiguration is not null)
            {
                SecondaryEncoding = videoConversionConfiguration.SecondaryEncoding;
            }

            ResetKeyFrameInterval();
            SelectedKeyFrameInterval = videoConversionConfiguration is not null && KeyFrameIntervalCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.KeyFrameInterval)) is ComboBoxItemModel selectedKeyFrameInterval ? selectedKeyFrameInterval : KeyFrameIntervalCollection[0];

            if (videoConversionConfiguration is not null)
            {
                DeInterlace = videoConversionConfiguration.DeInterlace;
            }

            RotationList.Add(new ComboBoxItemModel() { SelectedValue = Rotation.Rotate0, DisplayMember = NoRotateString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = Rotation.Rotate90, DisplayMember = RotateRightString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = Rotation.Rotate180, DisplayMember = UnsideDownString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = Rotation.Rotate270, DisplayMember = RotateLeftString });
            SelectedRotation = videoConversionConfiguration is not null && RotationList.Find(item => Equals((Rotation)item.SelectedValue, videoConversionConfiguration.Rotation)) is ComboBoxItemModel selectedRotation ? selectedRotation : RotationList[0];

            if (videoConversionConfiguration is not null)
            {
                MirrorReversal = videoConversionConfiguration.MirrorReversal;
            }

            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
            SelectedVideoFadeInEffect = videoConversionConfiguration is not null && VideoFadeInEffectList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.VideoFadeInEffect)) is ComboBoxItemModel selectedVideoFadeInEffect ? selectedVideoFadeInEffect : VideoFadeInEffectList[0];

            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });

            SelectedVideoFadeOutEffect = videoConversionConfiguration is not null && VideoFadeOutEffectList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.VideoFadeOutEffect)) is ComboBoxItemModel selectedVideoFadeOutEffect ? selectedVideoFadeOutEffect : VideoFadeOutEffectList[0];

            IsAudioConfigurationSupported = !Equals(SelectedFormatConversionType, FormatConversionTypeList[2]);

            ResetAudioEncoding();
            SelectedAudioEncoding = videoConversionConfiguration is not null && AudioEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.AudioEncoding)) is ComboBoxItemModel selectedAudioEncoding ? selectedAudioEncoding : AudioEncodingCollection[0];

            ResetSamplingRate();
            SelectedSamplingRate = videoConversionConfiguration is not null && SamplingRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.SamplingRate)) is ComboBoxItemModel selectedSamplingRate ? selectedSamplingRate : SamplingRateCollection[0];

            ResetAudioBitRate();
            SelectedAudioBitRate = videoConversionConfiguration is not null && AudioBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.AudioBitRate)) is ComboBoxItemModel selectedAudioBitRate ? selectedAudioBitRate : AudioBitRateCollection[0];

            ResetSoundTrack();
            SelectedSoundTrack = videoConversionConfiguration is not null && SoundTrackCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.SoundTrack)) is ComboBoxItemModel selectedSoundTrack ? selectedSoundTrack : SoundTrackCollection[0];

            if (videoConversionConfiguration is not null)
            {
                CloseSoundEffect = videoConversionConfiguration.CloseSoundEffect;
            }

            ResetVolume();
            SelectedVolume = videoConversionConfiguration is not null && VolumeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.Volume)) is ComboBoxItemModel selectedVolume ? selectedVolume : VolumeCollection[4];

            if (videoConversionConfiguration is not null)
            {
                PreserveAllSourceInputAudioStream = videoConversionConfiguration.PreserveAllSourceInputAudioStream;
            }

            ResetAudioFadeInEffect();
            SelectedAudioFadeInEffect = videoConversionConfiguration is not null && AudioFadeInEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.AudioFadeInEffect)) is ComboBoxItemModel selectedAudioFadeInEffect ? selectedAudioFadeInEffect : AudioFadeInEffectCollection[0];

            ResetAudioFadeOutEffect();
            SelectedAudioFadeOutEffect = videoConversionConfiguration is not null && AudioFadeOutEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.AudioFadeOutEffect)) is ComboBoxItemModel selectedAudioFadeOutEffect ? selectedAudioFadeOutEffect : AudioFadeOutEffectCollection[0];

            if (videoConversionConfiguration is not null)
            {
                Echo = videoConversionConfiguration.Echo;
                DeNoise = videoConversionConfiguration.DeNoise;
                Reverse = videoConversionConfiguration.Reverse;
            }

            ResetPreserveAllSourceInputSubtitleStream();
            if (videoConversionConfiguration is not null)
            {
                if (IsPreserveAllSourceInputSubtitleStreamEnabled)
                {
                    PreserveAllSourceInputSubtitleStream = videoConversionConfiguration.PreserveAllSourceInputSubtitleStream;
                }

                AdditionalSubtitlePath = videoConversionConfiguration.AdditionalSubtitlePath;
            }

            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Embedded", DisplayMember = "Embedded" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Ansi", DisplayMember = "Ansi" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Unicode", DisplayMember = "Unicode" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "UTF8", DisplayMember = "UTF8" });
            SelectedSubtitleNestType = videoConversionConfiguration is not null && SubtitleNestTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.SubtitleNestType)) is ComboBoxItemModel selectedSubtitleNestType ? selectedSubtitleNestType : SubtitleNestTypeList[0];

            FontName = videoConversionConfiguration is not null && !string.IsNullOrEmpty(videoConversionConfiguration.FontName) ? videoConversionConfiguration.FontName : FontName = System.Drawing.SystemFonts.DefaultFont.Name;

            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = string.Format("{0} {1}", 1, SmallString) });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = string.Format("{0} {1}", 3, NormalString) });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 5, DisplayMember = string.Format("{0} {1}", 5, LargeString) });
            SelectedFontSize = videoConversionConfiguration is not null && FontSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionConfiguration.FontSize)) is ComboBoxItemModel selectedFontSize ? selectedFontSize : FontSizeList[0];

            System.Windows.Media.Color accentColor = System.Windows.SystemParameters.WindowGlassColor;
            FontColor = videoConversionConfiguration is not null && !string.IsNullOrEmpty(videoConversionConfiguration.FontColor) ? videoConversionConfiguration.FontColor : accentColor.ToString();

            FontBorderStyleList.Add(new ComboBoxItemModel() { SelectedValue = "BorderAndShadow", DisplayMember = BorderAndShadowString });
            FontBorderStyleList.Add(new ComboBoxItemModel() { SelectedValue = "SolidColorBackground", DisplayMember = SolidColorBackgroundString });
            SelectedFontBorderStyle = videoConversionConfiguration is not null && FontBorderStyleList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionConfiguration.FontBorderStyle)) is ComboBoxItemModel selectedFontBorderStyle ? selectedFontBorderStyle : FontBorderStyleList[0];

            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 0, DisplayMember = "0" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = "1" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = "3" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });
            SelectedCounterLineSize = videoConversionConfiguration is not null && CounterLineSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionConfiguration.CounterLineSize)) is ComboBoxItemModel selectedCounterLineSize ? selectedCounterLineSize : CounterLineSizeList[0];

            CounterLineColor = videoConversionConfiguration is not null && !string.IsNullOrEmpty(videoConversionConfiguration.CounterLineColor) ? videoConversionConfiguration.CounterLineColor : accentColor.ToString();

            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 0, DisplayMember = "0" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = "1" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = "3" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });
            SelectedShadowSize = videoConversionConfiguration is not null && ShadowSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionConfiguration.ShadowSize)) is ComboBoxItemModel selectedShadowSize ? selectedShadowSize : ShadowSizeList[0];
        }

        /// <summary>
        /// 重置视频编码列表选项
        /// </summary>
        private void ResetVideoEncoding()
        {
            VideoEncodingCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "VVC", DisplayMember = "VVC(H266)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_Xvid", DisplayMember = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AV1", DisplayMember = "AV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "VP9", DisplayMember = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_Xvid", DisplayMember = "MPEG4(Xvid)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "GIF", DisplayMember = "GIF" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AV1", DisplayMember = "AV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "VP8", DisplayMember = "VP8" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "VP9", DisplayMember = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_Xvid", DisplayMember = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MSMPEG4V2", DisplayMember = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MSMPEG4V3", DisplayMember = "MSMPEG4V3" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMV2", DisplayMember = "WMV2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "FLV1", DisplayMember = "FLV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG2", DisplayMember = "MPEG2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MJPEG", DisplayMember = "MJPEG" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[5]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "FLV1", DisplayMember = "FLV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AV1", DisplayMember = "AV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "VP9", DisplayMember = "VP9" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[6]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[8]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MSMPEG4V2", DisplayMember = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMV2", DisplayMember = "WMV2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "H263", DisplayMember = "H263" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_Xvid", DisplayMember = "MPEG4(Xvid)" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG2", DisplayMember = "MPEG2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG2", DisplayMember = "MPEG2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[13]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Theora", DisplayMember = "Theora" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "HEVC", DisplayMember = "HEVC(H265)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AVC", DisplayMember = "AVC(H264)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_DivX", DisplayMember = "MPEG4(DivX)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG4_Xvid", DisplayMember = "MPEG4(Xvid)" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MSMPEG4V2", DisplayMember = "MSMPEG4V2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MSMPEG4V3", DisplayMember = "MSMPEG4V3" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMV2", DisplayMember = "WMV2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "FLV1", DisplayMember = "FLV1" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MPEG2", DisplayMember = "MPEG2" });
                VideoEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MJPEG", DisplayMember = "MJPEG" });
            }
        }

        /// <summary>
        /// 重置大小限制
        /// </summary>
        private void ResetSizeLimitation()
        {
            SizeLimitationCollection.Clear();

            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);

            if (string.IsNullOrEmpty(selectedVideoEncoding))
            {
                SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            }
            else
            {
                if (string.Equals(selectedVideoEncoding, "Copy"))
                {
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
                }
                else
                {
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "10", DisplayMember = "10MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "15", DisplayMember = "15MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "20", DisplayMember = "20MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "25", DisplayMember = "25MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "30", DisplayMember = "30MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "35", DisplayMember = "35MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "40", DisplayMember = "40MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "50", DisplayMember = "50MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "60", DisplayMember = "60MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "70", DisplayMember = "70MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "80", DisplayMember = "80MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "90", DisplayMember = "90MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "100", DisplayMember = "100MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "150", DisplayMember = "150MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "200", DisplayMember = "200MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "300", DisplayMember = "300MB" });
                    SizeLimitationCollection.Add(new ComboBoxItemModel() { SelectedValue = "500", DisplayMember = "500MB" });
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
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "DefaultSize", DisplayMember = DefaultSizeString });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "360P", DisplayMember = "360p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "480P", DisplayMember = "480p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "720P", DisplayMember = "720p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "1080P", DisplayMember = "1080p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "2160P", DisplayMember = "2160p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "480I", DisplayMember = "480i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "720I", DisplayMember = "720i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "1280I", DisplayMember = "1280i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "1920I", DisplayMember = "1920i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "3840I", DisplayMember = "3840i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "25%", DisplayMember = "25%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "50%", DisplayMember = "50%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "75%", DisplayMember = "75%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "125%", DisplayMember = "125%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "150%", DisplayMember = "150%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "200%", DisplayMember = "200%" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "Custom", DisplayMember = CustomString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "96P", DisplayMember = "96p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "144P", DisplayMember = "144p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "240P", DisplayMember = "240p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "480P", DisplayMember = "480p" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "320I", DisplayMember = "320i" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "640I", DisplayMember = "640i" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "176×144", DisplayMember = "176×144" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "352×288", DisplayMember = "352×288" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "128×96", DisplayMember = "128×96" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "176×144", DisplayMember = "176×144" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "320×240", DisplayMember = "320×240" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "352×288", DisplayMember = "352×288" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "400×240", DisplayMember = "400×240" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "480×320", DisplayMember = "480×320" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "176×144", DisplayMember = "176×144" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "320×240", DisplayMember = "320×240" });
                ScreenSizeCollection.Add(new ComboBoxItemModel() { SelectedValue = "640×480", DisplayMember = "640×480" });
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
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "384K", DisplayMember = "384K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "512K", DisplayMember = "512K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "768K", DisplayMember = "768K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "1M", DisplayMember = "1M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "1.5M", DisplayMember = "1.5M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "2M", DisplayMember = "2M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "5M", DisplayMember = "5M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "10M", DisplayMember = "10M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "15M", DisplayMember = "15M" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "20M", DisplayMember = "20M" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "96K", DisplayMember = "96K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "128K", DisplayMember = "128K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "160K", DisplayMember = "160K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "192K", DisplayMember = "192K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "384K", DisplayMember = "384K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "128K", DisplayMember = "128K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "160K", DisplayMember = "160K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "192K", DisplayMember = "192K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "384K", DisplayMember = "384K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "512K", DisplayMember = "512K" });
                VideoBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "768K", DisplayMember = "768K" });
            }
        }

        /// <summary>
        /// 重置固定速率系数
        /// </summary>
        private void ResetCRF()
        {
            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
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

            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
            if (string.Equals(Convert.ToString(selectedVideoEncoding), "HEVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AVC") || string.Equals(Convert.ToString(selectedVideoEncoding), "AV1"))
            {
                foreach (ComboBoxItemModel gpu in GPUList)
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
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "4:3", DisplayMember = "4:3" });
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "16:9", DisplayMember = "16:9" });
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "3:2", DisplayMember = "3:2" });
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "5:4", DisplayMember = "5:4" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AspectRatioCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            }
        }

        /// <summary>
        /// 重置二次编码
        /// </summary>
        private void ResetSecondaryEncoding()
        {
            string selectedVideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);

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
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "8", DisplayMember = "5" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "6", DisplayMember = "6" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "7", DisplayMember = "7" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "8", DisplayMember = "8" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "9", DisplayMember = "9" });
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "10", DisplayMember = "10" });
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
                KeyFrameIntervalCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
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
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AAC", DisplayMember = "AAC" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AAC", DisplayMember = "AAC" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3", DisplayMember = "MP3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3-VBR", DisplayMember = "MP3-VBR" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "OPUS", DisplayMember = "OPUS" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Vorbis", DisplayMember = "Vorbis" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP2", DisplayMember = "MP2" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3", DisplayMember = "MP3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3-VBR", DisplayMember = "MP3-VBR" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMAv2", DisplayMember = "WMAv2" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "PCM", DisplayMember = "PCM" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AAC", DisplayMember = "AAC" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3", DisplayMember = "MP3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[8]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMAv2", DisplayMember = "WMAv2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AMR_NB", DisplayMember = "AMR_NB" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AMR_WB", DisplayMember = "AMR_WB" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AMR_NB", DisplayMember = "AMR_NB" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AMR_WB", DisplayMember = "AMR_WB" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[10]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AAC", DisplayMember = "AAC" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) || Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[13]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Vorbis", DisplayMember = "Vorbis" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[14]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP2", DisplayMember = "MP2" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3", DisplayMember = "MP3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3-VBR", DisplayMember = "MP3-VBR" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMAv2", DisplayMember = "WMAv2" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "PCM", DisplayMember = "PCM" });
            }
        }

        /// <summary>
        /// 重置采样率
        /// </summary>
        private void ResetSamplingRate()
        {
            SamplingRateCollection.Clear();

            string selectedAudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
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
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "22050", DisplayMember = "22050" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "24000", DisplayMember = "24000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "44100", DisplayMember = "44100" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "48000", DisplayMember = "48000" });
            }
            else if (string.Equals(selectedAudioEncoding, "OPUS") || string.Equals(selectedAudioEncoding, "Vorbis"))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "24000", DisplayMember = "24000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "48000", DisplayMember = "48000" });
            }
            else if (string.Equals(selectedAudioEncoding, "AMR_NB") || string.Equals(selectedAudioEncoding, "AMR_WB"))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "8000", DisplayMember = "8000" });
            }
            else if (string.Equals(selectedAudioEncoding, "None"))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
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
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "24K", DisplayMember = "24K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "32K", DisplayMember = "32K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "64K", DisplayMember = "64K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "128K", DisplayMember = "128K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "192K", DisplayMember = "192K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "224K", DisplayMember = "224K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "320K", DisplayMember = "320K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "12.20", DisplayMember = "12.20" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "10.20", DisplayMember = "10.20" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "7.40", DisplayMember = "7.40" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "4.75", DisplayMember = "4.75" });
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
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = string.Format("{0} {1}", 1, MonoString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = string.Format("{0} {1}", 2, StereoString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = string.Format("{0} {1}", 4, QuadString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "6", DisplayMember = string.Format("{0} {1}", 6, Stereo51String) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "8", DisplayMember = string.Format("{0} {1}", 8, Stereo71String) });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[9]))
            {
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = string.Format("{0} {1}", 1, MonoString) });
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
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "10%", DisplayMember = "10%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "25%", DisplayMember = "25%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "50%", DisplayMember = "50%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "75%", DisplayMember = "75%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "100%", DisplayMember = "100%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "150%", DisplayMember = "150%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "200%", DisplayMember = "200%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "300%", DisplayMember = "300%" });
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "400%", DisplayMember = "400%" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
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
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
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
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
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
            overlappedPresenter.PreferredMinimumWidth = width;
            overlappedPresenter.PreferredMinimumHeight = height;
            User32Library.GetWindowRect((nint)ConversionToolsWindow.AppWindow.Id.Value, out RECT parentRect);
            int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
            int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);
            SelectedItem = VideoConversionConfigurationSelectorBar.Items[0];

            // 挂载相应的事件
            AlwaysShowBackdropService.PropertyChanged += OnServicePropertyChanged;
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            videoConversionConfigurationWindowSubClassProc = new SUBCLASSPROC(VideoConversionConfigurationWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, videoConversionConfigurationWindowSubClassProc, 0, 0);

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
        private Visibility GetIsCustomScreenSizeSelected(ComboBoxItemModel selectedScreenSize)
        {
            return string.Equals(Convert.ToString(selectedScreenSize.SelectedValue), "Custom") ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查是否包含字幕选项
        /// </summary>
        private Visibility GetHasSubtitleConfiguration(VideoConversionTypeKind videoConversionTypeKind)
        {
            return videoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion || videoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow ? Visibility.Visible : Visibility.Collapsed;
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
