using FFmpegInterop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.PInvoke.MediaInfo;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 音频编辑页面
    /// </summary>
    public sealed partial class AudioEditPage : Page, INotifyPropertyChanged
    {
        private readonly string SelectFileString = ResourceService.AudioEditResource.GetString("SelectFile");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private AudioFormatConversionFileModel selectedAudioFormatConversionFile;
        private IRandomAccessStream audioRandomAccessStream;

        private MediaSource _mediaSource;

        public MediaSource MediaSource
        {
            get { return _mediaSource; }

            set
            {
                if (!Equals(_mediaSource, value))
                {
                    _mediaSource = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaSource)));
                }
            }
        }

        private string _errorReason;

        public string ErrorReason
        {
            get { return _errorReason; }

            set
            {
                if (!string.Equals(_errorReason, value))
                {
                    _errorReason = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorReason)));
                }
            }
        }

        private AudioEditResultKind _audioEditResultKind;

        public AudioEditResultKind AudioEditResultKind
        {
            get { return _audioEditResultKind; }

            set
            {
                if (!Equals(_audioEditResultKind, value))
                {
                    _audioEditResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioEditResultKind)));
                }
            }
        }

        private string _fileName;

        public string FileName
        {
            get { return _fileName; }

            set
            {
                if (!Equals(_fileName, value))
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        private SelectorBarItem _audioEditSelectedItem;

        public SelectorBarItem AudioEditSelectedItem
        {
            get { return _audioEditSelectedItem; }

            set
            {
                if (!Equals(_audioEditSelectedItem, value))
                {
                    _audioEditSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioEditSelectedItem)));
                }
            }
        }

        private int _timeStartHours;

        public int TimeStartHours
        {
            get { return _timeStartHours; }

            set
            {
                if (!Equals(_timeStartHours, value))
                {
                    _timeStartHours = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeStartHours)));
                }
            }
        }

        private int _timeStartMinutes;

        public int TimeStartMinutes
        {
            get { return _timeStartMinutes; }

            set
            {
                if (!Equals(_timeStartMinutes, value))
                {
                    _timeStartMinutes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeStartMinutes)));
                }
            }
        }

        private int _timeStartSeconds;

        public int TimeStartSeconds
        {
            get { return _timeStartSeconds; }

            set
            {
                if (!Equals(_timeStartSeconds, value))
                {
                    _timeStartSeconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeStartSeconds)));
                }
            }
        }

        private int _timeStartMillseconds;

        public int TimeStartMillseconds
        {
            get { return _timeStartMillseconds; }

            set
            {
                if (!Equals(_timeStartMillseconds, value))
                {
                    _timeStartMillseconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeStartMillseconds)));
                }
            }
        }

        private int _timeEndHours;

        public int TimeEndHours
        {
            get { return _timeEndHours; }

            set
            {
                if (!Equals(_timeEndHours, value))
                {
                    _timeEndHours = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEndHours)));
                }
            }
        }

        private int _timeEndMinutes;

        public int TimeEndMinutes
        {
            get { return _timeEndMinutes; }

            set
            {
                if (!Equals(_timeEndMinutes, value))
                {
                    _timeEndMinutes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEndMinutes)));
                }
            }
        }

        private int _timeEndSeconds;

        public int TimeEndSeconds
        {
            get { return _timeEndSeconds; }

            set
            {
                if (!Equals(_timeEndSeconds, value))
                {
                    _timeEndSeconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEndSeconds)));
                }
            }
        }

        private int _timeEndMillseconds;

        public int TimeEndMillseconds
        {
            get { return _timeEndMillseconds; }

            set
            {
                if (!Equals(_timeEndMillseconds, value))
                {
                    _timeEndMillseconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEndMillseconds)));
                }
            }
        }

        private string _audioCoverFilePath = string.Empty;

        public string AudioCoverFilePath
        {
            get { return _audioCoverFilePath; }

            set
            {
                if (!string.Equals(_audioCoverFilePath, value))
                {
                    _audioCoverFilePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioCoverFilePath)));
                }
            }
        }

        private bool _isLoadingAudioCover;

        public bool IsLoadingAudioCover
        {
            get { return _isLoadingAudioCover; }

            set
            {
                if (!Equals(_isLoadingAudioCover, value))
                {
                    _isLoadingAudioCover = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingAudioCover)));
                }
            }
        }

        private ImageSource _audioCoverImage;

        public ImageSource AudioCoverImage
        {
            get { return _audioCoverImage; }

            set
            {
                if (!Equals(_audioCoverImage, value))
                {
                    _audioCoverImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioCoverImage)));
                }
            }
        }

        private ImageSource _selectedAudioCoverImage;

        public ImageSource SelectedAudioCoverImage
        {
            get { return _selectedAudioCoverImage; }

            set
            {
                if (!Equals(_selectedAudioCoverImage, value))
                {
                    _selectedAudioCoverImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAudioCoverImage)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioEditPage()
        {
            InitializeComponent();
        }

        #region 第一部分：重载分类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            if(args.Parameter is AudioFormatConversionFileModel audioFormatConversionFile)
            {
                selectedAudioFormatConversionFile = audioFormatConversionFile;
                FileName = selectedAudioFormatConversionFile.FileName;
                UpdateData(selectedAudioFormatConversionFile.AudioEdit);
                AudioEditSelectedItem = AudioEditSelectorBar.Items[0];

                if (AudioEditResultKind is AudioEditResultKind.None)
                {
                    AudioEditResultKind = AudioEditResultKind.Loading;
                    AudioEditMediaPlayerElement.MediaPlayer.MediaOpened += OnMediaOpened;
                    AudioEditMediaPlayerElement.MediaPlayer.MediaFailed += OnMediaFailed;
                    GetCoverImage(selectedAudioFormatConversionFile.FilePath);

                    (bool isLoadedSccessfully, MediaSource mediaSource, Exception exception) = await Task.Run(async () =>
                    {
                        try
                        {
                            IActivationFactory activationFactory = WindowsRuntimeMarshal.GetActivationFactory(typeof(MediaStreamSource));
                            MediaStreamSource mediaStreamSource = activationFactory.ActivateInstance() as MediaStreamSource;
                            audioRandomAccessStream = await (await StorageFile.GetFileFromPathAsync(selectedAudioFormatConversionFile.FilePath)).OpenAsync(FileAccessMode.Read);
                            FFmpegInteropMSSConfig ffmpegInteropMSSConfig = new()
                            {
                                ForceVideoDecode = true,
                                ForceAudioDecode = true
                            };
                            FFmpegInteropMSS.InitializeFromStream(audioRandomAccessStream, mediaStreamSource, ffmpegInteropMSSConfig);
                            MediaSource meidaSource = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
                            return ValueTuple.Create<bool, MediaSource, Exception>(true, meidaSource, null);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnNavigatedTo), 1, e);
                            return ValueTuple.Create<bool, MediaSource, Exception>(false, null, e);
                        }
                    });

                    if (isLoadedSccessfully)
                    {
                        MediaSource = mediaSource;
                    }
                    else
                    {
                        AudioEditResultKind = AudioEditResultKind.Failed;
                        MediaSource = null;
                        ErrorReason = exception is not null ? string.Format("0x{0:X8},{1}", exception.HResult, exception.Message) : "N/A";
                    }
                }
            }
        }

        /// <summary>
        /// 离开该页面触发的事件
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            base.OnNavigatingFrom(args);

            try
            {
                AudioEditResultKind = AudioEditResultKind.None;
                MediaSource = null;
                audioRandomAccessStream?.Dispose();
                AudioEditMediaPlayerElement.MediaPlayer.MediaOpened -= OnMediaOpened;
                AudioEditMediaPlayerElement.MediaPlayer.MediaFailed -= OnMediaFailed;
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnNavigatingFrom), 2, e);
            }
        }

        #endregion 第一部分：重载分类事件

        #region 第二部分：音频编辑页面——挂载的事件

        /// <summary>
        /// 音频加载失败后触发的事件
        /// </summary>
        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                AudioEditResultKind = AudioEditResultKind.Failed;
                ErrorReason = args.ExtendedErrorCode is not null ? string.Format("0x{0:X8},{1}", args.ExtendedErrorCode.HResult, args.ExtendedErrorCode.Message) : "N/A";
            }, null);
            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnMediaFailed), 1, args.ExtendedErrorCode is not null ? args.ExtendedErrorCode : new Exception());
        }

        /// <summary>
        /// 音频加载成功后触发的事件
        /// </summary>
        private void OnMediaOpened(MediaPlayer sender, object args)
        {
            sender.SystemMediaTransportControls.IsChannelDownEnabled = true;
            sender.SystemMediaTransportControls.IsChannelUpEnabled = true;
            sender.SystemMediaTransportControls.IsFastForwardEnabled = true;
            sender.SystemMediaTransportControls.IsNextEnabled = true;
            sender.SystemMediaTransportControls.IsPauseEnabled = true;
            sender.SystemMediaTransportControls.IsPlayEnabled = true;
            sender.SystemMediaTransportControls.IsRecordEnabled = true;
            sender.SystemMediaTransportControls.IsRewindEnabled = true;
            sender.SystemMediaTransportControls.IsStopEnabled = true;

            synchronizationContext.Post((_) =>
            {
                AudioEditResultKind = AudioEditResultKind.Successfully;
                ErrorReason = string.Empty;
            }, null);
        }

        /// <summary>
        /// 使用系统播放器播放
        /// </summary>
        private void OnPlayWithSystemAudioClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(selectedAudioFormatConversionFile.FilePath);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnPlayWithSystemAudioClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 音频编辑选中项发生变化时触发的事件
        /// </summary>
        private void OnAudioEditSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (!Equals(sender.SelectedItem, AudioEditSelectedItem))
            {
                AudioEditSelectedItem = sender.SelectedItem;
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 更新数据
            selectedAudioFormatConversionFile.AudioEdit.StartTime = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
            selectedAudioFormatConversionFile.AudioEdit.EndTime = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
            selectedAudioFormatConversionFile.AudioEdit.AudioCoverFilePath = AudioCoverFilePath;
            selectedAudioFormatConversionFile = null;

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage)
            {
                audioConversionPage.NavigateTo(audioConversionPage.PageList[0], null, false);
            }
        }

        /// <summary>
        /// 起始时间点时发生变化时触发的事件
        /// </summary>
        private void OnTimeStartHoursValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeStartHours = int.MaxValue;
                TimeStartHours = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    TimeStartHours = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, newValue, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeStartHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeStartMinutes = totalDuration.Minutes;
                            TimeStartSeconds = totalDuration.Seconds;
                            TimeStartMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeStartHours = newValue;
                        }
                    }
                    else
                    {
                        TimeStartHours = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 起始时间点分发生变化时触发的事件
        /// </summary>
        private void OnTimeStartMinutesValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeStartMinutes = int.MaxValue;
                TimeStartMinutes = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeStartMinutes = 59;
                }
                else if (newValue < 0)
                {
                    TimeStartMinutes = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeStartHours, newValue, TimeStartSeconds, TimeStartMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeStartHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeStartMinutes = totalDuration.Minutes;
                            TimeStartSeconds = totalDuration.Seconds;
                            TimeStartMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeStartMinutes = newValue;
                        }
                    }
                    else
                    {
                        TimeStartMinutes = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 起始时间点秒发生变化时触发的事件
        /// </summary>
        private void OnTimeStartSecondsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeStartSeconds = int.MaxValue;
                TimeStartSeconds = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeStartSeconds = 59;
                }
                else if (newValue < 0)
                {
                    TimeStartSeconds = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeStartHours, TimeStartMinutes, newValue, TimeStartMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeStartHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeStartMinutes = totalDuration.Minutes;
                            TimeStartSeconds = totalDuration.Seconds;
                            TimeStartMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeStartSeconds = newValue;
                        }
                    }
                    else
                    {
                        TimeStartSeconds = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 起始时间点毫秒发生变化时触发的事件
        /// </summary>
        private void OnTimeStartMillsecondsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeStartMillseconds = int.MaxValue;
                TimeStartMillseconds = Convert.ToInt32(args.OldValue);

                if (newValue > 1000)
                {
                    TimeStartMillseconds = 999;
                }
                else if (newValue < 0)
                {
                    TimeStartMillseconds = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, newValue);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeStartHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeStartMinutes = totalDuration.Minutes;
                            TimeStartSeconds = totalDuration.Seconds;
                            TimeStartMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeStartMillseconds = newValue;
                        }
                    }
                    else
                    {
                        TimeStartMillseconds = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 定位起始时间
        /// </summary>
        private async void OnTimeStartLocationClicked(object sender, RoutedEventArgs args)
        {
            TimeSpan currentPosition = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeStartHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeStartMinutes = currentPosition.Minutes;
            TimeStartSeconds = currentPosition.Seconds;
            TimeStartMillseconds = currentPosition.Milliseconds;
        }

        /// <summary>
        /// 从起始位置时间播放音频
        /// </summary>
        private void OnTimeStartLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
        }

        /// <summary>
        /// 结束时间点时发生变化时触发的事件
        /// </summary>
        private void OnTimeEndHoursValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeEndHours = int.MaxValue;
                TimeEndHours = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    TimeEndHours = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, newValue, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeEndHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeEndMinutes = totalDuration.Minutes;
                            TimeEndSeconds = totalDuration.Seconds;
                            TimeEndMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeEndHours = newValue;
                        }
                    }
                    else
                    {
                        TimeEndHours = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 结束时间点分发生变化时触发的事件
        /// </summary>
        private void OnTimeEndMinutesValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeEndMinutes = int.MaxValue;
                TimeEndMinutes = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeEndMinutes = 59;
                }
                else if (newValue < 0)
                {
                    TimeEndMinutes = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeEndHours, newValue, TimeEndSeconds, TimeEndMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeEndHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeEndMinutes = totalDuration.Minutes;
                            TimeEndSeconds = totalDuration.Seconds;
                            TimeEndMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeEndMinutes = newValue;
                        }
                    }
                    else
                    {
                        TimeEndMinutes = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 结束时间点秒发生变化时触发的事件
        /// </summary>
        private void OnTimeEndSecondsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeEndSeconds = int.MaxValue;
                TimeEndSeconds = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeEndSeconds = 59;
                }
                else if (newValue < 0)
                {
                    TimeEndSeconds = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeEndHours, TimeEndMinutes, newValue, TimeEndMillseconds);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeEndHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeEndMinutes = totalDuration.Minutes;
                            TimeEndSeconds = totalDuration.Seconds;
                            TimeEndMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeEndSeconds = newValue;
                        }
                    }
                    else
                    {
                        TimeEndSeconds = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 结束时间点毫秒发生变化时触发的事件
        /// </summary>
        private void OnTimeEndMillsecondsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeEndMillseconds = int.MaxValue;
                TimeEndMillseconds = Convert.ToInt32(args.OldValue);

                if (newValue > 1000)
                {
                    TimeEndMillseconds = 999;
                }
                else if (newValue < 0)
                {
                    TimeEndMillseconds = 0;
                }
                else
                {
                    if (AudioEditResultKind is AudioEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
                        TimeSpan currentDuration = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, newValue);

                        // 防止时间溢出
                        if (totalDuration < currentDuration)
                        {
                            TimeEndHours = Convert.ToInt32(totalDuration.TotalHours);
                            TimeEndMinutes = totalDuration.Minutes;
                            TimeEndSeconds = totalDuration.Seconds;
                            TimeEndMillseconds = totalDuration.Milliseconds;
                        }
                        else
                        {
                            TimeEndMillseconds = newValue;
                        }
                    }
                    else
                    {
                        TimeEndMillseconds = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 定位结束时间
        /// </summary>
        private async void OnTimeEndLocationClicked(object sender, RoutedEventArgs args)
        {
            TimeSpan currentPosition = AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeEndHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeEndMinutes = currentPosition.Minutes;
            TimeEndSeconds = currentPosition.Seconds;
            TimeEndMillseconds = currentPosition.Milliseconds;
        }

        /// <summary>
        /// 从结束位置时间播放音频
        /// </summary>
        private void OnTimeEndLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            AudioEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
        }

        /// <summary>
        /// 选择封面文件
        /// </summary>
        private void OnSelectFileClicked(object sender, RoutedEventArgs args)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName) && File.Exists(openFileDialog.FileName))
            {
                AudioCoverFilePath = openFileDialog.FileName;
                SelectedAudioCoverImage = new BitmapImage() { UriSource = new Uri(AudioCoverFilePath) };
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 清空选中的封面文件
        /// </summary>
        private void OnClearSelectFileClicked(object sender, RoutedEventArgs args)
        {
            AudioCoverFilePath = string.Empty;
            SelectedAudioCoverImage = null;
        }

        /// <summary>
        /// 打开封面文件所在的目录
        /// </summary>
        private void OnAudioCoverFilePathClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(AudioCoverFilePath))
                    {
                        if (File.Exists(AudioCoverFilePath))
                        {
                            nint pidlList = Shell32Library.ILCreateFromPath(AudioCoverFilePath);
                            if (pidlList is not 0)
                            {
                                Shell32Library.SHOpenFolderAndSelectItems(pidlList, 0, 0, 0);
                                Shell32Library.ILFree(pidlList);
                            }
                        }
                        else
                        {
                            string directoryPath = Path.GetDirectoryName(AudioCoverFilePath);

                            if (Directory.Exists(directoryPath))
                            {
                                Process.Start(directoryPath);
                            }
                            else
                            {
                                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnAudioCoverFilePathClicked), 1, e);
                }
            });
        }

        #endregion 第二部分：音频编辑页面——挂载的事件

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(AudioEditModel audioEdit)
        {
            TimeStartHours = (int)Math.Truncate(audioEdit.StartTime.TotalHours);
            TimeStartMinutes = audioEdit.StartTime.Minutes;
            TimeStartSeconds = audioEdit.StartTime.Seconds;
            TimeStartMillseconds = audioEdit.StartTime.Milliseconds;

            TimeEndHours = (int)Math.Truncate(audioEdit.EndTime.TotalHours);
            TimeEndMinutes = audioEdit.EndTime.Minutes;
            TimeEndSeconds = audioEdit.EndTime.Seconds;
            TimeEndMillseconds = audioEdit.EndTime.Milliseconds;

            if (File.Exists(audioEdit.AudioCoverFilePath))
            {
                AudioCoverFilePath = audioEdit.AudioCoverFilePath;
                SelectedAudioCoverImage = new BitmapImage() { UriSource = new Uri(audioEdit.AudioCoverFilePath) };
            }
        }

        /// <summary>
        /// 获取文件缩略图
        /// </summary>
        private void GetCoverImage(string filePath)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        string cover = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Cover", InfoKind.Text, InfoKind.Name));
                        if (string.Equals(cover, "Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            string coverMine = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Cover_Mime", InfoKind.Text, InfoKind.Name));
                            string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.{1}", Path.GetRandomFileName(), coverMine.Replace("image/", string.Empty)));
                            string arguments = string.Format("-i \"{0}\" -an -vcodec copy \"{1}\"", filePath, tempFilePath);

                            Process process = new()
                            {
                                StartInfo = new()
                                {
                                    FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "FFmpeg.exe"),
                                    Arguments = arguments,
                                    CreateNoWindow = true,
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    UseShellExecute = false
                                }
                            };
                            process.Start();
                            process.WaitForExit();

                            if (process.ExitCode is 0 && File.Exists(tempFilePath))
                            {
                                byte[] tempFileByteArray = File.ReadAllBytes(tempFilePath);
                                File.Delete(tempFilePath);
                                InMemoryRandomAccessStream inMemoryRandomAccessStream = new();
                                await inMemoryRandomAccessStream.WriteAsync(tempFileByteArray.AsBuffer());
                                inMemoryRandomAccessStream.Seek(0);

                                synchronizationContext.Post((_) =>
                                {
                                    if (inMemoryRandomAccessStream is not null)
                                    {
                                        BitmapImage bitmapImage = new();
                                        bitmapImage.SetSource(inMemoryRandomAccessStream);
                                        AudioCoverImage = bitmapImage;
                                        SelectedAudioCoverImage = bitmapImage;
                                        inMemoryRandomAccessStream.Dispose();
                                    }
                                }, null);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioEditPage), nameof(OnMediaOpened), 1, e);
                }
            });
        }

        private Visibility GetAudioEditSelectedItem(SelectorBarItem selectedSelectorBarItem, SelectorBarItem comparedSelectorBarItem)
        {
            return Equals(selectedSelectorBarItem, comparedSelectorBarItem) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetAudioEditKind(AudioEditResultKind selectedAudioKind, AudioEditResultKind comparedAudioKind)
        {
            return Equals(selectedAudioKind, comparedAudioKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetAudioConverFilePathState(string audioCoverFilePath, bool needReverse)
        {
            return needReverse ? string.IsNullOrEmpty(audioCoverFilePath) ? Visibility.Visible : Visibility.Collapsed : string.IsNullOrEmpty(audioCoverFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
