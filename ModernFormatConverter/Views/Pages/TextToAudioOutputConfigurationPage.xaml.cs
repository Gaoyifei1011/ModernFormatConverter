using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Helpers.Reflection;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading.Tasks;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 文本转语音输出配置页面
    /// </summary>
    public sealed partial class TextToAudioOutputConfigurationPage : Page, INotifyPropertyChanged
    {
        private readonly bool isInitialized;
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

        private bool _isVoiceExisted;

        public bool IsVoiceExisted
        {
            get { return _isVoiceExisted; }

            set
            {
                if (!Equals(_isVoiceExisted, value))
                {
                    _isVoiceExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVoiceExisted)));
                }
            }
        }

        private VoiceTypeModel _selectedVoiceType;

        public VoiceTypeModel SelectedVoiceType
        {
            get { return _selectedVoiceType; }

            set
            {
                if (!Equals(_selectedVoiceType, value))
                {
                    _selectedVoiceType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVoiceType)));
                }
            }
        }

        private int _readingSpeed;

        public int ReadingSpeed
        {
            get { return _readingSpeed; }

            set
            {
                if (!Equals(_readingSpeed, value))
                {
                    _readingSpeed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReadingSpeed)));
                }
            }
        }

        private int _volume;

        public int Volume
        {
            get { return _volume; }

            set
            {
                if (!Equals(_volume, value))
                {
                    _volume = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                }
            }
        }

        public List<VoiceTypeModel> VoiceTypeList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public TextToAudioOutputConfigurationPage()
        {
            InitializeComponent();
            isInitialized = true;
        }

        #region 第一部分：重载父类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            SelectedItem = TextToAudioOutputConfigurationSelectorBar.Items[0];
            if (args.Parameter is AudioConversionNavigationParameter audioConversionNavigationParameterData)
            {
                audioConversionNavigationParameter = audioConversionNavigationParameterData;
                // 语音转文本
                if (audioConversionNavigationParameter.IsGlobalSettings && audioConversionNavigationParameter.AudioConversionData is TextToAudioModel textToAudio && textToAudio.TextToAudioOutputConfiguration is not null)
                {
                    UpdateData(textToAudio.TextToAudioOutputConfiguration);
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

        #region 第二部分：文本转语音输出配置页面——挂载的事件

        /// <summary>
        /// 打开系统设置
        /// </summary>
        private void OnSystemSettingsClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("ms-settings:speech");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(TextToAudioOutputConfigurationPage), nameof(OnSystemSettingsClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 更新数据
            // 语音转文本
            if (audioConversionNavigationParameter.IsGlobalSettings && audioConversionNavigationParameter.AudioConversionData is TextToAudioModel textToAudio && textToAudio.TextToAudioOutputConfiguration is not null)
            {
                textToAudio.TextToAudioOutputConfiguration.VoiceType = Convert.ToString(SelectedVoiceType.SelectedValue);
                textToAudio.TextToAudioOutputConfiguration.ReadingSpeed = ReadingSpeed;
                textToAudio.TextToAudioOutputConfiguration.Volume = Volume;
            }

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage)
            {
                audioConversionPage.NavigateTo(audioConversionPage.PageList[0], null, false);
            }
        }

        /// <summary>
        /// 语音类型选中项发生改变时触发的事件
        /// </summary>
        private void OnSelectedVoiceTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is VoiceTypeModel voiceType && !Equals(SelectedVoiceType, voiceType))
            {
                SelectedVoiceType = voiceType;
            }
        }

        /// <summary>
        /// 阅读速率发生变化时触发的事件
        /// </summary>
        private void OnReadingSpeedValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    ReadingSpeed = Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(TextToAudioOutputConfigurationPage), nameof(OnReadingSpeedValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 音量发生变化时触发的事件
        /// </summary>
        private void OnVolumeValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    Volume = Convert.ToInt32(args.NewValue);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(TextToAudioOutputConfigurationPage), nameof(OnVolumeValueChanged), 1, e);
                }
            }
        }

        #endregion 第二部分：文本转语音输出配置页面——挂载的事件

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(TextToAudioOutputConfigurationModel textToAudioOutputConfiguration)
        {
            List<VoiceInfo> voiceInfoList = [];

            try
            {
                SpeechSynthesizer speechSynthesizer = new();
                speechSynthesizer.InjectOneCoreVoices();

                foreach (InstalledVoice installedVoice in speechSynthesizer.GetInstalledVoices())
                {
                    if (installedVoice.Enabled)
                    {
                        voiceInfoList.Add(installedVoice.VoiceInfo);
                    }
                }

                speechSynthesizer.Dispose();
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(TextToAudioOutputConfigurationPage), nameof(UpdateData), 1, e);
            }

            if (voiceInfoList.Count > 0)
            {
                IsVoiceExisted = true;
                VoiceTypeList.Clear();
                foreach (VoiceInfo voiceInfo in voiceInfoList)
                {
                    VoiceTypeList.Add(new VoiceTypeModel()
                    {
                        DisplayMember = voiceInfo.Name,
                        SelectedValue = voiceInfo.Id,
                        VoiceInfo = voiceInfo
                    });
                }

                SelectedVoiceType = textToAudioOutputConfiguration is not null && VoiceTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), textToAudioOutputConfiguration.VoiceType)) is VoiceTypeModel selectedVoiceInfo ? selectedVoiceInfo : VoiceTypeList[0];
            }
            else
            {
                IsVoiceExisted = false;
            }

            ReadingSpeed = textToAudioOutputConfiguration is not null ? textToAudioOutputConfiguration.ReadingSpeed : 0;
            Volume = textToAudioOutputConfiguration is not null ? textToAudioOutputConfiguration.Volume : 100;
        }
    }
}
