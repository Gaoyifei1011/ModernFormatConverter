using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Pages;
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.Dxgi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Foundation;
using Windows.UI;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 视频转换输出配置页面
    /// </summary>
    public sealed partial class VideoConversionOutputConfigurationPage : Page, INotifyPropertyChanged
    {
        private readonly string AllFilesString = ResourceService.VideoConversionOutputConfigurationResource.GetString("AllFiles");
        private readonly string BorderAndShadowString = ResourceService.VideoConversionOutputConfigurationResource.GetString("BorderAndShadow");
        private readonly string CopyString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Copy");
        private readonly string CustomString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Custom");
        private readonly string DefaultString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Default");
        private readonly string DefaultSizeString = ResourceService.VideoConversionOutputConfigurationResource.GetString("DefaultSize");
        private readonly string LargeString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Large");
        private readonly string MonoString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Mono");
        private readonly string NoneString = ResourceService.VideoConversionOutputConfigurationResource.GetString("None");
        private readonly string NormalString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Normal");
        private readonly string NoRotateString = ResourceService.VideoConversionOutputConfigurationResource.GetString("NoRotate");
        private readonly string QuadString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Quad");
        private readonly string RotateLeftString = ResourceService.VideoConversionOutputConfigurationResource.GetString("RotateLeft");
        private readonly string RotateRightString = ResourceService.VideoConversionOutputConfigurationResource.GetString("RotateRight");
        private readonly string SecondString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Second");
        private readonly string SelectFileString = ResourceService.VideoConversionOutputConfigurationResource.GetString("SelectFile");
        private readonly string SmallString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Small");
        private readonly string SolidColorBackgroundString = ResourceService.VideoConversionOutputConfigurationResource.GetString("SolidColorBackground");
        private readonly string StereoString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.VideoConversionOutputConfigurationResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.VideoConversionOutputConfigurationResource.GetString("Stereo71");
        private readonly string SubtitleString = ResourceService.VideoConversionOutputConfigurationResource.GetString("Subtitle");
        private readonly string UnsideDownString = ResourceService.VideoConversionOutputConfigurationResource.GetString("UnsideDown");
        private readonly Guid CLSID_DxgiFactory = new("7B7166EC-21C7-44AE-B21A-C9AE321AE369");
        private readonly List<ComboBoxItemModel> GPUList = [];
        private readonly Color accentColor = (Color)Application.Current.Resources["SystemAccentColor"];
        private bool isInitialized;
        private VideoConversionNavigationParameterModel videoConversionNavigationParameter;

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

        private double _speedPlayback;

        public double SpeedPlayback
        {
            get { return _speedPlayback; }

            set
            {
                if (!Equals(_speedPlayback, value))
                {
                    _speedPlayback = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedPlayback)));
                }
            }
        }

        private double _selectedSpeedPlayback;

        public double SelectedSpeedPlayback
        {
            get { return _selectedSpeedPlayback; }

            set
            {
                if (!Equals(_selectedSpeedPlayback, value))
                {
                    _selectedSpeedPlayback = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSpeedPlayback)));
                }
            }
        }

        private bool _reverseVideo;

        public bool ReverseVideo
        {
            get { return _reverseVideo; }

            set
            {
                if (!Equals(_reverseVideo, value))
                {
                    _reverseVideo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReverseVideo)));
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

        private ComboBoxItemModel _selectedFontName;

        public ComboBoxItemModel SelectedFontName
        {
            get { return _selectedFontName; }

            set
            {
                if (!Equals(_selectedFontName, value))
                {
                    _selectedFontName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontName)));
                }
            }
        }

        private FontFamily _selectedFontFamily = FontFamily.XamlAutoFontFamily;

        public FontFamily SelectedFontFamily
        {
            get { return _selectedFontFamily; }

            set
            {
                if (!Equals(_selectedFontFamily, value))
                {
                    _selectedFontFamily = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontFamily)));
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

        private Color _fontColor;

        public Color FontColor
        {
            get { return _fontColor; }

            set
            {
                if (!Equals(_fontColor, value))
                {
                    _fontColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
                }
            }
        }

        private Color _selectedFontColor;

        public Color SelectedFontColor
        {
            get { return _selectedFontColor; }

            set
            {
                if (!Equals(_selectedFontColor, value))
                {
                    _selectedFontColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontColor)));
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

        private Color _counterLineColor;

        public Color CounterLineColor
        {
            get { return _counterLineColor; }

            set
            {
                if (!Equals(_counterLineColor, value))
                {
                    _counterLineColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CounterLineColor)));
                }
            }
        }

        private Color _selectedCounterLineColor;

        public Color SelectedCounterLineColor
        {
            get { return _selectedCounterLineColor; }

            set
            {
                if (!Equals(_selectedCounterLineColor, value))
                {
                    _selectedCounterLineColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCounterLineColor)));
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

        public List<ComboBoxItemModel> FontNameList { get; } = [];

        public List<ComboBoxItemModel> FontSizeList { get; } = [];

        public List<ComboBoxItemModel> FontBorderStyleList { get; } = [];

        public List<ComboBoxItemModel> CounterLineSizeList { get; } = [];

        public List<ComboBoxItemModel> ShadowSizeList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoConversionOutputConfigurationPage()
        {
            InitializeData();
            InitializeComponent();
        }

        #region 第一部分：重载父类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            SelectedItem = VideoConversionOutputConfigurationSelectorBar.Items[0];
            if (args.Parameter is VideoConversionNavigationParameterModel videoConversionNavigationParameterData)
            {
                videoConversionNavigationParameter = videoConversionNavigationParameterData;
                SelectedVideoConversionTypeKind = videoConversionNavigationParameter.VideoConversionTypeKind;

                // 视频格式转换
                if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    if (videoConversionNavigationParameter.IsGlobalSettings)
                    {
                        UpdateData(null);
                    }
                    else
                    {
                        if (videoConversionNavigationParameter.VideoConversionData is VideoFormatConversionFileModel videoFormatConversionFile && videoFormatConversionFile.VideoConversionOutputConfiguration is not null)
                        {
                            UpdateData(videoFormatConversionFile.VideoConversionOutputConfiguration);
                        }
                    }
                }
                // 视频合并
                else if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                {
                    if (videoConversionNavigationParameter.IsGlobalSettings && videoConversionNavigationParameter.VideoConversionData is VideoConcatModel videoConcat && videoConcat.VideoConversionOutputConfiguration is not null)
                    {
                        UpdateData(videoConcat.VideoConversionOutputConfiguration);
                    }
                }
                // 视频混流
                else if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
                {
                    if (videoConversionNavigationParameter.IsGlobalSettings && videoConversionNavigationParameter.VideoConversionData is VideoMixedFlowModel videoMixedFlow && videoMixedFlow.VideoConversionOutputConfiguration is not null)
                    {
                        UpdateData(videoMixedFlow.VideoConversionOutputConfiguration);
                    }
                }
            }

            isInitialized = true;
        }

        /// <summary>
        /// 离开该页面触发的事件
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            isInitialized = false;
            videoConversionNavigationParameter = null;
        }

        #endregion 第一部分：重载父类事件

        #region 第二部分：视频转换输出配置页面——挂载的事件

        /// <summary>
        /// 点击选择器栏选中项发生变化时发生的事件
        /// </summary>
        private void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (VideoConversionOutputConfigurationScroll.IsLoaded && !Equals(SelectedItem, sender.SelectedItem))
            {
                SelectedItem = sender.SelectedItem;
                int index = sender.Items.IndexOf(SelectedItem);

                switch (index)
                {
                    case 0:
                        {
                            double currentScrollPosition = VideoConversionOutputConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = VideoHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionOutputConfigurationScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 1:
                        {
                            double currentScrollPosition = VideoConversionOutputConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = AudioHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionOutputConfigurationScroll.ChangeView(null, targetPosition.Y, null);
                            break;
                        }
                    case 2:
                        {
                            double currentScrollPosition = VideoConversionOutputConfigurationScroll.VerticalOffset;
                            Point currentPoint = new(0, (int)currentScrollPosition);
                            Point targetPosition = SubtitleHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);
                            VideoConversionOutputConfigurationScroll.ChangeView(null, targetPosition.Y, null);
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
            // 更新数据
            // 视频格式转换
            if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                if (videoConversionNavigationParameter.IsGlobalSettings)
                {
                    if (videoConversionNavigationParameter.VideoConversionData is List<VideoFormatConversionFileModel> videoFormatConversionFileList)
                    {
                        foreach (VideoFormatConversionFileModel videoFormatConversionFile in videoFormatConversionFileList)
                        {
                            if (videoFormatConversionFile.VideoConversionOutputConfiguration is not null)
                            {
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(SelectedSizeLimitation.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(SelectedScreenSize.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(SelectedVideoBitRate.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.CRF = UseCRF ? CRF : -1;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(SelectedGPU.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(SelectedFramePerSecond.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(SelectedAspectRatio.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = SecondaryEncoding;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(SelectedKeyFrameInterval.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.DeInterlace = DeInterlace;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SpeedPlayback = SpeedPlayback;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.ReverseVideo = ReverseVideo;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.Rotation = (Rotation)SelectedRotation.SelectedValue;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.MirrorReversal = MirrorReversal;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(SelectedVideoFadeInEffect);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(SelectedVideoFadeOutEffect);

                                videoFormatConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = CloseSoundEffect;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = PreserveAllSourceInputAudioStream;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.Echo = Echo;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.DeNoise = DeNoise;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.Reverse = Reverse;

                                videoFormatConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = PreserveAllSourceInputSubtitleStream;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.AdditionalSubtitlePath = AdditionalSubtitlePath;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(SelectedSubtitleNestType.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FontName = FontName;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(SelectedFontSize.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FontColor = FontColor;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(SelectedFontBorderStyle.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(SelectedCounterLineSize.SelectedValue);
                                videoFormatConversionFile.VideoConversionOutputConfiguration.CounterLineColor = CounterLineColor;
                                videoFormatConversionFile.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(SelectedShadowSize.SelectedValue);
                            }
                        }
                    }
                }
                else
                {
                    if (videoConversionNavigationParameter.VideoConversionData is VideoFormatConversionFileModel videoFormatConversionFile && videoFormatConversionFile.VideoConversionOutputConfiguration is not null)
                    {
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(SelectedSizeLimitation.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(SelectedScreenSize.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(SelectedVideoBitRate.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.CRF = UseCRF ? CRF : -1;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(SelectedGPU.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(SelectedFramePerSecond.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(SelectedAspectRatio.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = SecondaryEncoding;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(SelectedKeyFrameInterval.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.DeInterlace = DeInterlace;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SpeedPlayback = SpeedPlayback;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.ReverseVideo = ReverseVideo;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.Rotation = (Rotation)SelectedRotation.SelectedValue;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.MirrorReversal = MirrorReversal;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(SelectedVideoFadeInEffect);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(SelectedVideoFadeOutEffect);

                        videoFormatConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = CloseSoundEffect;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = PreserveAllSourceInputAudioStream;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.Echo = Echo;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.DeNoise = DeNoise;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.Reverse = Reverse;

                        videoFormatConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = PreserveAllSourceInputSubtitleStream;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.AdditionalSubtitlePath = AdditionalSubtitlePath;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(SelectedSubtitleNestType.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FontName = FontName;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(SelectedFontSize.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FontColor = FontColor;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(SelectedFontBorderStyle.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(SelectedCounterLineSize.SelectedValue);
                        videoFormatConversionFile.VideoConversionOutputConfiguration.CounterLineColor = CounterLineColor;
                        videoFormatConversionFile.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(SelectedShadowSize.SelectedValue);
                    }
                }
            }
            // 视频合并
            else if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                if (videoConversionNavigationParameter.IsGlobalSettings && videoConversionNavigationParameter.VideoConversionData is VideoConcatModel videoConcat && videoConcat.VideoConversionOutputConfiguration is not null)
                {
                    videoConcat.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(SelectedSizeLimitation.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(SelectedScreenSize.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(SelectedVideoBitRate.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.CRF = UseCRF ? CRF : -1;
                    videoConcat.VideoConversionOutputConfiguration.GPU = Convert.ToString(SelectedGPU.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(SelectedFramePerSecond.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(SelectedAspectRatio.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.SecondaryEncoding = SecondaryEncoding;
                    videoConcat.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(SelectedKeyFrameInterval.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.DeInterlace = DeInterlace;
                    videoConcat.VideoConversionOutputConfiguration.SpeedPlayback = SpeedPlayback;
                    videoConcat.VideoConversionOutputConfiguration.ReverseVideo = ReverseVideo;
                    videoConcat.VideoConversionOutputConfiguration.Rotation = (Rotation)SelectedRotation.SelectedValue;
                    videoConcat.VideoConversionOutputConfiguration.MirrorReversal = MirrorReversal;
                    videoConcat.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(SelectedVideoFadeInEffect);
                    videoConcat.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(SelectedVideoFadeOutEffect);

                    videoConcat.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.CloseSoundEffect = CloseSoundEffect;
                    videoConcat.VideoConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = PreserveAllSourceInputAudioStream;
                    videoConcat.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                    videoConcat.VideoConversionOutputConfiguration.Echo = Echo;
                    videoConcat.VideoConversionOutputConfiguration.DeNoise = DeNoise;
                    videoConcat.VideoConversionOutputConfiguration.Reverse = Reverse;
                }
            }
            // 视频混流
            else if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
            {
                if (videoConversionNavigationParameter.IsGlobalSettings && videoConversionNavigationParameter.VideoConversionData is VideoMixedFlowModel videoMixedFlow && videoMixedFlow.VideoConversionOutputConfiguration is not null)
                {
                    videoMixedFlow.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(SelectedSizeLimitation.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(SelectedVideoEncoding.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(SelectedScreenSize.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(SelectedVideoBitRate.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.CRF = UseCRF ? CRF : -1;
                    videoMixedFlow.VideoConversionOutputConfiguration.GPU = Convert.ToString(SelectedGPU.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(SelectedFramePerSecond.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(SelectedAspectRatio.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.SecondaryEncoding = SecondaryEncoding;
                    videoMixedFlow.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(SelectedKeyFrameInterval.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.DeInterlace = DeInterlace;
                    videoMixedFlow.VideoConversionOutputConfiguration.SpeedPlayback = SpeedPlayback;
                    videoMixedFlow.VideoConversionOutputConfiguration.ReverseVideo = ReverseVideo;
                    videoMixedFlow.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)SelectedRotation.SelectedValue;
                    videoMixedFlow.VideoConversionOutputConfiguration.MirrorReversal = MirrorReversal;
                    videoMixedFlow.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(SelectedVideoFadeInEffect);
                    videoMixedFlow.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(SelectedVideoFadeOutEffect);

                    videoMixedFlow.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.CloseSoundEffect = CloseSoundEffect;
                    videoMixedFlow.VideoConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = PreserveAllSourceInputAudioStream;
                    videoMixedFlow.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.Echo = Echo;
                    videoMixedFlow.VideoConversionOutputConfiguration.DeNoise = DeNoise;
                    videoMixedFlow.VideoConversionOutputConfiguration.Reverse = Reverse;

                    videoMixedFlow.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = PreserveAllSourceInputSubtitleStream;
                    videoMixedFlow.VideoConversionOutputConfiguration.AdditionalSubtitlePath = AdditionalSubtitlePath;
                    videoMixedFlow.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(SelectedSubtitleNestType.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.FontName = FontName;
                    videoMixedFlow.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(SelectedFontSize.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.FontColor = FontColor;
                    videoMixedFlow.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(SelectedFontBorderStyle.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(SelectedCounterLineSize.SelectedValue);
                    videoMixedFlow.VideoConversionOutputConfiguration.CounterLineColor = CounterLineColor;
                    videoMixedFlow.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(SelectedShadowSize.SelectedValue);
                }
            }

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage)
            {
                videoConversionPage.NavigateTo(videoConversionPage.PageList[0], null, false);
            }
        }

        /// <summary>
        /// 滚动列表视图发生变化时触发的事件
        /// </summary>
        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
        {
            // 视频格式转换 或 视频混流
            if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion || SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
            {
                double currentScrollPosition = VideoConversionOutputConfigurationScroll.VerticalOffset;
                Point currentPoint = new(0, (int)currentScrollPosition);
                Point audioHeaderTargetPosition = AudioHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);
                Point subtitleHeaderTargetPosition = SubtitleHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);

                if (currentScrollPosition >= subtitleHeaderTargetPosition.Y)
                {
                    SelectedItem = VideoConversionOutputConfigurationSelectorBar.Items[2];
                }
                else if (currentScrollPosition >= audioHeaderTargetPosition.Y && currentScrollPosition < subtitleHeaderTargetPosition.Y)
                {
                    SelectedItem = VideoConversionOutputConfigurationSelectorBar.Items[1];
                }
                else
                {
                    SelectedItem = VideoConversionOutputConfigurationSelectorBar.Items[0];
                }
            }
            // 视频合并
            else if (SelectedVideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                double currentScrollPosition = VideoConversionOutputConfigurationScroll.VerticalOffset;
                Point currentPoint = new(0, (int)currentScrollPosition);
                Point audioHeaderTargetPosition = AudioHeader.TransformToVisual(VideoConversionOutputConfigurationScroll).TransformPoint(currentPoint);
                SelectedItem = currentScrollPosition >= audioHeaderTargetPosition.Y ? VideoConversionOutputConfigurationSelectorBar.Items[1] : VideoConversionOutputConfigurationSelectorBar.Items[0];
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
                SelectedVolume = Equals(SelectedFormatConversionType, FormatConversionTypeList[2]) ? VolumeCollection[0] : VolumeCollection[4];

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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionOutputConfigurationPage), nameof(OnScreenWidthValueChanged), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionOutputConfigurationPage), nameof(OnScreenHeightValueChanged), 1, e);
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
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    CRF = Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionOutputConfigurationPage), nameof(OnCRFValueChanged), 1, e);
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
        /// 打开倍速播放速度浮出控件时触发的事件
        /// </summary>
        private void OnSpeedPlaybackOpening(object sender, object args)
        {
            SelectedSpeedPlayback = SpeedPlayback;
        }

        /// <summary>
        /// 倍速播放滑动速度发生变化时触发的事件
        /// </summary>
        private void OnSpeedPlaybackValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    SelectedSpeedPlayback = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionOutputConfigurationPage), nameof(OnSpeedPlaybackValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 修改倍速播放
        /// </summary>
        private void OnSpeedPlaybackOkClicked(object sender, RoutedEventArgs args)
        {
            SpeedPlayback = SelectedSpeedPlayback;
            if (SpeedPlaybackFlyout.IsOpen)
            {
                SpeedPlaybackFlyout.Hide();
            }
        }

        /// <summary>
        /// 关闭倍速播放浮出控件
        /// </summary>
        private void OnSpeedPlaybackCancelClicked(object sender, RoutedEventArgs args)
        {
            if (SpeedPlaybackFlyout.IsOpen)
            {
                SpeedPlaybackFlyout.Hide();
            }
        }

        /// <summary>
        /// 是否启用倒放视频
        /// </summary>
        private void OnReverseVideoToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                ReverseVideo = toggleSwitch.IsOn;
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionOutputConfigurationPage), nameof(OnAdditionalSubtitlePathClicked), 1, e);
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
        /// 打开字体名称浮出控件时触发的事件
        /// </summary>
        private void OnFontNameFlyoutOpening(object sender, object args)
        {
            SelectedFontName = FontNameList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), FontName));
            SelectedFontFamily = new FontFamily(Convert.ToString(SelectedFontName.SelectedValue));
        }

        /// <summary>
        /// 修改选中的字体名称
        /// </summary>
        private void OnFontNameSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel fontName && !Equals(SelectedFontName, fontName))
            {
                SelectedFontName = fontName;
                SelectedFontFamily = new FontFamily(Convert.ToString(SelectedFontName.SelectedValue));
            }
        }

        /// <summary>
        /// 修改字体名称
        /// </summary>
        private void OnFontNameOkClicked(object sender, RoutedEventArgs args)
        {
            FontName = Convert.ToString(SelectedFontName.SelectedValue);
            if (FontNameFlyout.IsOpen)
            {
                FontNameFlyout.Hide();
            }
        }

        /// <summary>
        /// 关闭字体名称浮出控件
        /// </summary>
        private void OnFontNameCancelClicked(object sender, RoutedEventArgs args)
        {
            if (FontNameFlyout.IsOpen)
            {
                FontNameFlyout.Hide();
            }
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
        /// 打开字体颜色浮出控件时触发的事件
        /// </summary>
        private void OnFontColorFlyoutOpening(object sender, object args)
        {
            SelectedFontColor = FontColor;
        }

        /// <summary>
        /// 修改字体颜色
        /// </summary>
        private void OnFontColorOkClicked(object sender, RoutedEventArgs args)
        {
            FontColor = SelectedFontColor;
            if (FontColorFlyout.IsOpen)
            {
                FontColorFlyout.Hide();
            }
        }

        /// <summary>
        /// 字体颜色恢复默认
        /// </summary>
        private void OnFontColorRestoreDefaultClicked(object sender, RoutedEventArgs args)
        {
            SelectedFontColor = accentColor;
        }

        /// <summary>
        /// 关闭字体颜色浮出控件
        /// </summary>
        private void OnFontColorCancelClicked(object sender, RoutedEventArgs args)
        {
            if (FontColorFlyout.IsOpen)
            {
                FontColorFlyout.Hide();
            }
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
        /// 打开轮廓线颜色浮出控件时触发的事件
        /// </summary>
        private void OnCounterLineColorFlyoutOpening(object sender, object args)
        {
            SelectedCounterLineColor = CounterLineColor;
        }

        /// <summary>
        /// 修改轮廓线颜色
        /// </summary>
        private void OnCounterLineColorOkClicked(object sender, RoutedEventArgs args)
        {
            CounterLineColor = SelectedCounterLineColor;
            if (CounterLineColorFlyout.IsOpen)
            {
                CounterLineColorFlyout.Hide();
            }
        }

        /// <summary>
        /// 轮廓线颜色恢复默认
        /// </summary>
        private void OnCounterLineColorRestoreDefaultClicked(object sender, RoutedEventArgs args)
        {
            SelectedCounterLineColor = accentColor;
        }

        /// <summary>
        /// 关闭轮廓线颜色浮出控件
        /// </summary>
        private void OnCounterLineColorCancelClicked(object sender, RoutedEventArgs args)
        {
            if (CounterLineColorFlyout.IsOpen)
            {
                CounterLineColorFlyout.Hide();
            }
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

        #endregion 第二部分：视频转换输出配置页面——挂载的事件

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            uint iAdapterNum = 0;
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

            RotationList.Add(new ComboBoxItemModel() { SelectedValue = System.Windows.Media.Imaging.Rotation.Rotate0, DisplayMember = NoRotateString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = System.Windows.Media.Imaging.Rotation.Rotate90, DisplayMember = RotateRightString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = System.Windows.Media.Imaging.Rotation.Rotate180, DisplayMember = UnsideDownString });
            RotationList.Add(new ComboBoxItemModel() { SelectedValue = System.Windows.Media.Imaging.Rotation.Rotate270, DisplayMember = RotateLeftString });

            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            VideoFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });

            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            VideoFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });

            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Embedded", DisplayMember = "Embedded" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Ansi", DisplayMember = "Ansi" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "Unicode", DisplayMember = "Unicode" });
            SubtitleNestTypeList.Add(new ComboBoxItemModel() { SelectedValue = "UTF8", DisplayMember = "UTF8" });

            InstalledFontCollection installedFontCollection = new();

            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                FontNameList.Add(new ComboBoxItemModel()
                {
                    DisplayMember = fontFamily.Name,
                    SelectedValue = fontFamily.Name
                });
            }

            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = string.Format("{0} {1}", 1, SmallString) });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = string.Format("{0} {1}", 3, NormalString) });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });
            FontSizeList.Add(new ComboBoxItemModel() { SelectedValue = 5, DisplayMember = string.Format("{0} {1}", 5, LargeString) });

            FontBorderStyleList.Add(new ComboBoxItemModel() { SelectedValue = "BorderAndShadow", DisplayMember = BorderAndShadowString });
            FontBorderStyleList.Add(new ComboBoxItemModel() { SelectedValue = "SolidColorBackground", DisplayMember = SolidColorBackgroundString });

            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 0, DisplayMember = "0" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = "1" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = "3" });
            CounterLineSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });

            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 0, DisplayMember = "0" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = "1" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 2, DisplayMember = "2" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 3, DisplayMember = "3" });
            ShadowSizeList.Add(new ComboBoxItemModel() { SelectedValue = 4, DisplayMember = "4" });
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(VideoConversionOutputConfigurationModel videoConversionOutputConfiguration)
        {
            SelectedFormatConversionType = videoConversionOutputConfiguration is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.FormatConversionType)) is ComboBoxItemModel selectedFormatConversionType ? selectedFormatConversionType : FormatConversionTypeList[0];

            ResetVideoEncoding();
            SelectedVideoEncoding = videoConversionOutputConfiguration is not null && VideoEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.VideoEncoding)) is ComboBoxItemModel selectedVideoEncoding ? selectedVideoEncoding : VideoEncodingCollection[0];

            ResetSizeLimitation();
            SelectedSizeLimitation = videoConversionOutputConfiguration is not null && SizeLimitationCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.SizeLimitation)) is ComboBoxItemModel selectedSizeLimitation ? selectedSizeLimitation : SizeLimitationCollection[0];

            ResetScreenSize();
            SelectedScreenSize = videoConversionOutputConfiguration is not null && ScreenSizeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.ScreenSize)) is ComboBoxItemModel selectedScreenSize ? selectedScreenSize : ScreenSizeCollection[0];

            ResetVideoBitRate();
            SelectedVideoBitRate = videoConversionOutputConfiguration is not null && VideoBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.VideoBitRate)) is ComboBoxItemModel selectedVideoBitRate ? selectedVideoBitRate : VideoBitRateCollection[0];

            ResetCRF();
            if (IsCRFSupported && videoConversionOutputConfiguration is not null)
            {
                UseCRF = videoConversionOutputConfiguration.CRF is not -1;
                CRF = videoConversionOutputConfiguration.CRF is not -1 && videoConversionOutputConfiguration.CRF >= 10 && videoConversionOutputConfiguration.CRF <= 50 ? videoConversionOutputConfiguration.CRF : 10;
            }

            ResetGPU();
            SelectedGPU = videoConversionOutputConfiguration is not null && GPUCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.GPU)) is ComboBoxItemModel selectedGPU ? selectedGPU : GPUCollection[0];

            SelectedFramePerSecond = videoConversionOutputConfiguration is not null && FramePerSecondList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.FramePerSecond)) is ComboBoxItemModel selectedFramePerSecond ? selectedFramePerSecond : FramePerSecondList[0];

            ResetAspectRatio();
            SelectedAspectRatio = videoConversionOutputConfiguration is not null && AspectRatioCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.AspectRatio)) is ComboBoxItemModel selectedAspectRatio ? selectedAspectRatio : AspectRatioCollection[0];

            ResetSecondaryEncoding();
            if (IsSecondaryEncodingEnabled && videoConversionOutputConfiguration is not null)
            {
                SecondaryEncoding = videoConversionOutputConfiguration.SecondaryEncoding;
            }

            ResetKeyFrameInterval();
            SelectedKeyFrameInterval = videoConversionOutputConfiguration is not null && KeyFrameIntervalCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.KeyFrameInterval)) is ComboBoxItemModel selectedKeyFrameInterval ? selectedKeyFrameInterval : KeyFrameIntervalCollection[0];

            if (videoConversionOutputConfiguration is not null)
            {
                DeInterlace = videoConversionOutputConfiguration.DeInterlace;
            }

            SelectedRotation = videoConversionOutputConfiguration is not null && RotationList.Find(item => Equals((Rotation)item.SelectedValue, videoConversionOutputConfiguration.Rotation)) is ComboBoxItemModel selectedRotation ? selectedRotation : RotationList[0];

            SpeedPlayback = videoConversionOutputConfiguration is not null && videoConversionOutputConfiguration.SpeedPlayback >= 0.1 && videoConversionOutputConfiguration.SpeedPlayback <= 5.0 ? videoConversionOutputConfiguration.SpeedPlayback : 1.0;

            if (videoConversionOutputConfiguration is not null)
            {
                ReverseVideo = videoConversionOutputConfiguration.ReverseVideo;
            }

            if (videoConversionOutputConfiguration is not null)
            {
                MirrorReversal = videoConversionOutputConfiguration.MirrorReversal;
            }

            SelectedVideoFadeInEffect = videoConversionOutputConfiguration is not null && VideoFadeInEffectList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.VideoFadeInEffect)) is ComboBoxItemModel selectedVideoFadeInEffect ? selectedVideoFadeInEffect : VideoFadeInEffectList[0];

            SelectedVideoFadeOutEffect = videoConversionOutputConfiguration is not null && VideoFadeOutEffectList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.VideoFadeOutEffect)) is ComboBoxItemModel selectedVideoFadeOutEffect ? selectedVideoFadeOutEffect : VideoFadeOutEffectList[0];

            IsAudioConfigurationSupported = !Equals(SelectedFormatConversionType, FormatConversionTypeList[2]);

            ResetAudioEncoding();
            SelectedAudioEncoding = videoConversionOutputConfiguration is not null && AudioEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.AudioEncoding)) is ComboBoxItemModel selectedAudioEncoding ? selectedAudioEncoding : AudioEncodingCollection[0];

            ResetSamplingRate();
            SelectedSamplingRate = videoConversionOutputConfiguration is not null && SamplingRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.SamplingRate)) is ComboBoxItemModel selectedSamplingRate ? selectedSamplingRate : SamplingRateCollection[0];

            ResetAudioBitRate();
            SelectedAudioBitRate = videoConversionOutputConfiguration is not null && AudioBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.AudioBitRate)) is ComboBoxItemModel selectedAudioBitRate ? selectedAudioBitRate : AudioBitRateCollection[0];

            ResetSoundTrack();
            SelectedSoundTrack = videoConversionOutputConfiguration is not null && SoundTrackCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.SoundTrack)) is ComboBoxItemModel selectedSoundTrack ? selectedSoundTrack : SoundTrackCollection[0];

            if (videoConversionOutputConfiguration is not null)
            {
                CloseSoundEffect = videoConversionOutputConfiguration.CloseSoundEffect;
            }

            ResetVolume();
            SelectedVolume = videoConversionOutputConfiguration is not null && VolumeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.Volume)) is ComboBoxItemModel selectedVolume ? selectedVolume : VolumeCollection[4];

            if (videoConversionOutputConfiguration is not null)
            {
                PreserveAllSourceInputAudioStream = videoConversionOutputConfiguration.PreserveAllSourceInputAudioStream;
            }

            ResetAudioFadeInEffect();
            SelectedAudioFadeInEffect = videoConversionOutputConfiguration is not null && AudioFadeInEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.AudioFadeInEffect)) is ComboBoxItemModel selectedAudioFadeInEffect ? selectedAudioFadeInEffect : AudioFadeInEffectCollection[0];

            ResetAudioFadeOutEffect();
            SelectedAudioFadeOutEffect = videoConversionOutputConfiguration is not null && AudioFadeOutEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.AudioFadeOutEffect)) is ComboBoxItemModel selectedAudioFadeOutEffect ? selectedAudioFadeOutEffect : AudioFadeOutEffectCollection[0];

            if (videoConversionOutputConfiguration is not null)
            {
                Echo = videoConversionOutputConfiguration.Echo;
                DeNoise = videoConversionOutputConfiguration.DeNoise;
                Reverse = videoConversionOutputConfiguration.Reverse;
            }

            ResetPreserveAllSourceInputSubtitleStream();
            if (videoConversionOutputConfiguration is not null)
            {
                if (IsPreserveAllSourceInputSubtitleStreamEnabled)
                {
                    PreserveAllSourceInputSubtitleStream = videoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream;
                }

                AdditionalSubtitlePath = videoConversionOutputConfiguration.AdditionalSubtitlePath;
            }

            SelectedSubtitleNestType = videoConversionOutputConfiguration is not null && SubtitleNestTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.SubtitleNestType)) is ComboBoxItemModel selectedSubtitleNestType ? selectedSubtitleNestType : SubtitleNestTypeList[0];

            SelectedFontName = videoConversionOutputConfiguration is not null && !string.IsNullOrEmpty(videoConversionOutputConfiguration.FontName) && FontNameList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.FontName)) is ComboBoxItemModel selectedFontName ? selectedFontName : FontNameList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), System.Drawing.SystemFonts.DefaultFont.Name)) is ComboBoxItemModel defaultFontName ? defaultFontName : FontNameList[0];
            FontName = Convert.ToString(SelectedFontName.SelectedValue);

            SelectedFontSize = videoConversionOutputConfiguration is not null && FontSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionOutputConfiguration.FontSize)) is ComboBoxItemModel selectedFontSize ? selectedFontSize : FontSizeList[0];

            FontColor = videoConversionOutputConfiguration is not null && videoConversionOutputConfiguration.FontColor.HasValue ? videoConversionOutputConfiguration.FontColor.Value : accentColor;

            SelectedFontBorderStyle = videoConversionOutputConfiguration is not null && FontBorderStyleList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoConversionOutputConfiguration.FontBorderStyle)) is ComboBoxItemModel selectedFontBorderStyle ? selectedFontBorderStyle : FontBorderStyleList[0];

            SelectedCounterLineSize = videoConversionOutputConfiguration is not null && CounterLineSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionOutputConfiguration.CounterLineSize)) is ComboBoxItemModel selectedCounterLineSize ? selectedCounterLineSize : CounterLineSizeList[0];

            CounterLineColor = videoConversionOutputConfiguration is not null && videoConversionOutputConfiguration.CounterLineColor.HasValue ? videoConversionOutputConfiguration.CounterLineColor.Value : accentColor;

            SelectedShadowSize = videoConversionOutputConfiguration is not null && ShadowSizeList.Find(item => Equals(Convert.ToInt32(item.SelectedValue), videoConversionOutputConfiguration.ShadowSize)) is ComboBoxItemModel selectedShadowSize ? selectedShadowSize : ShadowSizeList[0];
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
        /// 重置音频淡出效果
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
    }
}
