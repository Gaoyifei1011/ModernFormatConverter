using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 视频导出图片输出配置页面
    /// </summary>
    public sealed partial class VideoExportPictureOutputConfigurationPage : Page, INotifyPropertyChanged
    {
        private readonly string TimePeriodString = ResourceService.VideoExportPictureOutputConfigurationResource.GetString("TimePeriod");
        private readonly string TimePointString = ResourceService.VideoExportPictureOutputConfigurationResource.GetString("TimePoint");
        private VideoConversionNavigationParameter videoConversionNavigationParameter;

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

        private ComboBoxItemModel _selectedSavePictureFormat;

        public ComboBoxItemModel SelectedSavePictureFormat
        {
            get { return _selectedSavePictureFormat; }

            set
            {
                if (!Equals(_selectedSavePictureFormat, value))
                {
                    _selectedSavePictureFormat = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSavePictureFormat)));
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

        private ComboBoxItemModel _selectedVideoExportPictureKind;

        public ComboBoxItemModel SelectedVideoExportPictureKind
        {
            get { return _selectedVideoExportPictureKind; }

            set
            {
                if (!Equals(_selectedVideoExportPictureKind, value))
                {
                    _selectedVideoExportPictureKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVideoExportPictureKind)));
                }
            }
        }

        private int _timeHours;

        public int TimeHours
        {
            get { return _timeHours; }

            set
            {
                if (!Equals(_timeHours, value))
                {
                    _timeHours = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeHours)));
                }
            }
        }

        private int _timeMinutes;

        public int TimeMinutes
        {
            get { return _timeMinutes; }

            set
            {
                if (!Equals(_timeMinutes, value))
                {
                    _timeMinutes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeMinutes)));
                }
            }
        }

        private int _timeSeconds;

        public int TimeSeconds
        {
            get { return _timeSeconds; }

            set
            {
                if (!Equals(_timeSeconds, value))
                {
                    _timeSeconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeSeconds)));
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

        private int _pictureExportPerSecond = 1000;

        public int PictureExportPerSecond
        {
            get { return _pictureExportPerSecond; }

            set
            {
                if (!Equals(_pictureExportPerSecond, value))
                {
                    _pictureExportPerSecond = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PictureExportPerSecond)));
                }
            }
        }

        public List<ComboBoxItemModel> SavePictureFormatList { get; } =
        [
            new ComboBoxItemModel(){ SelectedValue = "BMP", DisplayMember = ".bmp" },
            new ComboBoxItemModel(){ SelectedValue = "GIF", DisplayMember = ".gif" },
            new ComboBoxItemModel(){ SelectedValue = "ICO", DisplayMember = ".ico" },
            new ComboBoxItemModel(){ SelectedValue = "JPEG", DisplayMember = ".jpeg" },
            new ComboBoxItemModel(){ SelectedValue = "JPG", DisplayMember = ".jpg" },
            new ComboBoxItemModel(){ SelectedValue = "PNG", DisplayMember = ".png" },
            new ComboBoxItemModel(){ SelectedValue = "WEBP", DisplayMember = ".webp" }
        ];

        public List<ComboBoxItemModel> VideoExportPictureKindList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoExportPictureOutputConfigurationPage()
        {
            InitializeData();
            InitializeComponent();
        }

        #region 第一部分：重载父类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            SelectedItem = VideoExportPictureOutputConfigurationSelectorBar.Items[0];
            if (args.Parameter is VideoConversionNavigationParameter videoConversionNavigationParameterData)
            {
                videoConversionNavigationParameter = videoConversionNavigationParameterData;
                if (videoConversionNavigationParameter.IsGlobalSettings)
                {
                    UpdateData(null);
                }
                else
                {
                    if (videoConversionNavigationParameter.VideoConversionData is VideoExportPictureFileModel videoExportPictureFile && videoExportPictureFile.VideoExportPictureOutputConfiguration is not null)
                    {
                        UpdateData(videoExportPictureFile.VideoExportPictureOutputConfiguration);
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
            videoConversionNavigationParameter = null;
        }

        #endregion 第一部分：重载父类事件

        #region 第二部分：视频导出图片输出配置页面——挂载的事件

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 视频导出图片
            if (videoConversionNavigationParameter.IsGlobalSettings)
            {
                if (videoConversionNavigationParameter.VideoConversionData is List<VideoExportPictureFileModel> videoExportPictureFileList)
                {
                    foreach (VideoExportPictureFileModel videoExportPictureFile in videoExportPictureFileList)
                    {
                        if (videoExportPictureFile.VideoExportPictureOutputConfiguration is not null)
                        {
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.SavePictureFormat = Convert.ToString(SelectedSavePictureFormat.SelectedValue);
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.VideoExportPictureKind = Convert.ToString(SelectedVideoExportPictureKind.SelectedValue);
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.ExportTime = new(TimeHours, TimeMinutes, TimeSeconds);
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.StartTime = new(TimeStartHours, TimeStartMinutes, TimeStartSeconds);
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.EndTime = new(TimeEndHours, TimeEndMinutes, TimeEndSeconds);
                            videoExportPictureFile.VideoExportPictureOutputConfiguration.PictureExportPerSecond = PictureExportPerSecond;
                        }
                    }
                }
            }
            else
            {
                if (videoConversionNavigationParameter.VideoConversionData is VideoExportPictureFileModel videoExportPictureFile && videoExportPictureFile.VideoExportPictureOutputConfiguration is not null)
                {
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.SavePictureFormat = Convert.ToString(SelectedSavePictureFormat.SelectedValue);
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.VideoExportPictureKind = Convert.ToString(SelectedVideoExportPictureKind.SelectedValue);
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.ExportTime = new(TimeHours, TimeMinutes, TimeSeconds);
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.StartTime = new(TimeStartHours, TimeStartMinutes, TimeStartSeconds);
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.EndTime = new(TimeEndHours, TimeEndMinutes, TimeEndSeconds);
                    videoExportPictureFile.VideoExportPictureOutputConfiguration.PictureExportPerSecond = PictureExportPerSecond;
                }
            }

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage)
            {
                videoConversionPage.NavigateTo(videoConversionPage.PageList[0], null, false);
            }
        }

        /// <summary>
        /// 保存图片格式菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSelectedSavePictureFormatSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel savePictureFormat && !Equals(SelectedSavePictureFormat, savePictureFormat))
            {
                SelectedSavePictureFormat = savePictureFormat;
            }
        }

        /// <summary>
        /// 视频导出图片方式菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnSelectedVideoExportPictureKindSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel videoExportPictureKind && !Equals(SelectedVideoExportPictureKind, videoExportPictureKind))
            {
                SelectedVideoExportPictureKind = videoExportPictureKind;
            }
        }

        /// <summary>
        /// 时间点时发生变化时触发的事件
        /// </summary>
        private void OnTimeHoursValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeHours = int.MaxValue;
                TimeHours = newValue < 0 ? 0 : newValue;
            }
        }

        /// <summary>
        /// 时间点分发生变化时触发的事件
        /// </summary>
        private void OnTimeMinutesValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeMinutes = int.MaxValue;
                TimeMinutes = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeMinutes = 59;
                }
                else if (newValue < 0)
                {
                    TimeMinutes = 0;
                }
                else
                {
                    TimeMinutes = newValue;
                }
            }
        }

        /// <summary>
        /// 时间点秒发生变化时触发的事件
        /// </summary>
        private void OnTimeSecondsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeSeconds = int.MaxValue;
                TimeSeconds = Convert.ToInt32(args.OldValue);

                if (newValue > 59)
                {
                    TimeSeconds = 59;
                }
                else if (newValue < 0)
                {
                    TimeSeconds = 0;
                }
                else
                {
                    TimeSeconds = newValue;
                }
            }
        }

        /// <summary>
        /// 时间段起始时发生变化时触发的事件
        /// </summary>
        private void OnTimeStartHoursValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeStartHours = int.MaxValue;
                TimeStartHours = newValue < 0 ? 0 : newValue;
            }
        }

        /// <summary>
        /// 时间段起始分发生变化时触发的事件
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
                    TimeStartMinutes = newValue;
                }
            }
        }

        /// <summary>
        /// 时间段起始秒发生变化时触发的事件
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
                    TimeStartSeconds = newValue;
                }
            }
        }

        /// <summary>
        /// 时间段起始时发生变化时触发的事件
        /// </summary>
        private void OnTimeEndHoursValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                TimeEndHours = int.MaxValue;
                TimeEndHours = Convert.ToInt32(args.OldValue);
                TimeEndHours = newValue < 0 ? 0 : newValue;
            }
        }

        /// <summary>
        /// 时间段起始分发生变化时触发的事件
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
                    TimeEndMinutes = newValue;
                }
            }
        }

        /// <summary>
        /// 时间段起始秒发生变化时触发的事件
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
                    TimeEndSeconds = newValue;
                }
            }
        }

        /// <summary>
        /// 每秒导出图片数量发生变化时触发的事件
        /// </summary>
        private void OnPictureExportPerSecondValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                PictureExportPerSecond = int.MaxValue;
                PictureExportPerSecond = Convert.ToInt32(args.OldValue);
                PictureExportPerSecond = newValue < 1 ? 1 : newValue;
            }
        }

        #endregion 第二部分：视频导出图片输出配置页面——挂载的事件

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            VideoExportPictureKindList.Add(new ComboBoxItemModel() { SelectedValue = "TimePoint", DisplayMember = TimePointString });
            VideoExportPictureKindList.Add(new ComboBoxItemModel() { SelectedValue = "TimePeriod", DisplayMember = TimePeriodString });
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(VideoExportPictureOutputConfigurationModel videoExportPictureOutputConfiguration)
        {
            SelectedSavePictureFormat = videoExportPictureOutputConfiguration is not null && SavePictureFormatList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoExportPictureOutputConfiguration.SavePictureFormat)) is ComboBoxItemModel selectedSavePictureFormat ? selectedSavePictureFormat : SavePictureFormatList[5];

            SelectedVideoExportPictureKind = videoExportPictureOutputConfiguration is not null && VideoExportPictureKindList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), videoExportPictureOutputConfiguration.VideoExportPictureKind)) is ComboBoxItemModel selectedVideoExportPictureKind ? selectedVideoExportPictureKind : VideoExportPictureKindList[0];

            TimeHours = videoExportPictureOutputConfiguration is not null ? (int)Math.Truncate(videoExportPictureOutputConfiguration.ExportTime.TotalHours) : 0;
            TimeMinutes = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.ExportTime.Minutes : 0;
            TimeSeconds = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.ExportTime.Seconds : 0;

            TimeStartHours = videoExportPictureOutputConfiguration is not null ? (int)Math.Truncate(videoExportPictureOutputConfiguration.StartTime.TotalHours) : 0;
            TimeStartMinutes = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.StartTime.Minutes : 0;
            TimeStartSeconds = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.StartTime.Seconds : 0;

            TimeEndHours = videoExportPictureOutputConfiguration is not null ? (int)Math.Truncate(videoExportPictureOutputConfiguration.EndTime.TotalHours) : 0;
            TimeEndMinutes = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.EndTime.Minutes : 0;
            TimeEndSeconds = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.EndTime.Seconds : 0;

            PictureExportPerSecond = videoExportPictureOutputConfiguration is not null ? videoExportPictureOutputConfiguration.PictureExportPerSecond : 1;
        }

        private Visibility GetSelectedVideoExportPictureKind(object selectedVideoExportPictureKind, object videoExportPictureKind)
        {
            return Equals(selectedVideoExportPictureKind, videoExportPictureKind) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
