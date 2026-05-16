using FFmpegInterop;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.Backdrop;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 剪辑视频窗口
    /// </summary>
    public sealed partial class CutVideoWindow : Window, INotifyPropertyChanged
    {
        private readonly string BlurringString = ResourceService.CutVideoResource.GetString("Blurring");
        private readonly string CloseString = ResourceService.CutVideoResource.GetString("Close");
        private readonly string ImageCroppingString = ResourceService.CutVideoResource.GetString("ImageCropping");
        private readonly string RemoveWatermarkString = ResourceService.CutVideoResource.GetString("RemoveWatermark");
        private readonly string SelectFileString = ResourceService.CutVideoResource.GetString("SelectFile");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly string filePath;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC cutVideoWindowSubClassProc;
        private ContentIsland contentIsland;
        private InputKeyboardSource inputKeyboardSource;
        private InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;
        private IRandomAccessStream videoRandomAccessStream;

        public new static CutVideoWindow Current { get; private set; }

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

        private CutVideoResultKind _cutVideoResultKind;

        public CutVideoResultKind CutVideoResultKind
        {
            get { return _cutVideoResultKind; }

            set
            {
                if (!Equals(_cutVideoResultKind, value))
                {
                    _cutVideoResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CutVideoResultKind)));
                }
            }
        }

        private SelectorBarItem _cutVideoSelectedItem;

        public SelectorBarItem CutVideoSelectedItem
        {
            get { return _cutVideoSelectedItem; }

            set
            {
                if (!Equals(_cutVideoSelectedItem, value))
                {
                    _cutVideoSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CutVideoSelectedItem)));
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

        public CutVideoWindow(ConversionToolsWindow conversionToolsWindow, CutVideoModel cutVideo, string selectedFilePath)
        {
            filePath = selectedFilePath;
            InitializeData(cutVideo);
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

        #region 第四部分：ExecuteCommand 命令调用时挂载的事件

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

            if (CutVideoResultKind is CutVideoResultKind.None)
            {
                CutVideoResultKind = CutVideoResultKind.Loading;
                CutVideoMediaPlayerElement.MediaPlayer.MediaOpened += OnMediaOpened;
                CutVideoMediaPlayerElement.MediaPlayer.MediaFailed += OnMediaFailed;

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
                        videoRandomAccessStream = await (await StorageFile.GetFileFromPathAsync(filePath)).OpenAsync(FileAccessMode.Read);
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnLoaded), 1, e);
                        return ValueTuple.Create<bool, MediaSource, Exception>(false, null, e);
                    }
                });

                if (isLoadedSccessfully)
                {
                    MediaSource = mediaSource;
                }
                else
                {
                    CutVideoResultKind = CutVideoResultKind.Failed;
                    MediaSource = null;
                    ErrorReason = exception is not null ? string.Format("0x{0:X8},{1}", exception.HResult, exception.Message) : "N/A";
                }
            }
        }

        /// <summary>
        /// 视频加载失败后触发的事件
        /// </summary>
        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                CutVideoResultKind = CutVideoResultKind.Failed;
                ErrorReason = args.ExtendedErrorCode is not null ? string.Format("0x{0:X8},{1}", args.ExtendedErrorCode.HResult, args.ExtendedErrorCode.Message) : "N/A";
                IsLoadingTimeStartImformation = false;
                IsLoadingTimeEndImformation = false;
            }, null);
            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnMediaFailed), 1, args.ExtendedErrorCode is not null ? args.ExtendedErrorCode : new Exception());
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
                        string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds, filePath, tempFilePath);

                        Process process = new()
                        {
                            StartInfo = new()
                            {
                                FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "ffmpeg.exe"),
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnMediaOpened), 1, e);
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
                        string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds, filePath, tempFilePath);

                        Process process = new()
                        {
                            StartInfo = new()
                            {
                                FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "ffmpeg.exe"),
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnMediaOpened), 2, e);
                    }
                });
            }

            synchronizationContext.Post((_) =>
            {
                CutVideoResultKind = CutVideoResultKind.Successfully;
                ErrorReason = string.Empty;
            }, null);
        }

        /// <summary>
        /// 使用系统播放器播放
        /// </summary>
        private void OnPlayWithSystemVideoClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(filePath);
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnPlayWithSystemVideoClicked), 1, e);
            }
        }

        /// <summary>
        /// 剪辑视频选中项发生变化时触发的事件
        /// </summary>
        private void OnCutVideoSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (!Equals(sender.SelectedItem, CutVideoSelectedItem))
            {
                CutVideoSelectedItem = sender.SelectedItem;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
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
                    string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds, filePath, tempFilePath);

                    Process process = new()
                    {
                        StartInfo = new()
                        {
                            FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "ffmpeg.exe"),
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnTimeStartLocationClicked), 1, e);
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
            CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
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
                    string arguments = string.Format("-ss {0}:{1}:{2}.{3} -i \"{4}\" -vframes 1 -q:v 2 \"{5}\"", TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds, filePath, tempFilePath);

                    Process process = new()
                    {
                        StartInfo = new()
                        {
                            FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "ffmpeg.exe"),
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnTimeEndLocationClicked), 1, e);
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
            CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnVideoCoverFilePathClicked), 1, e);
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
                await ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CutVideoClipNoDefaultValue));
                return;
            }

            if (CutVideoResultKind is CutVideoResultKind.Successfully)
            {
                IsLoadingSelectRegionPreviewImformation = true;
                TimeSpan currentPosition = CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
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
                            filePath,
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
                                FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "ffmpeg.exe"),
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(OnClipPreviewClicked), 1, e);
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
                await ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CutVideoClipNoDefaultValue));
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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        int videoWidth = Convert.ToInt32(CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoWidth);

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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        int videoHeight = Convert.ToInt32(CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoHeight);

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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        int videoWidth = Convert.ToInt32(CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoWidth);

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
                    if (CutVideoResultKind is CutVideoResultKind.Successfully)
                    {
                        int videoHeight = Convert.ToInt32(CutVideoMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalVideoHeight);

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
                VisualStateManager.GoToState(CutVideoPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(CutVideoPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(CutVideoPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(CutVideoPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(CutVideoPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(CutVideoPage, "BackgroundDefault", false);
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
        /// 许可证文字内容窗口消息处理
        /// </summary>
        private nint CutVideoWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }
                        break;
                    }
                // 窗口激活状态发生变化时触发的消息
                case WindowMessage.WM_ACTIVATEAPP:
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
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(CutVideoWindowSubClassProc), 1, e);
                        }
                        break;
                    }
                // 窗口销毁后触发的消息
                case WindowMessage.WM_DESTROY:
                    {
                        try
                        {
                            Current = null;
                            MediaSource = null;
                            videoRandomAccessStream.Dispose();
                            ThemeService.PropertyChanged -= OnServicePropertyChanged;
                            BackdropService.PropertyChanged -= OnServicePropertyChanged;
                            inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                            inputPointerSource.PointerReleased -= OnPointerReleased;
                            CutVideoMediaPlayerElement.MediaPlayer.MediaOpened -= OnMediaOpened;
                            CutVideoMediaPlayerElement.MediaPlayer.MediaFailed -= OnMediaFailed;
                            Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, cutVideoWindowSubClassProc, 0);
                            // TODO：未完成
                            if (!taskCompletionSource.Task.IsCompleted)
                            {
                                taskCompletionSource.TrySetResult(ContentDialogResult.None);
                            }
                            ConversionToolsWindow.Activate();
                            ConversionToolsWindow = null;
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(CutVideoWindowSubClassProc), 2, e);
                        }

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
                        overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(1000 * Convert.ToDouble(wParam) / 96);
                        overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(600 * Convert.ToDouble(wParam) / 96);
                        break;
                    }
                // 选择窗口右键菜单的条目时接收到的消息
                case WindowMessage.WM_SYSCOMMAND:
                    {
                        SYSTEMCOMMAND sysCommand = (SYSTEMCOMMAND)(wParam & 0xFFF0);

                        if (sysCommand is SYSTEMCOMMAND.SC_MOUSEMENU)
                        {
                            return 0;
                        }
                        else if (sysCommand is SYSTEMCOMMAND.SC_KEYMENU)
                        {
                            if (lParam is (int)System.Windows.Forms.Keys.Space)
                            {
                                return 0;
                            }
                        }
                        break;
                    }
            }
            return Comctl32Library.DefSubclassProc(hWnd, Msg, wParam, lParam);
        }

        #endregion 第八部分：窗口过程

        #region 第九部分：显示应用通知

        /// <summary>
        /// 使用教学提示显示应用内通知
        /// </summary>
        public async Task ShowNotificationAsync(TeachingTip teachingTip, int duration = 2000)
        {
            if (teachingTip is not null && Content is Page page && page.Content is Grid grid)
            {
                try
                {
                    grid.Children.Add(teachingTip);

                    teachingTip.IsOpen = true;
                    await Task.Delay(duration);
                    teachingTip.IsOpen = false;

                    // 应用内通知关闭动画显示耗费 300 ms
                    await Task.Delay(300);
                    grid.Children.Remove(teachingTip);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutVideoWindow), nameof(ShowNotificationAsync), 1, e);
                }
            }
        }

        #endregion 第九部分：显示应用通知

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(CutVideoModel cutVideo)
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
            SelectedSelectRegionOperation = cutVideo is not null && SelectRegionOperationList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), cutVideo.SelectRegionOperation)) is ComboBoxItemModel selectRegionOperation ? selectRegionOperation : SelectRegionOperationList[0];

            TimeStartHours = (int)Math.Truncate(cutVideo.StartTime.TotalHours);
            TimeStartMinutes = cutVideo.StartTime.Minutes;
            TimeStartSeconds = cutVideo.StartTime.Seconds;
            TimeStartMillseconds = cutVideo.StartTime.Milliseconds;

            TimeEndHours = (int)Math.Truncate(cutVideo.EndTime.TotalHours);
            TimeEndMinutes = cutVideo.EndTime.Minutes;
            TimeEndSeconds = cutVideo.EndTime.Seconds;
            TimeEndMillseconds = cutVideo.EndTime.Milliseconds;

            foreach (SelectRegionOperationModel selectRegionOperationItem in cutVideo.SelectRegionOperationList)
            {
                SelectRegionOperationCollection.Add(selectRegionOperationItem);
            }

            if (File.Exists(cutVideo.VideoCoverFilePath))
            {
                VideoCoverFilePath = cutVideo.VideoCoverFilePath;
                VideoCoverImage = new BitmapImage() { UriSource = new Uri(cutVideo.VideoCoverFilePath) };
            }
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI(ConversionToolsWindow conversionToolsWindow)
        {
            // 窗口部分初始化
            Current = this;
            ConversionToolsWindow = conversionToolsWindow;

            if (IntPtr.Size is 8)
            {
                User32Library.SetWindowLongPtr((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, conversionToolsWindow.AppWindow.Id.Value);
            }
            else
            {
                User32Library.SetWindowLong((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, conversionToolsWindow.AppWindow.Id.Value);
            }
            overlappedPresenter = OverlappedPresenter.CreateForDialog();
            ExtendsContentIntoTitleBar = true;
            overlappedPresenter.IsResizable = true;
            overlappedPresenter.IsMinimizable = false;
            overlappedPresenter.IsMaximizable = false;
            overlappedPresenter.IsModal = true;
            AppWindow.SetPresenter(overlappedPresenter);
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            int dpi = User32Library.GetDpiForWindow((nint)AppWindow.Id.Value);
            overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(1000 * Convert.ToDouble(dpi) / 96);
            overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(600 * Convert.ToDouble(dpi) / 96);
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);

            // 挂载相应的事件
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            cutVideoWindowSubClassProc = new SUBCLASSPROC(CutVideoWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, cutVideoWindowSubClassProc, 0, 0);

            SetWindowTheme();
            SetSystemBackdrop();

            CutVideoSelectedItem = CutVideoSelectorBar.Items[0];
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

        private Visibility GetCutVideoSelectedItem(SelectorBarItem selectedSelectorBarItem, SelectorBarItem comparedSelectorBarItem)
        {
            return Equals(selectedSelectorBarItem, comparedSelectorBarItem) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetCutVideoKind(CutVideoResultKind selectedVideoKind, CutVideoResultKind comparedVideoKind)
        {
            return Equals(selectedVideoKind, comparedVideoKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetVideoConverFilePathState(string videoCoverFilePath, bool needReverse)
        {
            return needReverse ? string.IsNullOrEmpty(videoCoverFilePath) ? Visibility.Visible : Visibility.Collapsed : string.IsNullOrEmpty(videoCoverFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
