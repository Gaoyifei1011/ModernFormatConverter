using FFmpegInterop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    /// 视频编辑窗口
    /// </summary>
    public sealed partial class VideoEditPage : Page, INotifyPropertyChanged
    {
        private readonly string BlurringString = ResourceService.VideoEditResource.GetString("Blurring");
        private readonly string CloseString = ResourceService.VideoEditResource.GetString("Close");
        private readonly string ImageCroppingString = ResourceService.VideoEditResource.GetString("ImageCropping");
        private readonly string RemoveWatermarkString = ResourceService.VideoEditResource.GetString("RemoveWatermark");
        private readonly string SelectFileString = ResourceService.VideoEditResource.GetString("SelectFile");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private VideoFormatConversionFileModel selectedVideoFormatConversionFile;
        private IRandomAccessStream videoRandomAccessStream;

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

        private VideoEditResultKind _videoEditResultKind;

        public VideoEditResultKind VideoEditResultKind
        {
            get { return _videoEditResultKind; }

            set
            {
                if (!Equals(_videoEditResultKind, value))
                {
                    _videoEditResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoEditResultKind)));
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

        private SelectorBarItem _videoEditSelectedItem;

        public SelectorBarItem VideoEditSelectedItem
        {
            get { return _videoEditSelectedItem; }

            set
            {
                if (!Equals(_videoEditSelectedItem, value))
                {
                    _videoEditSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoEditSelectedItem)));
                }
            }
        }

        private bool _isLoadingTimeStartInformation;

        public bool IsLoadingTimeStartImformation
        {
            get { return _isLoadingTimeStartInformation; }

            set
            {
                if (!Equals(_isLoadingTimeStartInformation, value))
                {
                    _isLoadingTimeStartInformation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingTimeStartImformation)));
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

        private ImageSource _timeStartImage;

        public ImageSource TimeStartImage
        {
            get { return _timeStartImage; }

            set
            {
                if (!Equals(_timeStartImage, value))
                {
                    _timeStartImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeStartImage)));
                }
            }
        }

        private bool _isLoadingTimeEndInformation;

        public bool IsLoadingTimeEndImformation
        {
            get { return _isLoadingTimeEndInformation; }

            set
            {
                if (!Equals(_isLoadingTimeEndInformation, value))
                {
                    _isLoadingTimeEndInformation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingTimeEndImformation)));
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

        private ImageSource _timeEndImage;

        public ImageSource TimeEndImage
        {
            get { return _timeEndImage; }

            set
            {
                if (!Equals(_timeEndImage, value))
                {
                    _timeEndImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEndImage)));
                }
            }
        }

        private ComboBoxItemModel _selectedSelectRegionOperation;

        public ComboBoxItemModel SelectedSelectRegionOperation
        {
            get { return _selectedSelectRegionOperation; }

            set
            {
                if (!Equals(_selectedSelectRegionOperation, value))
                {
                    _selectedSelectRegionOperation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSelectRegionOperation)));
                }
            }
        }

        private int _xCoordinate;

        public int XCoordinate
        {
            get { return _xCoordinate; }

            set
            {
                if (!Equals(_xCoordinate, value))
                {
                    _xCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XCoordinate)));
                }
            }
        }

        private int _yCoordinate;

        public int YCoordinate
        {
            get { return _yCoordinate; }

            set
            {
                if (!Equals(_yCoordinate, value))
                {
                    _yCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(YCoordinate)));
                }
            }
        }

        private int _clipWidth;

        public int ClipWidth
        {
            get { return _clipWidth; }

            set
            {
                if (!Equals(_clipWidth, value))
                {
                    _clipWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipWidth)));
                }
            }
        }

        private int _clipHeight;

        public int ClipHeight
        {
            get { return _clipHeight; }

            set
            {
                if (!Equals(_clipHeight, value))
                {
                    _clipHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipHeight)));
                }
            }
        }

        private bool _isLoadingSelectRegionPreviewImformation;

        public bool IsLoadingSelectRegionPreviewImformation
        {
            get { return _isLoadingSelectRegionPreviewImformation; }

            set
            {
                if (!Equals(_isLoadingSelectRegionPreviewImformation, value))
                {
                    _isLoadingSelectRegionPreviewImformation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingSelectRegionPreviewImformation)));
                }
            }
        }

        private ImageSource _selectRegionPreviewImage;

        public ImageSource SelectRegionPreviewImage
        {
            get { return _selectRegionPreviewImage; }

            set
            {
                if (!Equals(_selectRegionPreviewImage, value))
                {
                    _selectRegionPreviewImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectRegionPreviewImage)));
                }
            }
        }

        private string _videoCoverFilePath = string.Empty;

        public string VideoCoverFilePath
        {
            get { return _videoCoverFilePath; }

            set
            {
                if (!string.Equals(_videoCoverFilePath, value))
                {
                    _videoCoverFilePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoCoverFilePath)));
                }
            }
        }

        private bool _isLoadingVideoCover;

        public bool IsLoadingVideoCover
        {
            get { return _isLoadingVideoCover; }

            set
            {
                if (!Equals(_isLoadingVideoCover, value))
                {
                    _isLoadingVideoCover = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingVideoCover)));
                }
            }
        }

        private ImageSource _videoCoverImage;

        public ImageSource VideoCoverImage
        {
            get { return _videoCoverImage; }

            set
            {
                if (!Equals(_videoCoverImage, value))
                {
                    _videoCoverImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoCoverImage)));
                }
            }
        }

        public List<ComboBoxItemModel> SelectRegionOperationList { get; } = [];

        public WinRTObservableCollection<SelectRegionOperationModel> SelectRegionOperationCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoEditPage()
        {
            InitializeData();
            InitializeComponent();
        }

        #region 第一部分：重载分类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (args.Parameter is VideoFormatConversionFileModel videoFormatConversionFile)
            {
                selectedVideoFormatConversionFile = videoFormatConversionFile;
                FileName = Path.GetFileName(selectedVideoFormatConversionFile.FilePath);
                UpdateData(selectedVideoFormatConversionFile.VideoEdit);
                VideoEditSelectedItem = VideoEditSelectorBar.Items[0];

                if (VideoEditResultKind is VideoEditResultKind.None)
                {
                    VideoEditResultKind = VideoEditResultKind.Loading;
                    VideoEditMediaPlayerElement.MediaPlayer.MediaOpened += OnMediaOpened;
                    VideoEditMediaPlayerElement.MediaPlayer.MediaFailed += OnMediaFailed;

                    if (TimeStartHours is not 0 || TimeStartMinutes is not 0 || TimeStartSeconds is not 0 || TimeStartMillseconds is not 0)
                    {
                        IsLoadingTimeStartImformation = true;
                    }

                    if (TimeEndHours is not 0 || TimeEndMinutes is not 0 || TimeEndSeconds is not 0 || TimeEndMillseconds is not 0)
                    {
                        IsLoadingTimeEndImformation = true;
                    }

                    (bool isLoadedSccessfully, MediaSource mediaSource, Exception exception) = await Task.Run(async () =>
                    {
                        try
                        {
                            IActivationFactory activationFactory = WindowsRuntimeMarshal.GetActivationFactory(typeof(MediaStreamSource));
                            MediaStreamSource mediaStreamSource = activationFactory.ActivateInstance() as MediaStreamSource;
                            videoRandomAccessStream = await (await StorageFile.GetFileFromPathAsync(selectedVideoFormatConversionFile.FilePath)).OpenAsync(FileAccessMode.Read);
                            FFmpegInteropMSSConfig ffmpegInteropMSSConfig = new()
                            {
                                ForceVideoDecode = true,
                                ForceAudioDecode = true
                            };
                            FFmpegInteropMSS.InitializeFromStream(videoRandomAccessStream, mediaStreamSource, ffmpegInteropMSSConfig);
                            MediaSource meidaSource = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
                            return ValueTuple.Create<bool, MediaSource, Exception>(true, meidaSource, null);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnNavigatedTo), 1, e);
                            return ValueTuple.Create<bool, MediaSource, Exception>(false, null, e);
                        }
                    });

                    if (isLoadedSccessfully)
                    {
                        MediaSource = mediaSource;
                    }
                    else
                    {
                        VideoEditResultKind = VideoEditResultKind.Failed;
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
                VideoEditResultKind = VideoEditResultKind.None;
                MediaSource = null;
                videoRandomAccessStream.Dispose();
                VideoEditMediaPlayerElement.MediaPlayer.MediaOpened -= OnMediaOpened;
                VideoEditMediaPlayerElement.MediaPlayer.MediaFailed -= OnMediaFailed;
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnNavigatingFrom), 2, e);
            }
        }

        #endregion 第一部分：重载分类事件

        #region 第二部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 删除选中区域列表项
        /// </summary>
        private void OnDeleteExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is SelectRegionOperationModel selectRegionOperation)
            {
                SelectRegionOperationCollection.Remove(selectRegionOperation);
            }
        }

        #endregion 第二部分：ExecuteCommand 命令调用时挂载的事件

        #region 第三部分：视频编辑窗口——挂载的事件

        /// <summary>
        /// 视频加载失败后触发的事件
        /// </summary>
        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                VideoEditResultKind = VideoEditResultKind.Failed;
                ErrorReason = args.ExtendedErrorCode is not null ? string.Format("0x{0:X8},{1}", args.ExtendedErrorCode.HResult, args.ExtendedErrorCode.Message) : "N/A";
                IsLoadingTimeStartImformation = false;
                IsLoadingTimeEndImformation = false;
            }, null);
            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnMediaFailed), 1, args.ExtendedErrorCode is not null ? args.ExtendedErrorCode : new Exception());
        }

        /// <summary>
        /// 视频加载成功后触发的事件
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

            if (IsLoadingTimeStartImformation)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                        string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds, selectedVideoFormatConversionFile.FilePath, tempFilePath);

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
                                    TimeStartImage = bitmapImage;
                                    inMemoryRandomAccessStream.Dispose();
                                }
                                else
                                {
                                    TimeStartImage = null;
                                }
                                IsLoadingTimeStartImformation = false;
                            }, null);
                        }
                        else
                        {
                            synchronizationContext.Post((_) =>
                            {
                                IsLoadingTimeStartImformation = false;
                            }, null);
                        }
                    }
                    catch (Exception e)
                    {
                        synchronizationContext.Post((_) =>
                        {
                            IsLoadingTimeStartImformation = false;
                        }, null);
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnMediaOpened), 1, e);
                    }
                });
            }

            if (IsLoadingTimeEndImformation)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                        string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds, selectedVideoFormatConversionFile.FilePath, tempFilePath);

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
                                    TimeEndImage = bitmapImage;
                                    inMemoryRandomAccessStream.Dispose();
                                }
                                else
                                {
                                    TimeEndImage = null;
                                }
                                IsLoadingTimeEndImformation = false;
                            }, null);
                        }
                        else
                        {
                            synchronizationContext.Post((_) =>
                            {
                                IsLoadingTimeEndImformation = false;
                            }, null);
                        }
                    }
                    catch (Exception e)
                    {
                        synchronizationContext.Post((_) =>
                        {
                            IsLoadingTimeEndImformation = false;
                        }, null);
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnMediaOpened), 2, e);
                    }
                });
            }

            synchronizationContext.Post((_) =>
            {
                VideoEditResultKind = VideoEditResultKind.Successfully;
                ErrorReason = string.Empty;
            }, null);
        }

        /// <summary>
        /// 使用系统播放器播放
        /// </summary>
        private void OnPlayWithSystemVideoClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(selectedVideoFormatConversionFile.FilePath);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnPlayWithSystemVideoClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 剪辑视频选中项发生变化时触发的事件
        /// </summary>
        private void OnVideoEditSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (!Equals(sender.SelectedItem, VideoEditSelectedItem))
            {
                VideoEditSelectedItem = sender.SelectedItem;
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 更新数据
            selectedVideoFormatConversionFile.VideoEdit.SelectRegionOperation = Convert.ToString(SelectedSelectRegionOperation.SelectedValue);
            selectedVideoFormatConversionFile.VideoEdit.StartTime = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
            selectedVideoFormatConversionFile.VideoEdit.EndTime = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
            selectedVideoFormatConversionFile.VideoEdit.SelectRegionOperationList.Clear();
            selectedVideoFormatConversionFile.VideoEdit.SelectRegionOperationList.AddRange(SelectRegionOperationCollection);
            selectedVideoFormatConversionFile.VideoEdit.VideoCoverFilePath = VideoCoverFilePath;
            selectedVideoFormatConversionFile = null;

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage)
            {
                videoConversionPage.NavigateTo(videoConversionPage.PageList[0], null, false);
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeStartHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeStartMinutes = currentPosition.Minutes;
            TimeStartSeconds = currentPosition.Seconds;
            TimeStartMillseconds = currentPosition.Milliseconds;

            IsLoadingTimeStartImformation = true;
            InMemoryRandomAccessStream inMemoryRandomAccessStream = await Task.Run(async () =>
            {
                try
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                    string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds, selectedVideoFormatConversionFile.FilePath, tempFilePath);

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
                        return inMemoryRandomAccessStream;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnTimeStartLocationClicked), 1, e);
                    return null;
                }
            });

            if (inMemoryRandomAccessStream is not null)
            {
                BitmapImage bitmapImage = new();
                bitmapImage.SetSource(inMemoryRandomAccessStream);
                TimeStartImage = bitmapImage;
                inMemoryRandomAccessStream.Dispose();
            }
            else
            {
                TimeStartImage = null;
            }
            IsLoadingTimeStartImformation = false;
        }

        /// <summary>
        /// 从起始位置时间播放视频
        /// </summary>
        private void OnTimeStartLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        TimeSpan totalDuration = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeEndHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeEndMinutes = currentPosition.Minutes;
            TimeEndSeconds = currentPosition.Seconds;
            TimeEndMillseconds = currentPosition.Milliseconds;

            IsLoadingTimeEndImformation = true;
            InMemoryRandomAccessStream inMemoryRandomAccessStream = await Task.Run(async () =>
            {
                try
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                    string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds, selectedVideoFormatConversionFile.FilePath, tempFilePath);

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
                        return inMemoryRandomAccessStream;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnTimeEndLocationClicked), 1, e);
                    return null;
                }
            });

            if (inMemoryRandomAccessStream is not null)
            {
                BitmapImage bitmapImage = new();
                bitmapImage.SetSource(inMemoryRandomAccessStream);
                TimeEndImage = bitmapImage;
                inMemoryRandomAccessStream.Dispose();
            }
            else
            {
                TimeEndImage = null;
            }
            IsLoadingTimeEndImformation = false;
        }

        /// <summary>
        /// 从结束位置时间播放视频
        /// </summary>
        private void OnTimeEndLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
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
                VideoCoverFilePath = openFileDialog.FileName;
                VideoCoverImage = new BitmapImage() { UriSource = new Uri(VideoCoverFilePath) };
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 清空选中的封面文件
        /// </summary>
        private void OnClearSelectFileClicked(object sender, RoutedEventArgs args)
        {
            VideoCoverFilePath = string.Empty;
            VideoCoverImage = null;
        }

        /// <summary>
        /// 打开封面文件所在的目录
        /// </summary>
        private void OnVideoCoverFilePathClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(VideoCoverFilePath))
                    {
                        if (File.Exists(VideoCoverFilePath))
                        {
                            nint pidlList = Shell32Library.ILCreateFromPath(VideoCoverFilePath);
                            if (pidlList is not 0)
                            {
                                Shell32Library.SHOpenFolderAndSelectItems(pidlList, 0, 0, 0);
                                Shell32Library.ILFree(pidlList);
                            }
                        }
                        else
                        {
                            string directoryPath = Path.GetDirectoryName(VideoCoverFilePath);

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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnVideoCoverFilePathClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 选中区域操作选中项发生变化时触发的事件
        /// </summary>
        private void OnSelectRegionOperationSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel selectRegionOperation && !Equals(SelectedSelectRegionOperation, selectRegionOperation))
            {
                SelectedSelectRegionOperation = selectRegionOperation;
            }
        }

        /// <summary>
        /// 截取预览
        /// </summary>
        private async void OnClipPreviewClicked(object sender, RoutedEventArgs args)
        {
            if (XCoordinate is 0 && YCoordinate is 0 && ClipWidth is 0 && ClipHeight is 0 && ClipWidth * ClipHeight is 0)
            {
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CutVideoClipNoDefaultValue));
                return;
            }

            if (VideoEditResultKind is VideoEditResultKind.Successfully)
            {
                IsLoadingSelectRegionPreviewImformation = true;
                TimeSpan currentPosition = VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
                InMemoryRandomAccessStream inMemoryRandomAccessStream = await Task.Run(async () =>
                {
                    try
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                        string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vf \"crop={5}:{6}:{7}:{8}\" -vframes 1 -q:v 2 \"{9}\"",
                            Math.Truncate(currentPosition.TotalHours),
                            currentPosition.Minutes,
                            currentPosition.Seconds,
                            currentPosition.Milliseconds,
                            selectedVideoFormatConversionFile.FilePath,
                            ClipWidth,
                            ClipHeight,
                            XCoordinate,
                            YCoordinate,
                            tempFilePath
                            );

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
                            return inMemoryRandomAccessStream;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoEditPage), nameof(OnClipPreviewClicked), 1, e);
                        return null;
                    }
                });

                if (inMemoryRandomAccessStream is not null)
                {
                    BitmapImage bitmapImage = new();
                    bitmapImage.SetSource(inMemoryRandomAccessStream);
                    SelectRegionPreviewImage = bitmapImage;
                    inMemoryRandomAccessStream.Dispose();
                }
                else
                {
                    SelectRegionPreviewImage = null;
                }
                IsLoadingSelectRegionPreviewImformation = false;
            }
        }

        /// <summary>
        /// 添加截取信息
        /// </summary>
        private async void OnAddClipClicked(object sender, RoutedEventArgs args)
        {
            if (XCoordinate is 0 && YCoordinate is 0 && ClipWidth is 0 && ClipHeight is 0 && ClipWidth * ClipHeight is 0)
            {
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CutVideoClipNoDefaultValue));
                return;
            }

            SelectRegionOperationCollection.Add(new()
            {
                XCoordinate = XCoordinate,
                YCoordinate = YCoordinate,
                ClipWidth = ClipWidth,
                ClipHeight = ClipHeight
            });
        }

        /// <summary>
        /// 清空选择区域操作列表
        /// </summary>
        private void OnClearClipListClicked(object sender, RoutedEventArgs args)
        {
            SelectRegionOperationList.Clear();
        }

        /// <summary>
        /// 横坐标发生变化时触发的事件
        /// </summary>
        private void OnXCoordinateValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                XCoordinate = int.MaxValue;
                XCoordinate = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    XCoordinate = 0;
                }
                else
                {
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        int videoWidth = Convert.ToInt32(VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoWidth);

                        // 防止时间溢出
                        if (newValue + ClipWidth > videoWidth)
                        {
                            XCoordinate = videoWidth - ClipWidth;
                        }
                        else
                        {
                            XCoordinate = newValue;
                        }
                    }
                    else
                    {
                        XCoordinate = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 纵坐标发生变化时触发的事件
        /// </summary>
        private void OnYCoordinateValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                YCoordinate = int.MaxValue;
                YCoordinate = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    YCoordinate = 0;
                }
                else
                {
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        int videoHeight = Convert.ToInt32(VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoHeight);

                        // 防止时间溢出
                        if (newValue + ClipHeight > videoHeight)
                        {
                            YCoordinate = videoHeight - ClipHeight;
                        }
                        else
                        {
                            YCoordinate = newValue;
                        }
                    }
                    else
                    {
                        YCoordinate = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 截取长度发生变化时触发的事件
        /// </summary>
        private void OnClipWidthValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ClipWidth = int.MaxValue;
                ClipWidth = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    ClipWidth = 0;
                }
                else
                {
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        int videoWidth = Convert.ToInt32(VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoWidth);

                        // 防止时间溢出
                        if (newValue + XCoordinate > videoWidth)
                        {
                            ClipWidth = videoWidth - XCoordinate;
                        }
                        else
                        {
                            ClipWidth = newValue;
                        }
                    }
                    else
                    {
                        ClipWidth = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 截取宽度发生变化时触发的事件
        /// </summary>
        private void OnClipHeightValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ClipHeight = int.MaxValue;
                ClipHeight = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    ClipHeight = 0;
                }
                else
                {
                    if (VideoEditResultKind is VideoEditResultKind.Successfully)
                    {
                        int videoHeight = Convert.ToInt32(VideoEditMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoHeight);

                        // 防止时间溢出
                        if (newValue + YCoordinate > videoHeight)
                        {
                            ClipHeight = videoHeight - YCoordinate;
                        }
                        else
                        {
                            ClipHeight = newValue;
                        }
                    }
                    else
                    {
                        ClipHeight = newValue;
                    }
                }
            }
        }

        #endregion 第三部分：视频编辑窗口——挂载的事件

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            SelectRegionOperationList.Add(new ComboBoxItemModel()
            {
                DisplayMember = CloseString,
                SelectedValue = "Close",
            });
            SelectRegionOperationList.Add(new ComboBoxItemModel()
            {
                DisplayMember = ImageCroppingString,
                SelectedValue = "ImageCropping",
            });
            SelectRegionOperationList.Add(new ComboBoxItemModel()
            {
                DisplayMember = RemoveWatermarkString,
                SelectedValue = "RemoveWatermark",
            });
            SelectRegionOperationList.Add(new ComboBoxItemModel()
            {
                DisplayMember = BlurringString,
                SelectedValue = "Blurring",
            });
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(VideoEditModel videoEdit)
        {
            SelectedSelectRegionOperation = videoEdit is not null && SelectRegionOperationList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoEdit.SelectRegionOperation)) is ComboBoxItemModel selectRegionOperation ? selectRegionOperation : SelectRegionOperationList[0];

            TimeStartHours = (int)Math.Truncate(videoEdit.StartTime.TotalHours);
            TimeStartMinutes = videoEdit.StartTime.Minutes;
            TimeStartSeconds = videoEdit.StartTime.Seconds;
            TimeStartMillseconds = videoEdit.StartTime.Milliseconds;

            TimeEndHours = (int)Math.Truncate(videoEdit.EndTime.TotalHours);
            TimeEndMinutes = videoEdit.EndTime.Minutes;
            TimeEndSeconds = videoEdit.EndTime.Seconds;
            TimeEndMillseconds = videoEdit.EndTime.Milliseconds;

            foreach (SelectRegionOperationModel selectRegionOperationItem in videoEdit.SelectRegionOperationList)
            {
                SelectRegionOperationCollection.Add(selectRegionOperationItem);
            }

            if (File.Exists(videoEdit.VideoCoverFilePath))
            {
                VideoCoverFilePath = videoEdit.VideoCoverFilePath;
                VideoCoverImage = new BitmapImage() { UriSource = new Uri(videoEdit.VideoCoverFilePath) };
            }
        }

        private Visibility GetVideoEditSelectedItem(SelectorBarItem selectedSelectorBarItem, SelectorBarItem comparedSelectorBarItem)
        {
            return Equals(selectedSelectorBarItem, comparedSelectorBarItem) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetVideoEditKind(VideoEditResultKind selectedVideoKind, VideoEditResultKind comparedVideoKind)
        {
            return Equals(selectedVideoKind, comparedVideoKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetVideoConverFilePathState(string videoCoverFilePath, bool needReverse)
        {
            return needReverse ? string.IsNullOrEmpty(videoCoverFilePath) ? Visibility.Visible : Visibility.Collapsed : string.IsNullOrEmpty(videoCoverFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
