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

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 音频转换输出配置窗口
    /// </summary>
    public sealed partial class AudioConversionOutputConfigurationWindow : Window, INotifyPropertyChanged
    {
        private readonly string CloseString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Close");
        private readonly string CopyString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Copy");
        private readonly string DefaultString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Default");
        private readonly string MonoString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Mono");
        private readonly string NoneString = ResourceService.AudioConversionOutputConfigurationResource.GetString("None");
        private readonly string SecondString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Second");
        private readonly string StereoString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo71");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC audioConversionOutputConfigurationWindowSubClassProc;
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

        private AudioConversionTypeKind _selectedAudioConversionTypeKind;

        public AudioConversionTypeKind SelectedAudioConversionTypeKind
        {
            get { return _selectedAudioConversionTypeKind; }

            set
            {
                if (!Equals(_selectedAudioConversionTypeKind, value))
                {
                    _selectedAudioConversionTypeKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioConversionTypeKind)));
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

        private bool _isVariableBitRateSupported;

        public bool IsVariableBitRateSupported
        {
            get { return _isVariableBitRateSupported; }

            set
            {
                if (!Equals(_isVariableBitRateSupported, value))
                {
                    _isVariableBitRateSupported = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVariableBitRateSupported)));
                }
            }
        }

        private ComboBoxItemModel _selectedVariableBitRate;

        public ComboBoxItemModel SelectedVariableBitRate
        {
            get { return _selectedVariableBitRate; }

            set
            {
                if (!Equals(_selectedVariableBitRate, value))
                {
                    _selectedVariableBitRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVariableBitRate)));
                }
            }
        }

        private ComboBoxItemModel _selectedSamplingFormat;

        public ComboBoxItemModel SelectedSamplingFormat
        {
            get { return _selectedSamplingFormat; }

            set
            {
                if (!Equals(_selectedSamplingFormat, value))
                {
                    _selectedSamplingFormat = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSamplingFormat)));
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

        public List<ComboBoxItemModel> FormatConversionTypeList { get; } =
        [
            new ComboBoxItemModel(){ SelectedValue = "MP3", DisplayMember = ".mp3" },
            new ComboBoxItemModel(){ SelectedValue = "WMA", DisplayMember =  ".wma" },
            new ComboBoxItemModel(){ SelectedValue = "M4A", DisplayMember =  ".m4a" },
            new ComboBoxItemModel(){ SelectedValue = "FLAC",DisplayMember =  ".flac" },
            new ComboBoxItemModel(){ SelectedValue = "WAV", DisplayMember =  ".wav" },
            new ComboBoxItemModel(){ SelectedValue = "AC3", DisplayMember =  ".ac3" },
            new ComboBoxItemModel(){ SelectedValue = "AAC", DisplayMember =  ".aac" },
            new ComboBoxItemModel(){ SelectedValue = "DTS",DisplayMember =  ".dts" },
            new ComboBoxItemModel(){ SelectedValue = "MMF", DisplayMember =  ".mmf" },
            new ComboBoxItemModel(){ SelectedValue = "M4R", DisplayMember =  ".m4r" },
            new ComboBoxItemModel(){ SelectedValue = "MP2", DisplayMember =  ".mp2" },
            new ComboBoxItemModel(){ SelectedValue = "OGG", DisplayMember =  ".ogg" },
            new ComboBoxItemModel(){ SelectedValue = "WV", DisplayMember =  ".wv" },
        ];

        public WinRTObservableCollection<ComboBoxItemModel> AudioEncodingCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SamplingRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioBitRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SoundTrackCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VolumeCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VariableBitRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SamplingFormatCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioFadeInEffectCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> AudioFadeOutEffectCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioConversionOutputConfigurationWindow(AudioConversionTypeKind audioConversionTypeKind, ConversionToolsWindow conversionToolsWindow, AudioConversionOutputConfigurationModel audioConversionOutputConfiguration = null)
        {
            SelectedAudioConversionTypeKind = audioConversionTypeKind;
            InitializeData(audioConversionOutputConfiguration);
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
        /// 格式转换类型菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFormatConversionTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel formatConversionType && !Equals(SelectedFormatConversionType, formatConversionType))
            {
                SelectedFormatConversionType = formatConversionType;

                ResetAudioEncoding();
                SelectedAudioEncoding = AudioEncodingCollection[0];

                ResetSamplingRate();
                SelectedSamplingRate = SamplingRateCollection[0];

                ResetAudioBitRate();
                SelectedAudioBitRate = AudioBitRateCollection[0];

                ResetSoundTrack();
                SelectedSoundTrack = SoundTrackCollection[0];

                IsVariableBitRateSupported = Equals(SelectedFormatConversionType, FormatConversionTypeList[0]);
                ResetVariableBitRate();
                SelectedVariableBitRate = null;
                if (VariableBitRateCollection.Count > 0)
                {
                    SelectedVariableBitRate = VariableBitRateCollection[0];
                }

                ResetSamplingFormat();
                SelectedSamplingFormat = SamplingFormatCollection[0];

                SelectedAudioFadeInEffect = AudioFadeInEffectCollection[0];
                SelectedAudioFadeOutEffect = AudioFadeOutEffectCollection[0];

                CloseSoundEffect = false;
                Echo = false;
                DeNoise = false;
                Reverse = false;
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

                ResetSamplingFormat();

                SelectedSamplingFormat = SamplingFormatCollection[0];
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
        /// 可变采样率菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnVariableBitRateSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel variableBitRate && !Equals(SelectedVariableBitRate, variableBitRate))
            {
                SelectedVariableBitRate = variableBitRate;
            }
        }

        /// <summary>
        /// 采样格式菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSamplingFormatSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel samplingFormat && !Equals(SelectedSamplingFormat, samplingFormat))
            {
                SelectedSamplingFormat = samplingFormat;
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
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(AudioConversionOutputConfigurationPage, "BackgroundDefault", false);
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
        /// 音频转换输出配置窗口消息处理
        /// </summary>
        private nint AudioConversionOutputConfigurationWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
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
                                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionOutputConfigurationWindow), nameof(AudioConversionOutputConfigurationWindowSubClassProc), 1, e);
                            }
                        }, null);
                        break;
                    }
                // 窗口销毁后触发的消息
                case WindowMessage.WM_DESTROY:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        BackdropService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, audioConversionOutputConfigurationWindowSubClassProc, 0);
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
        private void InitializeData(AudioConversionOutputConfigurationModel audioConversionOutputConfiguration = null)
        {
            SelectedFormatConversionType = audioConversionOutputConfiguration is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.FormatConversionType)) is ComboBoxItemModel selectedFormatConversionType ? selectedFormatConversionType : FormatConversionTypeList[0];

            ResetAudioEncoding();
            SelectedAudioEncoding = audioConversionOutputConfiguration is not null && AudioEncodingCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioEncoding)) is ComboBoxItemModel selectedAudioEncoding ? selectedAudioEncoding : AudioEncodingCollection[0];

            ResetSamplingRate();
            SelectedSamplingRate = audioConversionOutputConfiguration is not null && SamplingRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.SamplingRate)) is ComboBoxItemModel selectedSamplingRate ? selectedSamplingRate : SamplingRateCollection[0];

            ResetAudioBitRate();
            SelectedAudioBitRate = audioConversionOutputConfiguration is not null && AudioBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioBitRate)) is ComboBoxItemModel selectedAudioBitRate ? selectedAudioBitRate : AudioBitRateCollection[0];

            ResetSoundTrack();
            SelectedSoundTrack = audioConversionOutputConfiguration is not null && SoundTrackCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.SoundTrack)) is ComboBoxItemModel selectedSoundTrack ? selectedSoundTrack : SoundTrackCollection[0];

            if (audioConversionOutputConfiguration is not null)
            {
                CloseSoundEffect = audioConversionOutputConfiguration.CloseSoundEffect;
            }

            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "10%", DisplayMember = "10%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "25%", DisplayMember = "25%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "50%", DisplayMember = "50%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "75%", DisplayMember = "75%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "100%", DisplayMember = "100%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "150%", DisplayMember = "150%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "200%", DisplayMember = "200%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "300%", DisplayMember = "300%" });
            VolumeCollection.Add(new ComboBoxItemModel() { SelectedValue = "400%", DisplayMember = "400%" });
            SelectedVolume = audioConversionOutputConfiguration is not null && VolumeCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.Volume)) is ComboBoxItemModel selectedVolume ? selectedVolume : VolumeCollection[4];

            IsVariableBitRateSupported = Equals(SelectedFormatConversionType, FormatConversionTypeList[0]);
            ResetVariableBitRate();
            SelectedVariableBitRate = null;
            if (VariableBitRateCollection.Count > 0)
            {
                SelectedVariableBitRate = audioConversionOutputConfiguration is not null && VariableBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.VariableBitRate)) is ComboBoxItemModel selectedVariableBitRate ? selectedVariableBitRate : VariableBitRateCollection[0];
            }

            ResetSamplingFormat();
            SelectedSamplingFormat = audioConversionOutputConfiguration is not null && SamplingFormatCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.SamplingFormat)) is ComboBoxItemModel selectedSamplingFormat ? selectedSamplingFormat : SamplingFormatCollection[0];

            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            AudioFadeInEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
            SelectedAudioFadeInEffect = audioConversionOutputConfiguration is not null && AudioFadeInEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioFadeInEffect)) is ComboBoxItemModel selectedAudioFadeInEffect ? selectedAudioFadeInEffect : AudioFadeInEffectCollection[0];

            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            AudioFadeOutEffectCollection.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
            SelectedAudioFadeOutEffect = audioConversionOutputConfiguration is not null && AudioFadeOutEffectCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioFadeOutEffect)) is ComboBoxItemModel selectedAudioFadeOutEffect ? selectedAudioFadeOutEffect : AudioFadeOutEffectCollection[0];

            if (audioConversionOutputConfiguration is not null)
            {
                Echo = audioConversionOutputConfiguration.Echo;
                DeNoise = audioConversionOutputConfiguration.DeNoise;
                Reverse = audioConversionOutputConfiguration.Reverse;
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
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "MP3", DisplayMember = "MP3" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AC3", DisplayMember = "AC3" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "WMAv2", DisplayMember = "WMAv2" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = CopyString });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "ALAC", DisplayMember = "ALAC" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "AAC", DisplayMember = "AAC" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Copy", DisplayMember = "Copy" });
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "FLAC", DisplayMember = "FLAC" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "DCA", DisplayMember = "DCA" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                AudioEncodingCollection.Add(new ComboBoxItemModel() { SelectedValue = "Vorbis", DisplayMember = "Vorbis" });
            }
        }

        /// <summary>
        /// 重置采样率
        /// </summary>
        private void ResetSamplingRate()
        {
            SamplingRateCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "8000", DisplayMember = "8000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "11025", DisplayMember = "11025" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "12000", DisplayMember = "12000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "16000", DisplayMember = "16000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "22050", DisplayMember = "22050" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "24000", DisplayMember = "24000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "32000", DisplayMember = "32000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "44100", DisplayMember = "44100" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "48000", DisplayMember = "48000" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "11025", DisplayMember = "11025" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "22050", DisplayMember = "22050" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "44100", DisplayMember = "44100" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "7350", DisplayMember = "7350" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "8000", DisplayMember = "8000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "11025", DisplayMember = "11025" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "12000", DisplayMember = "12000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "16000", DisplayMember = "16000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "22050", DisplayMember = "22050" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "24000", DisplayMember = "24000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "32000", DisplayMember = "32000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "44100", DisplayMember = "44100" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "48000", DisplayMember = "48000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "64000", DisplayMember = "64000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "88200", DisplayMember = "88200" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "96000", DisplayMember = "96000" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[8]))
            {
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "4000", DisplayMember = "4000" });
                SamplingRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "8000", DisplayMember = "8000" });
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
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "32K", DisplayMember = "32K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "64K", DisplayMember = "64K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "96K", DisplayMember = "96K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "128K", DisplayMember = "128K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "192K", DisplayMember = "192K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "224K", DisplayMember = "224K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "320K", DisplayMember = "320K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]))
            {
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "32K", DisplayMember = "32K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "64K", DisplayMember = "64K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "96K", DisplayMember = "96K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "128K", DisplayMember = "128K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "192K", DisplayMember = "192K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "224K", DisplayMember = "224K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "256K", DisplayMember = "256K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "320K", DisplayMember = "320K" });
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "512K", DisplayMember = "512K" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                AudioBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
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
                Equals(SelectedFormatConversionType, FormatConversionTypeList[2]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]))
            {
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = string.Format("{0} {1}", 1, MonoString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = string.Format("{0} {1}", 2, StereoString) });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = string.Format("{0} {1}", 1, MonoString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = string.Format("{0} {1}", 2, StereoString) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "6", DisplayMember = string.Format("{0} {1}", 6, Stereo51String) });
                SoundTrackCollection.Add(new ComboBoxItemModel() { SelectedValue = "8", DisplayMember = string.Format("{0} {1}", 8, Stereo71String) });
            }
        }

        /// <summary>
        /// 重置可变采样率
        /// </summary>
        private void ResetVariableBitRate()
        {
            VariableBitRateCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]))
            {
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "Close", DisplayMember = CloseString });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "0", DisplayMember = "0 (AVG 245K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1 (AVG 225K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2 (AVG 190K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3 (AVG 175K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4 (AVG 165K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5 (AVG 130K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "6", DisplayMember = "6 (AVG 115K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "7", DisplayMember = "7 (AVG 100K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "8", DisplayMember = "8 (AVG 85K)" });
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "9", DisplayMember = "9 (AVG 65K)" });
            }
            else
            {
                VariableBitRateCollection.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            }
        }

        /// <summary>
        /// 重置采样方式
        /// </summary>
        private void ResetSamplingFormat()
        {
            SamplingFormatCollection.Clear();

            if (Equals(SelectedFormatConversionType, FormatConversionTypeList[0]))
            {
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s16p", DisplayMember = "s16p" });
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "fltp", DisplayMember = "fltp" });
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s32p", DisplayMember = "s32p" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[2]))
            {
                if (Equals(SelectedAudioEncoding, AudioEncodingCollection[0]))
                {
                    SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                    SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s16p", DisplayMember = "s16p" });
                    SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s32p", DisplayMember = "s32p" });
                }
                else if (Equals(SelectedAudioEncoding, AudioEncodingCollection[1]) ||
                    Equals(SelectedAudioEncoding, AudioEncodingCollection[2]))
                {
                    SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                }
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[3]))
            {
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s16", DisplayMember = "s16" });
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "s32", DisplayMember = "s32" });
            }
            else if (Equals(SelectedFormatConversionType, FormatConversionTypeList[1]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[4]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[5]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[6]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[7]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[8]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[9]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[10]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[11]) ||
                Equals(SelectedFormatConversionType, FormatConversionTypeList[12]))
            {
                SamplingFormatCollection.Add(new ComboBoxItemModel() { SelectedValue = "Default", DisplayMember = DefaultString });
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
            SelectedItem = AudioConversionOutputConfigurationSelectorBar.Items[0];

            // 挂载相应的事件
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            audioConversionOutputConfigurationWindowSubClassProc = new SUBCLASSPROC(AudioConversionOutputConfigurationWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, audioConversionOutputConfigurationWindowSubClassProc, 0, 0);

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
