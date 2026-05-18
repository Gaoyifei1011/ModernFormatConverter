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
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
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
    /// 剪辑音频窗口
    /// </summary>
    public sealed partial class CutAudioWindow : Window, INotifyPropertyChanged
    {
        private readonly string SelectFileString = ResourceService.CutAudioResource.GetString("SelectFile");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly string filePath;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC cutAudioWindowSubClassProc;
        private ContentIsland contentIsland;
        private InputKeyboardSource inputKeyboardSource;
        private InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;
        private IRandomAccessStream audioRandomAccessStream;

        public new static CutAudioWindow Current { get; private set; }

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

        private CutAudioResultKind _cutAudioResultKind;

        public CutAudioResultKind CutAudioResultKind
        {
            get { return _cutAudioResultKind; }

            set
            {
                if (!Equals(_cutAudioResultKind, value))
                {
                    _cutAudioResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CutAudioResultKind)));
                }
            }
        }

        private SelectorBarItem _cutAudioSelectedItem;

        public SelectorBarItem CutAudioSelectedItem
        {
            get { return _cutAudioSelectedItem; }

            set
            {
                if (!Equals(_cutAudioSelectedItem, value))
                {
                    _cutAudioSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CutAudioSelectedItem)));
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

        public event PropertyChangedEventHandler PropertyChanged;

        public CutAudioWindow(ConversionToolsWindow conversionToolsWindow, CutAudioModel cutAudio, string selectedFilePath)
        {
            filePath = selectedFilePath;
            InitializeData(cutAudio);
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

            if (CutAudioResultKind is CutAudioResultKind.None)
            {
                CutAudioResultKind = CutAudioResultKind.Loading;
                CutAudioMediaPlayerElement.MediaPlayer.MediaOpened += OnMediaOpened;
                CutAudioMediaPlayerElement.MediaPlayer.MediaFailed += OnMediaFailed;

                (bool isLoadedSccessfully, MediaSource mediaSource, Exception exception) = await Task.Run(async () =>
                {
                    try
                    {
                        IActivationFactory activationFactory = WindowsRuntimeMarshal.GetActivationFactory(typeof(MediaStreamSource));
                        MediaStreamSource mediaStreamSource = activationFactory.ActivateInstance() as MediaStreamSource;
                        audioRandomAccessStream = await (await StorageFile.GetFileFromPathAsync(filePath)).OpenAsync(FileAccessMode.Read);
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(OnLoaded), 1, e);
                        return ValueTuple.Create<bool, MediaSource, Exception>(false, null, e);
                    }
                });

                if (isLoadedSccessfully)
                {
                    MediaSource = mediaSource;
                }
                else
                {
                    CutAudioResultKind = CutAudioResultKind.Failed;
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
                CutAudioResultKind = CutAudioResultKind.Failed;
                ErrorReason = args.ExtendedErrorCode is not null ? string.Format("0x{0:X8},{1}", args.ExtendedErrorCode.HResult, args.ExtendedErrorCode.Message) : "N/A";
            }, null);
            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(OnMediaFailed), 1, args.ExtendedErrorCode is not null ? args.ExtendedErrorCode : new Exception());
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

            synchronizationContext.Post((_) =>
            {
                CutAudioResultKind = CutAudioResultKind.Successfully;
                ErrorReason = string.Empty;
            }, null);
        }

        /// <summary>
        /// 使用系统播放器播放
        /// </summary>
        private void OnPlayWithSystemAudioClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(filePath);
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(OnPlayWithSystemAudioClicked), 1, e);
            }
        }

        /// <summary>
        /// 剪辑视频选中项发生变化时触发的事件
        /// </summary>
        private void OnCutAudioSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (!Equals(sender.SelectedItem, CutAudioSelectedItem))
            {
                CutAudioSelectedItem = sender.SelectedItem;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeStartHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeStartMinutes = currentPosition.Minutes;
            TimeStartSeconds = currentPosition.Seconds;
            TimeStartMillseconds = currentPosition.Milliseconds;
        }

        /// <summary>
        /// 从起始位置时间播放视频
        /// </summary>
        private void OnTimeStartLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeStartHours, TimeStartMinutes, TimeStartSeconds, TimeStartMillseconds);
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
                    if (CutAudioResultKind is CutAudioResultKind.Successfully)
                    {
                        TimeSpan totalDuration = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDuration;
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
            TimeSpan currentPosition = CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.Position;
            TimeEndHours = (int)Math.Truncate(currentPosition.TotalHours);
            TimeEndMinutes = currentPosition.Minutes;
            TimeEndSeconds = currentPosition.Seconds;
            TimeEndMillseconds = currentPosition.Milliseconds;
        }

        /// <summary>
        /// 从结束位置时间播放视频
        /// </summary>
        private void OnTimeEndLocateVdieoClicked(object sender, RoutedEventArgs args)
        {
            CutAudioMediaPlayerElement.MediaPlayer.PlaybackSession.Position = new(0, TimeEndHours, TimeEndMinutes, TimeEndSeconds, TimeEndMillseconds);
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
                AudioCoverImage = new BitmapImage() { UriSource = new Uri(AudioCoverFilePath) };
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 清空选中的封面文件
        /// </summary>
        private void OnClearSelectFileClicked(object sender, RoutedEventArgs args)
        {
            AudioCoverFilePath = string.Empty;
            AudioCoverImage = null;
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(OnAudioCoverFilePathClicked), 1, e);
                }
            });
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
                VisualStateManager.GoToState(CutAudioPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(CutAudioPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(CutAudioPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(CutAudioPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(CutAudioPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(CutAudioPage, "BackgroundDefault", false);
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
        private nint CutAudioWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
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
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(CutAudioWindowSubClassProc), 1, e);
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
                            audioRandomAccessStream.Dispose();
                            ThemeService.PropertyChanged -= OnServicePropertyChanged;
                            BackdropService.PropertyChanged -= OnServicePropertyChanged;
                            inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                            inputPointerSource.PointerReleased -= OnPointerReleased;
                            CutAudioMediaPlayerElement.MediaPlayer.MediaOpened -= OnMediaOpened;
                            CutAudioMediaPlayerElement.MediaPlayer.MediaFailed -= OnMediaFailed;
                            Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, cutAudioWindowSubClassProc, 0);
                            if (!taskCompletionSource.Task.IsCompleted)
                            {
                                taskCompletionSource.TrySetResult(ContentDialogResult.None);
                            }
                            ConversionToolsWindow.Activate();
                            ConversionToolsWindow = null;
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(CutAudioWindowSubClassProc), 2, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CutAudioWindow), nameof(ShowNotificationAsync), 1, e);
                }
            }
        }

        #endregion 第九部分：显示应用通知

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(CutAudioModel cutAudio)
        {
            TimeStartHours = (int)Math.Truncate(cutAudio.StartTime.TotalHours);
            TimeStartMinutes = cutAudio.StartTime.Minutes;
            TimeStartSeconds = cutAudio.StartTime.Seconds;
            TimeStartMillseconds = cutAudio.StartTime.Milliseconds;

            TimeEndHours = (int)Math.Truncate(cutAudio.EndTime.TotalHours);
            TimeEndMinutes = cutAudio.EndTime.Minutes;
            TimeEndSeconds = cutAudio.EndTime.Seconds;
            TimeEndMillseconds = cutAudio.EndTime.Milliseconds;

            if (File.Exists(cutAudio.AudioCoverFilePath))
            {
                AudioCoverFilePath = cutAudio.AudioCoverFilePath;
                AudioCoverImage = new BitmapImage() { UriSource = new Uri(cutAudio.AudioCoverFilePath) };
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
            cutAudioWindowSubClassProc = new SUBCLASSPROC(CutAudioWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, cutAudioWindowSubClassProc, 0, 0);

            SetWindowTheme();
            SetSystemBackdrop();

            CutAudioSelectedItem = CutAudioSelectorBar.Items[0];
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

        private Visibility GetCutAudioSelectedItem(SelectorBarItem selectedSelectorBarItem, SelectorBarItem comparedSelectorBarItem)
        {
            return Equals(selectedSelectorBarItem, comparedSelectorBarItem) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetCutAudioKind(CutAudioResultKind selectedAudioKind, CutAudioResultKind comparedAudioKind)
        {
            return Equals(selectedAudioKind, comparedAudioKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetAudioConverFilePathState(string audioCoverFilePath, bool needReverse)
        {
            return needReverse ? string.IsNullOrEmpty(audioCoverFilePath) ? Visibility.Visible : Visibility.Collapsed : string.IsNullOrEmpty(audioCoverFilePath) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
