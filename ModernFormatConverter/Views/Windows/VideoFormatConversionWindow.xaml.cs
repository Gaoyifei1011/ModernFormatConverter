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
    /// 视频格式转换窗口
    /// </summary>
    public sealed partial class VideoFormatConversionWindow : Window, INotifyPropertyChanged
    {
        private readonly string AllFilesString = ResourceService.VideoFormatConversionResource.GetString("AllFiles");
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
        private readonly string StereoString = ResourceService.VideoFormatConversionResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.VideoFormatConversionResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.VideoFormatConversionResource.GetString("Stereo71");
        private readonly string SubtitleString = ResourceService.VideoFormatConversionResource.GetString("Subtitle");
        private readonly string UnsideDownString = ResourceService.VideoFormatConversionResource.GetString("UnsideDown");
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

        private KeyValuePairModel _selectedFramePerSecond;

        public KeyValuePairModel SelectedFramePerSecond
        {
            get { return _selectedFramePerSecond; }

            set
            {
                _selectedFramePerSecond = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFramePerSecond)));
            }
        }

        private KeyValuePairModel _selectedAspectRatio;

        public KeyValuePairModel SelectedAspectRatio
        {
            get { return _selectedAspectRatio; }

            set
            {
                _selectedAspectRatio = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAspectRatio)));
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

        private KeyValuePairModel _selectedKeyFrameInterval;

        public KeyValuePairModel SelectedKeyFrameInterval
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

        private KeyValuePairModel _selectedRotation;

        public KeyValuePairModel SelectedRotation
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

        private KeyValuePairModel _selectedVideoFadeInEffect;

        public KeyValuePairModel SelectedVideoFadeInEffect
        {
            get { return _selectedVideoFadeInEffect; }

            set
            {
                _selectedVideoFadeInEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeInEffect)));
            }
        }

        private KeyValuePairModel _selectedVideoFadeOutEffect;

        public KeyValuePairModel SelectedVideoFadeOutEffect
        {
            get { return _selectedVideoFadeOutEffect; }

            set
            {
                _selectedVideoFadeOutEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoFadeOutEffect)));
            }
        }

        private KeyValuePairModel _selectedAudioEncoding;

        public KeyValuePairModel SelectedAudioEncoding
        {
            get { return _selectedAudioEncoding; }

            set
            {
                _selectedAudioEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioEncoding)));
            }
        }

        private KeyValuePairModel _selectedSamplingRate;

        public KeyValuePairModel SelectedSamplingRate
        {
            get { return _selectedSamplingRate; }

            set
            {
                _selectedSamplingRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSamplingRate)));
            }
        }

        private KeyValuePairModel _selectedAudioBitRate;

        public KeyValuePairModel SelectedAudioBitRate
        {
            get { return _selectedAudioBitRate; }

            set
            {
                _selectedAudioBitRate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioBitRate)));
            }
        }

        private KeyValuePairModel _selectedSoundTrack;

        public KeyValuePairModel SelectedSoundTrack
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

        private KeyValuePairModel _selectedVolume;

        public KeyValuePairModel SelectedVolume
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

        private KeyValuePairModel _selectedAudioFadeInEffect;

        public KeyValuePairModel SelectedAudioFadeInEffect
        {
            get { return _selectedAudioFadeInEffect; }

            set
            {
                _selectedAudioFadeInEffect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioFadeInEffect)));
            }
        }

        private KeyValuePairModel _selectedAudioFadeOutEffect;

        public KeyValuePairModel SelectedAudioFadeOutEffect
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

        private KeyValuePairModel _selectedSubtitleNestType;

        public KeyValuePairModel SelectedSubtitleNestType
        {
            get { return _selectedSubtitleNestType; }

            set
            {
                _selectedSubtitleNestType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubtitleNestType)));
            }
        }

        private KeyValuePairModel _selectedFontSize;

        public KeyValuePairModel SelectedFontSize
        {
            get { return _selectedFontSize; }

            set
            {
                _selectedFontSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFontSize)));
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

        public List<KeyValuePairModel> FramePerSecondList { get; } = [];

        public List<KeyValuePairModel> AspectRatioList { get; } = [];

        public List<KeyValuePairModel> KeyFrameIntervalList { get; } = [];

        public List<KeyValuePairModel> RotationList { get; } = [];

        public List<KeyValuePairModel> VideoFadeInEffectList { get; } = [];

        public List<KeyValuePairModel> VideoFadeOutEffectList { get; } = [];

        public List<KeyValuePairModel> AudioEncodingList { get; } = [];

        public List<KeyValuePairModel> SamplingRateList { get; } = [];

        public List<KeyValuePairModel> AudioBitRateList { get; } = [];

        public List<KeyValuePairModel> SoundTrackList { get; } = [];

        public List<KeyValuePairModel> VolumeList { get; } = [];

        public List<KeyValuePairModel> AudioFadeInEffectList { get; } = [];

        public List<KeyValuePairModel> AudioFadeOutEffectList { get; } = [];

        public List<KeyValuePairModel> SubtitleNestTypeList { get; } = [];

        public List<KeyValuePairModel> FontSizeList { get; } = [];

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
                SelectedFramePerSecond = FramePerSecondList[0];
                SelectedFramePerSecond.IsChecked = true;
                SelectedAspectRatio = AspectRatioList[0];
                SelectedAspectRatio.IsChecked = true;
                SelectedKeyFrameInterval = KeyFrameIntervalList[0];
                SelectedKeyFrameInterval.IsChecked = true;
                SelectedRotation = RotationList[0];
                SelectedRotation.IsChecked = true;
                SelectedRotation = RotationList[0];
                SelectedRotation.IsChecked = true;
                SelectedVideoFadeInEffect = VideoFadeInEffectList[0];
                SelectedVideoFadeInEffect.IsChecked = true;
                SelectedVideoFadeOutEffect = VideoFadeOutEffectList[0];
                SelectedVideoFadeOutEffect.IsChecked = true;
                SelectedAudioEncoding = AudioEncodingList[0];
                SelectedAudioEncoding.IsChecked = true;
                SelectedSamplingRate = SamplingRateList[0];
                SelectedSamplingRate.IsChecked = true;
                SelectedAudioBitRate = AudioBitRateList[0];
                SelectedAudioBitRate.IsChecked = true;
                SelectedSoundTrack = SoundTrackList[0];
                SelectedSoundTrack.IsChecked = true;
                SelectedVolume = VolumeList[4];
                SelectedVolume.IsChecked = true;
                SelectedAudioFadeInEffect = AudioFadeInEffectList[0];
                SelectedAudioFadeInEffect.IsChecked = true;
                SelectedAudioFadeOutEffect = AudioFadeOutEffectList[0];
                SelectedAudioFadeOutEffect.IsChecked = true;
                SelectedSubtitleNestType = SubtitleNestTypeList[0];
                SelectedSubtitleNestType.IsChecked = true;
                SelectedFontSize = FontSizeList[2];
                SelectedFontSize.IsChecked = true;
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
                SelectedFramePerSecond = FramePerSecondList[0];
                SelectedFramePerSecond.IsChecked = true;
                SelectedAspectRatio = AspectRatioList[0];
                SelectedAspectRatio.IsChecked = true;
                SelectedKeyFrameInterval = KeyFrameIntervalList[0];
                SelectedKeyFrameInterval.IsChecked = true;
                SelectedRotation = RotationList[0];
                SelectedRotation.IsChecked = true;
                SelectedVideoFadeInEffect = VideoFadeInEffectList[0];
                SelectedVideoFadeInEffect.IsChecked = true;
                SelectedVideoFadeOutEffect = VideoFadeOutEffectList[0];
                SelectedVideoFadeOutEffect.IsChecked = true;
                SelectedAudioEncoding = AudioEncodingList[0];
                SelectedAudioEncoding.IsChecked = true;
                SelectedSamplingRate = SamplingRateList[0];
                SelectedSamplingRate.IsChecked = true;
                SelectedAudioBitRate = AudioBitRateList[0];
                SelectedAudioBitRate.IsChecked = true;
                SelectedSoundTrack = SoundTrackList[0];
                SelectedSoundTrack.IsChecked = true;
                SelectedVolume = VolumeList[4];
                SelectedVolume.IsChecked = true;
                SelectedAudioFadeInEffect = AudioFadeInEffectList[0];
                SelectedAudioFadeInEffect.IsChecked = true;
                SelectedAudioFadeOutEffect = AudioFadeOutEffectList[0];
                SelectedAudioFadeOutEffect.IsChecked = true;
                SelectedSubtitleNestType = SubtitleNestTypeList[0];
                SelectedSubtitleNestType.IsChecked = true;
                SelectedFontSize = FontSizeList[2];
                SelectedFontSize.IsChecked = true;
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

        /// <summary>
        /// 修改每秒帧数
        /// </summary>
        private void OnFramePerSecondExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (FramePerSecondFlyout.IsOpen)
            {
                FramePerSecondFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel framePerSecond)
            {
                foreach (KeyValuePairModel framePerSecondItem in FramePerSecondList)
                {
                    framePerSecondItem.IsChecked = false;
                    if (string.Equals(framePerSecond.Key, framePerSecondItem.Key))
                    {
                        SelectedFramePerSecond = framePerSecondItem;
                        framePerSecondItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改宽高比
        /// </summary>
        private void OnAspectRatioExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (AspectRatioFlyout.IsOpen)
            {
                AspectRatioFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel aspectRatio)
            {
                foreach (KeyValuePairModel aspectRatioItem in AspectRatioList)
                {
                    aspectRatioItem.IsChecked = false;
                    if (string.Equals(aspectRatio.Key, aspectRatioItem.Key))
                    {
                        SelectedAspectRatio = aspectRatioItem;
                        aspectRatioItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改关键帧间隔
        /// </summary>
        private void OnKeyFrameIntervalExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (KeyFrameIntervalFlyout.IsOpen)
            {
                KeyFrameIntervalFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel keyFrameInterval)
            {
                foreach (KeyValuePairModel keyFrameIntervalItem in KeyFrameIntervalList)
                {
                    keyFrameIntervalItem.IsChecked = false;
                    if (string.Equals(keyFrameInterval.Key, keyFrameIntervalItem.Key))
                    {
                        SelectedKeyFrameInterval = keyFrameIntervalItem;
                        keyFrameIntervalItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改旋转
        /// </summary>
        private void OnRotationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (RotationFlyout.IsOpen)
            {
                RotationFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel rotation)
            {
                foreach (KeyValuePairModel rotationItem in RotationList)
                {
                    rotationItem.IsChecked = false;
                    if (string.Equals(rotation.Key, rotationItem.Key))
                    {
                        SelectedRotation = rotationItem;
                        rotationItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改视频淡入效果
        /// </summary>
        private void OnVideoFadeInEffectExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (VideoFadeInEffectFlyout.IsOpen)
            {
                VideoFadeInEffectFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel videoFadeInEffect)
            {
                foreach (KeyValuePairModel videoFadeInEffectItem in VideoFadeInEffectList)
                {
                    videoFadeInEffectItem.IsChecked = false;
                    if (string.Equals(videoFadeInEffect.Key, videoFadeInEffectItem.Key))
                    {
                        SelectedVideoFadeInEffect = videoFadeInEffectItem;
                        videoFadeInEffectItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改视频淡出效果
        /// </summary>
        private void OnVideoFadeOutEffectExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (VideoFadeOutEffectFlyout.IsOpen)
            {
                VideoFadeOutEffectFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel videoFadeOutEffect)
            {
                foreach (KeyValuePairModel videoFadeOutEffectItem in VideoFadeOutEffectList)
                {
                    videoFadeOutEffectItem.IsChecked = false;
                    if (string.Equals(videoFadeOutEffect.Key, videoFadeOutEffectItem.Key))
                    {
                        SelectedVideoFadeOutEffect = videoFadeOutEffectItem;
                        videoFadeOutEffectItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改音频编码
        /// </summary>
        private void OnAudioEncodingExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (AudioEncodingFlyout.IsOpen)
            {
                AudioEncodingFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel audioEncoding)
            {
                foreach (KeyValuePairModel audioEncodingItem in AudioEncodingList)
                {
                    audioEncodingItem.IsChecked = false;
                    if (string.Equals(audioEncoding.Key, audioEncodingItem.Key))
                    {
                        SelectedAudioEncoding = audioEncodingItem;
                        audioEncodingItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改采样率
        /// </summary>
        private void OnSamplingRateExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (SamplingRateFlyout.IsOpen)
            {
                SamplingRateFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel samplingRate)
            {
                foreach (KeyValuePairModel samplingRateItem in SamplingRateList)
                {
                    samplingRateItem.IsChecked = false;
                    if (string.Equals(samplingRate.Key, samplingRateItem.Key))
                    {
                        SelectedSamplingRate = samplingRateItem;
                        samplingRateItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改音频比特率
        /// </summary>
        private void OnAudioBitRateExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (AudioBitRateFlyout.IsOpen)
            {
                AudioBitRateFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel audioBitRate)
            {
                foreach (KeyValuePairModel audioBitRateItem in AudioBitRateList)
                {
                    audioBitRateItem.IsChecked = false;
                    if (string.Equals(audioBitRate.Key, audioBitRateItem.Key))
                    {
                        SelectedAudioBitRate = audioBitRateItem;
                        audioBitRateItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改声道
        /// </summary>
        private void OnSoundTrackExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (SoundTrackFlyout.IsOpen)
            {
                SoundTrackFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel soundTrack)
            {
                foreach (KeyValuePairModel soundTrackItem in SoundTrackList)
                {
                    soundTrackItem.IsChecked = false;
                    if (string.Equals(soundTrack.Key, soundTrackItem.Key))
                    {
                        SelectedSoundTrack = soundTrackItem;
                        soundTrackItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改声道
        /// </summary>
        private void OnVolumeExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (VolumeFlyout.IsOpen)
            {
                VolumeFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel volume)
            {
                foreach (KeyValuePairModel volumeItem in VolumeList)
                {
                    volumeItem.IsChecked = false;
                    if (string.Equals(volume.Key, volumeItem.Key))
                    {
                        SelectedVolume = volumeItem;
                        volumeItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改音频淡入效果
        /// </summary>
        private void OnAudioFadeInEffectExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (AudioFadeInEffectFlyout.IsOpen)
            {
                AudioFadeInEffectFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel audioFadeInEffect)
            {
                foreach (KeyValuePairModel audioFadeInEffectItem in AudioFadeInEffectList)
                {
                    audioFadeInEffectItem.IsChecked = false;
                    if (string.Equals(audioFadeInEffect.Key, audioFadeInEffectItem.Key))
                    {
                        SelectedAudioFadeInEffect = audioFadeInEffectItem;
                        audioFadeInEffectItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改音频淡出效果
        /// </summary>
        private void OnAudioFadeOutEffectExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (AudioFadeOutEffectFlyout.IsOpen)
            {
                AudioFadeOutEffectFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel audioFadeOutEffect)
            {
                foreach (KeyValuePairModel audioFadeOutEffectItem in AudioFadeOutEffectList)
                {
                    audioFadeOutEffectItem.IsChecked = false;
                    if (string.Equals(audioFadeOutEffect.Key, audioFadeOutEffectItem.Key))
                    {
                        SelectedAudioFadeOutEffect = audioFadeOutEffectItem;
                        audioFadeOutEffectItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改字幕嵌入类型
        /// </summary>
        private void OnSubtitleNestTypeExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (SubtitleNestTypeFlyout.IsOpen)
            {
                SubtitleNestTypeFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel subtitleNestType)
            {
                foreach (KeyValuePairModel subtitleNestTypeItem in SubtitleNestTypeList)
                {
                    subtitleNestTypeItem.IsChecked = false;
                    if (string.Equals(subtitleNestType.Key, subtitleNestTypeItem.Key))
                    {
                        SelectedSubtitleNestType = subtitleNestTypeItem;
                        subtitleNestTypeItem.IsChecked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 修改字体大小
        /// </summary>
        private void OnFontSizeExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (FontSizeFlyout.IsOpen)
            {
                FontSizeFlyout.Hide();
            }

            if (args.Parameter is KeyValuePairModel fontSize)
            {
                foreach (KeyValuePairModel fontSizeItem in FontSizeList)
                {
                    fontSizeItem.IsChecked = false;
                    if (string.Equals(fontSize.Key, fontSizeItem.Key))
                    {
                        SelectedFontSize = fontSizeItem;
                        fontSizeItem.IsChecked = true;
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
        /// 视频比特率菜单打开时自动定位到选中项
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

        /// <summary>
        /// 每秒帧数菜单打开时自动定位到选中项
        /// </summary>
        private void OnFramePerSecondOpened(object sender, object args)
        {
            foreach (KeyValuePairModel framePerSecond in FramePerSecondList)
            {
                if (framePerSecond.IsChecked)
                {
                    FramePerSecondListView.ScrollIntoView(framePerSecond);
                    break;
                }
            }
        }

        /// <summary>
        /// 宽高比菜单打开时自动定位到选中项
        /// </summary>
        private void OnAspectRatioOpened(object sender, object args)
        {
            foreach (KeyValuePairModel aspectRatio in AspectRatioList)
            {
                if (aspectRatio.IsChecked)
                {
                    AspectRatioListView.ScrollIntoView(aspectRatio);
                    break;
                }
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
        /// 关键帧间隔菜单打开时自动定位到选中项
        /// </summary>
        private void OnKeyFrameIntervalOpened(object sender, object args)
        {
            foreach (KeyValuePairModel keyFrameInterval in KeyFrameIntervalList)
            {
                if (keyFrameInterval.IsChecked)
                {
                    KeyFrameIntervalListView.ScrollIntoView(keyFrameInterval);
                    break;
                }
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
        /// 旋转菜单打开时自动定位到选中项
        /// </summary>
        private void OnRotationOpened(object sender, object args)
        {
            foreach (KeyValuePairModel rotation in RotationList)
            {
                if (rotation.IsChecked)
                {
                    RotationListView.ScrollIntoView(rotation);
                    break;
                }
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
        /// 视频淡入效果菜单打开时自动定位到选中项
        /// </summary>
        private void OnVideoFadeInEffectOpened(object sender, object args)
        {
            foreach (KeyValuePairModel videoFadeInEffect in VideoFadeInEffectList)
            {
                if (videoFadeInEffect.IsChecked)
                {
                    VideoFadeInEffectListView.ScrollIntoView(videoFadeInEffect);
                    break;
                }
            }
        }

        /// <summary>
        /// 视频淡出效果菜单打开时自动定位到选中项
        /// </summary>
        private void OnVideoFadeOutEffectOpened(object sender, object args)
        {
            foreach (KeyValuePairModel videoFadeOutEffect in VideoFadeOutEffectList)
            {
                if (videoFadeOutEffect.IsChecked)
                {
                    VideoFadeOutEffectListView.ScrollIntoView(videoFadeOutEffect);
                    break;
                }
            }
        }

        /// <summary>
        /// 音频编码菜单打开时自动定位到选中项
        /// </summary>
        private void OnAudioEncodingOpened(object sender, object args)
        {
            foreach (KeyValuePairModel audioEncoding in AudioEncodingList)
            {
                if (audioEncoding.IsChecked)
                {
                    AudioEncodingListView.ScrollIntoView(audioEncoding);
                    break;
                }
            }
        }

        /// <summary>
        /// 音频编码菜单打开时自动定位到选中项
        /// </summary>
        private void OnSamplingRateOpened(object sender, object args)
        {
            foreach (KeyValuePairModel samplingRate in SamplingRateList)
            {
                if (samplingRate.IsChecked)
                {
                    SamplingRateListView.ScrollIntoView(samplingRate);
                    break;
                }
            }
        }

        /// <summary>
        /// 音频比特率菜单打开时自动定位到选中项
        /// </summary>
        private void OnAudioBitRateOpened(object sender, object args)
        {
            foreach (KeyValuePairModel audioBitRate in AudioBitRateList)
            {
                if (audioBitRate.IsChecked)
                {
                    AudioBitRateListView.ScrollIntoView(audioBitRate);
                    break;
                }
            }
        }

        /// <summary>
        /// 声道菜单打开时自动定位到选中项
        /// </summary>
        private void OnSoundTrackOpened(object sender, object args)
        {
            foreach (KeyValuePairModel soundTrack in SoundTrackList)
            {
                if (soundTrack.IsChecked)
                {
                    SoundTrackListView.ScrollIntoView(soundTrack);
                    break;
                }
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
        /// 音量菜单打开时自动定位到选中项
        /// </summary>
        private void OnVolumeOpened(object sender, object args)
        {
            foreach (KeyValuePairModel volume in VolumeList)
            {
                if (volume.IsChecked)
                {
                    VolumeListView.ScrollIntoView(volume);
                    break;
                }
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
        /// 音频淡入效果菜单打开时自动定位到选中项
        /// </summary>
        private void OnAudioFadeInEffectOpened(object sender, object args)
        {
            foreach (KeyValuePairModel audioFadeInEffect in AudioFadeInEffectList)
            {
                if (audioFadeInEffect.IsChecked)
                {
                    AudioFadeInEffectListView.ScrollIntoView(audioFadeInEffect);
                    break;
                }
            }
        }

        /// <summary>
        /// 音频淡出效果菜单打开时自动定位到选中项
        /// </summary>
        private void OnAudioFadeOutEffectOpened(object sender, object args)
        {
            foreach (KeyValuePairModel audioFadeOutEffect in AudioFadeOutEffectList)
            {
                if (audioFadeOutEffect.IsChecked)
                {
                    AudioFadeOutEffectListView.ScrollIntoView(audioFadeOutEffect);
                    break;
                }
            }
        }

        /// <summary>
        /// 字幕嵌入类型菜单打开时自动定位到选中项
        /// </summary>
        private void OnSubtitleNestTypeOpened(object sender, object args)
        {
            foreach (KeyValuePairModel subtitleNestType in SubtitleNestTypeList)
            {
                if (subtitleNestType.IsChecked)
                {
                    SubtitleNestTypeListView.ScrollIntoView(subtitleNestType);
                    break;
                }
            }
        }

        /// <summary>
        /// 字体大小菜单打开时自动定位到选中项
        /// </summary>
        private void OnFontSizeOpened(object sender, object args)
        {
            foreach (KeyValuePairModel fontSize in FontSizeList)
            {
                if (fontSize.IsChecked)
                {
                    FontSizeListView.ScrollIntoView(fontSize);
                    break;
                }
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

            FramePerSecondList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "12", Value = "12" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "15", Value = "15" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "18", Value = "18" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "20", Value = "20" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "23.976", Value = "23.976" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "24", Value = "24" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "25", Value = "25" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "29.97", Value = "29.97" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "30", Value = "30" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "50", Value = "50" });
            FramePerSecondList.Add(new KeyValuePairModel() { Key = "60", Value = "60" });

            AspectRatioList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            AspectRatioList.Add(new KeyValuePairModel() { Key = "4:3", Value = "4:3" });
            AspectRatioList.Add(new KeyValuePairModel() { Key = "16:9", Value = "16:9" });
            AspectRatioList.Add(new KeyValuePairModel() { Key = "3:2", Value = "3:2" });
            AspectRatioList.Add(new KeyValuePairModel() { Key = "5:4", Value = "5:4" });

            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "1", Value = "1" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "2", Value = "2" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "3", Value = "3" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "4", Value = "4" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "8", Value = "5" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "6", Value = "6" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "7", Value = "7" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "8", Value = "8" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "9", Value = "9" });
            KeyFrameIntervalList.Add(new KeyValuePairModel() { Key = "10", Value = "10" });

            RotationList.Add(new KeyValuePairModel() { Key = Convert.ToString(Rotation.Rotate0), Value = NoRotateString });
            RotationList.Add(new KeyValuePairModel() { Key = Convert.ToString(Rotation.Rotate90), Value = RotateRightString });
            RotationList.Add(new KeyValuePairModel() { Key = Convert.ToString(Rotation.Rotate180), Value = UnsideDownString });
            RotationList.Add(new KeyValuePairModel() { Key = Convert.ToString(Rotation.Rotate270), Value = RotateLeftString });

            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "1", Value = "1" + SecondString });
            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "2", Value = "2" + SecondString });
            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "3", Value = "3" + SecondString });
            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "4", Value = "4" + SecondString });
            VideoFadeInEffectList.Add(new KeyValuePairModel() { Key = "5", Value = "5" + SecondString });

            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "1", Value = "1" + SecondString });
            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "2", Value = "2" + SecondString });
            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "3", Value = "3" + SecondString });
            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "4", Value = "4" + SecondString });
            VideoFadeOutEffectList.Add(new KeyValuePairModel() { Key = "5", Value = "5" + SecondString });

            AudioEncodingList.Add(new KeyValuePairModel() { Key = "Copy", Value = CopyString });
            AudioEncodingList.Add(new KeyValuePairModel() { Key = "AAC", Value = "AAC" });
            AudioEncodingList.Add(new KeyValuePairModel() { Key = "AC3", Value = "AC3" });

            SamplingRateList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            SamplingRateList.Add(new KeyValuePairModel() { Key = "22050", Value = "22050" });
            SamplingRateList.Add(new KeyValuePairModel() { Key = "24000", Value = "24000" });
            SamplingRateList.Add(new KeyValuePairModel() { Key = "44100", Value = "44100" });
            SamplingRateList.Add(new KeyValuePairModel() { Key = "48000", Value = "48000" });

            AudioBitRateList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "24K", Value = "24K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "32K", Value = "32K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "64K", Value = "64K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "128K", Value = "128K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "192K", Value = "192K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "224K", Value = "224K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "256K", Value = "256K" });
            AudioBitRateList.Add(new KeyValuePairModel() { Key = "320K", Value = "320K" });

            SoundTrackList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            SoundTrackList.Add(new KeyValuePairModel() { Key = "1", Value = string.Format("{0} {1}", 1, MonoString) });
            SoundTrackList.Add(new KeyValuePairModel() { Key = "2", Value = string.Format("{0} {1}", 2, StereoString) });
            SoundTrackList.Add(new KeyValuePairModel() { Key = "4", Value = string.Format("{0} {1}", 4, QuadString) });
            SoundTrackList.Add(new KeyValuePairModel() { Key = "6", Value = string.Format("{0} {1}", 6, Stereo51String) });
            SoundTrackList.Add(new KeyValuePairModel() { Key = "8", Value = string.Format("{0} {1}", 8, Stereo71String) });

            VolumeList.Add(new KeyValuePairModel() { Key = "10%", Value = "10%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "25%", Value = "25%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "50%", Value = "50%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "75%", Value = "75%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "100%", Value = "100%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "150%", Value = "150%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "200%", Value = "200%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "300%", Value = "300%" });
            VolumeList.Add(new KeyValuePairModel() { Key = "400%", Value = "400%" });

            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "1", Value = "1" + SecondString });
            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "2", Value = "2" + SecondString });
            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "3", Value = "3" + SecondString });
            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "4", Value = "4" + SecondString });
            AudioFadeInEffectList.Add(new KeyValuePairModel() { Key = "5", Value = "5" + SecondString });

            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "1", Value = "1" + SecondString });
            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "2", Value = "2" + SecondString });
            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "3", Value = "3" + SecondString });
            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "4", Value = "4" + SecondString });
            AudioFadeOutEffectList.Add(new KeyValuePairModel() { Key = "5", Value = "5" + SecondString });

            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "Default", Value = DefaultString });
            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "None", Value = NoneString });
            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "Embedded", Value = "Embedded" });
            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "Ansi", Value = "Ansi" });
            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "Unicode", Value = "Unicode" });
            SubtitleNestTypeList.Add(new KeyValuePairModel() { Key = "UTF8", Value = "UTF8" });

            FontSizeList.Add(new KeyValuePairModel() { Key = "1", Value = string.Format("{0} {1}", 1, SmallString) });
            FontSizeList.Add(new KeyValuePairModel() { Key = "2", Value = "2" });
            FontSizeList.Add(new KeyValuePairModel() { Key = "3", Value = string.Format("{0} {1}", 3, NormalString) });
            FontSizeList.Add(new KeyValuePairModel() { Key = "4", Value = "4" });
            FontSizeList.Add(new KeyValuePairModel() { Key = "5", Value = string.Format("{0} {1}", 1, LargeString) });
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
            AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
            AppWindow.TitleBar.InactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
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
