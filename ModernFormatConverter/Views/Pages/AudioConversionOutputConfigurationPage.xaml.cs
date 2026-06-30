using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 音频转换输出配置页面
    /// </summary>
    public sealed partial class AudioConversionOutputConfigurationPage : Page, INotifyPropertyChanged
    {
        private readonly string CloseString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Close");
        private readonly string CopyString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Copy");
        private readonly string DefaultString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Default");
        private readonly string MonoString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Mono");
        private readonly string NoneString = ResourceService.AudioConversionOutputConfigurationResource.GetString("None");
        private readonly string SecondString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Second");
        private readonly string SelectFolderString = ResourceService.AudioConversionOutputConfigurationResource.GetString("SelectFolder");
        private readonly string StereoString = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo");
        private readonly string Stereo51String = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo51");
        private readonly string Stereo71String = ResourceService.AudioConversionOutputConfigurationResource.GetString("Stereo71");
        private AudioConversionNavigationParameter audioConversionNavigationParameter;

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

        private string _outputFolder;

        public string OutputFolder
        {
            get { return _outputFolder; }

            set
            {
                if (!Equals(_outputFolder, value))
                {
                    _outputFolder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputFolder)));
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

        public List<ComboBoxItemModel> VolumeList { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> VariableBitRateCollection { get; } = [];

        public WinRTObservableCollection<ComboBoxItemModel> SamplingFormatCollection { get; } = [];

        public List<ComboBoxItemModel> AudioFadeInEffectList { get; } = [];

        public List<ComboBoxItemModel> AudioFadeOutEffectList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioConversionOutputConfigurationPage()
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

            SelectedItem = AudioConversionOutputConfigurationSelectorBar.Items[0];
            if (args.Parameter is AudioConversionNavigationParameter audioConversionNavigationParameterData)
            {
                audioConversionNavigationParameter = audioConversionNavigationParameterData;
                SelectedAudioConversionTypeKind = audioConversionNavigationParameter.AudioConversionTypeKind;

                // 音频格式转换
                if (SelectedAudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                {
                    if (audioConversionNavigationParameter.IsGlobalSettings)
                    {
                        UpdateData(null);
                    }
                    else
                    {
                        if (audioConversionNavigationParameter.AudioConversionData is AudioFormatConversionFileModel audioFormatConversionFile && audioFormatConversionFile.AudioConversionOutputConfiguration is not null)
                        {
                            UpdateData(audioFormatConversionFile.AudioConversionOutputConfiguration);
                        }
                    }
                }
                // 音频合并
                else if (SelectedAudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
                {
                    if (audioConversionNavigationParameter.IsGlobalSettings && audioConversionNavigationParameter.AudioConversionData is AudioConcatModel audioConcat && audioConcat.AudioConversionOutputConfiguration is not null)
                    {
                        UpdateData(audioConcat.AudioConversionOutputConfiguration);
                    }
                }
            }
        }

        /// <summary>
        /// 离开该页面触发的事件
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            audioConversionNavigationParameter = null;
        }

        #endregion 第一部分：重载父类事件

        #region 第二部分：音频转换输出配置页面——挂载的事件

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 更新数据
            // 音频格式转换
            if (SelectedAudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
            {
                if (audioConversionNavigationParameter.IsGlobalSettings)
                {
                    if (audioConversionNavigationParameter.AudioConversionData is List<AudioFormatConversionFileModel> audioFormatConversionFileList)
                    {
                        foreach (AudioFormatConversionFileModel audioFormatConversionFile in audioFormatConversionFileList)
                        {
                            if (audioFormatConversionFile.AudioConversionOutputConfiguration is not null)
                            {
                                audioFormatConversionFile.AudioConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.VariableBitRate = IsVariableBitRateSupported ? Convert.ToString(SelectedVariableBitRate.SelectedValue) : string.Empty;
                                audioFormatConversionFile.AudioConversionOutputConfiguration.SamplingFormat = Convert.ToString(SelectedSamplingFormat);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                                audioFormatConversionFile.AudioConversionOutputConfiguration.Echo = Echo;
                                audioFormatConversionFile.AudioConversionOutputConfiguration.DeNoise = DeNoise;
                                audioFormatConversionFile.AudioConversionOutputConfiguration.Reverse = Reverse;
                                audioFormatConversionFile.AudioConversionOutputConfiguration.OutputFolder = OutputFolder;
                            }
                        }
                    }
                }
                else
                {
                    if (audioConversionNavigationParameter.AudioConversionData is AudioFormatConversionFileModel audioFormatConversionFile && audioFormatConversionFile.AudioConversionOutputConfiguration is not null)
                    {
                        audioFormatConversionFile.AudioConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.VariableBitRate = IsVariableBitRateSupported ? Convert.ToString(SelectedVariableBitRate.SelectedValue) : string.Empty;
                        audioFormatConversionFile.AudioConversionOutputConfiguration.SamplingFormat = Convert.ToString(SelectedSamplingFormat);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                        audioFormatConversionFile.AudioConversionOutputConfiguration.Echo = Echo;
                        audioFormatConversionFile.AudioConversionOutputConfiguration.DeNoise = DeNoise;
                        audioFormatConversionFile.AudioConversionOutputConfiguration.Reverse = Reverse;
                        audioFormatConversionFile.AudioConversionOutputConfiguration.OutputFolder = OutputFolder;
                    }
                }
            }
            // 音频合并
            else if (SelectedAudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
            {
                if (audioConversionNavigationParameter.IsGlobalSettings && audioConversionNavigationParameter.AudioConversionData is AudioConcatModel audioConcat && audioConcat.AudioConversionOutputConfiguration is not null)
                {
                    audioConcat.AudioConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.AudioEncoding = Convert.ToString(SelectedAudioEncoding.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.SamplingRate = Convert.ToString(SelectedSamplingRate.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.AudioBitRate = Convert.ToString(SelectedAudioBitRate.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.SoundTrack = Convert.ToString(SelectedSoundTrack.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.Volume = Convert.ToString(SelectedVolume.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.VariableBitRate = IsVariableBitRateSupported ? Convert.ToString(SelectedVariableBitRate.SelectedValue) : string.Empty;
                    audioConcat.AudioConversionOutputConfiguration.SamplingFormat = Convert.ToString(SelectedSamplingFormat);
                    audioConcat.AudioConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(SelectedAudioFadeInEffect.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(SelectedAudioFadeOutEffect.SelectedValue);
                    audioConcat.AudioConversionOutputConfiguration.Echo = Echo;
                    audioConcat.AudioConversionOutputConfiguration.DeNoise = DeNoise;
                    audioConcat.AudioConversionOutputConfiguration.Reverse = Reverse;
                    audioConcat.AudioConversionOutputConfiguration.OutputFolder = OutputFolder;
                }
            }

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage)
            {
                audioConversionPage.NavigateTo(audioConversionPage.PageList[0], null, false);
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

                SelectedAudioFadeInEffect = AudioFadeInEffectList[0];
                SelectedAudioFadeOutEffect = AudioFadeOutEffectList[0];

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
            if (sender is ToggleSwitch toggleSwitch && !Equals(Echo, toggleSwitch.IsOn))
            {
                Echo = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 是否启用降噪
        /// </summary>
        private void OnDeNoiseToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch && !Equals(DeNoise, toggleSwitch.IsOn))
            {
                DeNoise = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 是否启用反向
        /// </summary>
        private void OnReverseToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch && !Equals(Reverse, toggleSwitch.IsOn))
            {
                Reverse = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 打开输出文件夹
        /// </summary>
        private void OnOpenOutputFolderClicked(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(OutputFolder);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionOutputConfigurationPage), nameof(OnOpenOutputFolderClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 修改输出的文件夹
        /// </summary>
        private void OnOutputChangeFolderClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is string tag)
            {
                switch (tag)
                {
                    case "AppCache":
                        {
                            Shell32Library.SHGetKnownFolderPath(new("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"), KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION, 0, out string localAppDataPath);
                            OutputFolder = Path.Combine(localAppDataPath, "Audios");
                            break;
                        }
                    case "Music":
                        {
                            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            OutputFolder = musicFolder;
                            break;
                        }
                    case "Desktop":
                        {
                            OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            break;
                        }
                    case "Custom":
                        {
                            OpenFolderDialog openFolderDialog = new((nint)MainWindow.Current.AppWindow.Id.Value)
                            {
                                Description = SelectFolderString,
                                RootFolder = Environment.SpecialFolder.Desktop
                            };
                            DialogResult dialogResult = openFolderDialog.ShowDialog();
                            if (dialogResult is DialogResult.OK || dialogResult is DialogResult.Yes)
                            {
                                OutputFolder = openFolderDialog.SelectedPath;
                            }
                            openFolderDialog.Dispose();
                            break;
                        }
                }
            }
        }

        #endregion 第二部分：音频转换输出配置页面——挂载的事件

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(AudioConversionOutputConfigurationModel audioConversionOutputConfiguration = null)
        {
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "10%", DisplayMember = "10%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "25%", DisplayMember = "25%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "50%", DisplayMember = "50%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "75%", DisplayMember = "75%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "100%", DisplayMember = "100%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "150%", DisplayMember = "150%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "200%", DisplayMember = "200%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "300%", DisplayMember = "300%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "400%", DisplayMember = "400%" });

            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            AudioFadeInEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });

            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "None", DisplayMember = NoneString });
            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "1", DisplayMember = "1" + SecondString });
            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "2", DisplayMember = "2" + SecondString });
            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "3", DisplayMember = "3" + SecondString });
            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "4", DisplayMember = "4" + SecondString });
            AudioFadeOutEffectList.Add(new ComboBoxItemModel() { SelectedValue = "5", DisplayMember = "5" + SecondString });
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(AudioConversionOutputConfigurationModel audioConversionOutputConfiguration)
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

            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "10%", DisplayMember = "10%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "25%", DisplayMember = "25%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "50%", DisplayMember = "50%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "75%", DisplayMember = "75%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "100%", DisplayMember = "100%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "150%", DisplayMember = "150%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "200%", DisplayMember = "200%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "300%", DisplayMember = "300%" });
            VolumeList.Add(new ComboBoxItemModel() { SelectedValue = "400%", DisplayMember = "400%" });
            SelectedVolume = audioConversionOutputConfiguration is not null && VolumeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.Volume)) is ComboBoxItemModel selectedVolume ? selectedVolume : VolumeList[4];

            IsVariableBitRateSupported = Equals(SelectedFormatConversionType, FormatConversionTypeList[0]);
            ResetVariableBitRate();
            SelectedVariableBitRate = null;
            if (VariableBitRateCollection.Count > 0)
            {
                SelectedVariableBitRate = audioConversionOutputConfiguration is not null && VariableBitRateCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.VariableBitRate)) is ComboBoxItemModel selectedVariableBitRate ? selectedVariableBitRate : VariableBitRateCollection[0];
            }

            ResetSamplingFormat();
            SelectedSamplingFormat = audioConversionOutputConfiguration is not null && SamplingFormatCollection.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.SamplingFormat)) is ComboBoxItemModel selectedSamplingFormat ? selectedSamplingFormat : SamplingFormatCollection[0];

            SelectedAudioFadeInEffect = audioConversionOutputConfiguration is not null && AudioFadeInEffectList.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioFadeInEffect)) is ComboBoxItemModel selectedAudioFadeInEffect ? selectedAudioFadeInEffect : AudioFadeInEffectList[0];

            SelectedAudioFadeOutEffect = audioConversionOutputConfiguration is not null && AudioFadeOutEffectList.FirstOrDefault(item => string.Equals(Convert.ToString(item.SelectedValue), audioConversionOutputConfiguration.AudioFadeOutEffect)) is ComboBoxItemModel selectedAudioFadeOutEffect ? selectedAudioFadeOutEffect : AudioFadeOutEffectList[0];

            if (audioConversionOutputConfiguration is not null)
            {
                Echo = audioConversionOutputConfiguration.Echo;
                DeNoise = audioConversionOutputConfiguration.DeNoise;
                Reverse = audioConversionOutputConfiguration.Reverse;
            }

            OutputFolder = audioConversionOutputConfiguration is not null && !string.IsNullOrEmpty(audioConversionOutputConfiguration.OutputFolder) ? audioConversionOutputConfiguration.OutputFolder : ConvertConfigurationService.ConvertedAudioSavePath;
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
    }
}
