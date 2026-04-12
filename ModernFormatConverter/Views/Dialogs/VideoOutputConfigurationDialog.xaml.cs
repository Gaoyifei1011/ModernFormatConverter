using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

// 抑制 IDE0060 警告
#pragma warning disable IDE0060

namespace ModernFormatConverter.Views.Dialogs
{
    /// <summary>
    /// 视频输出配置对话框
    /// </summary>
    public sealed partial class VideoOutputConfigurationDialog : ContentDialog, INotifyPropertyChanged
    {
        private readonly string DefaultSizeString = ResourceService.DialogResource.GetString("DefaultSize");
        private readonly string NoneString = ResourceService.DialogResource.GetString("None");
        private readonly string CustomString = ResourceService.DialogResource.GetString("Custom");

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

        private KeyValuePair<string, string> _selectedFormatConversionType;

        public KeyValuePair<string, string> SelectedFormatConversionType
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

        private KeyValuePair<string, string> _selectedSizeLimitation;

        public KeyValuePair<string, string> SelectedSizeLimitation
        {
            get { return _selectedSizeLimitation; }

            set
            {
                _selectedSizeLimitation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSizeLimitation)));
            }
        }

        private KeyValuePair<string, string> _selectedScreenSize;

        public KeyValuePair<string, string> SelectedScreenSize
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

        private bool _isSaved;

        public bool IsSaved
        {
            get { return _isSaved; }

            set
            {
                _isSaved = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSaved)));
            }
        }

        public List<KeyValuePair<string, string>> FormatConversionTypeList { get; } =
        [
            new KeyValuePair<string, string>("MP4", ".mp4"),
            new KeyValuePair<string, string>("MKV", ".mkv"),
            new KeyValuePair<string, string>("GIF", ".gif"),
            new KeyValuePair<string, string>("WebM", ".webm"),
            new KeyValuePair<string, string>("AVI", ".avi"),
            new KeyValuePair<string, string>("FLV", ".flv"),
            new KeyValuePair<string, string>("MOV", ".mov"),
            new KeyValuePair<string, string>("M3U8", ".m3u8"),
            new KeyValuePair<string, string>("WMV", ".wmv"),
            new KeyValuePair<string, string>("3GP", ".3gp"),
            new KeyValuePair<string, string>("3G2", ".3g2"),
            new KeyValuePair<string, string>("MPG", ".mpg"),
            new KeyValuePair<string, string>("VOB", ".vob"),
            new KeyValuePair<string, string>("OGG", ".ogg"),
            new KeyValuePair<string, string>("SWF", ".swf"),
        ];

        public List<KeyValuePair<string, string>> SizeLimitationList { get; } = [];

        public List<KeyValuePair<string, string>> ScreenSizeList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoOutputConfigurationDialog(VideoConversionFileModel videoConversionFileModel)
        {
            InitializeComponent();
            Initialize();

            // TODO：未完成
            SelectedFormatConversionType = FormatConversionTypeList[0];
            SelectedSizeLimitation = SizeLimitationList[0];
            SelectedScreenSize = ScreenSizeList[0];
        }

        public VideoOutputConfigurationDialog(VideoConversionTypeKind videoConversionTypeKind, WinRTObservableCollection<VideoConversionFileModel> videoConversionFileCollection)
        {
            InitializeComponent();
            Initialize();
            SelectedFormatConversionType = FormatConversionTypeList[0];
            SelectedSizeLimitation = SizeLimitationList[0];
            SelectedScreenSize = ScreenSizeList[0];
        }

        #region 第一部分：视频输出配置对话框——挂载的事件

        /// <summary>
        /// 关闭对话框
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            Hide();
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
            Hide();
            IsSaved = true;
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
        /// 格式转换类型选中项发生变化时触发的事件
        /// </summary>
        private void OnFormatConversionTypeRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> formatConversionType)
            {
                SelectedFormatConversionType = formatConversionType;
            }
        }

        /// <summary>
        /// 大小限制选中项发生变化时触发的事件
        /// </summary>
        private void OnSizeLimitationRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> sizeLimitation)
            {
                SelectedSizeLimitation = sizeLimitation;
            }
        }

        /// <summary>
        /// 屏幕大小选中项发生变化时触发的事件
        /// </summary>
        private void OnScreenSizeRadioGroupSelectClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is KeyValuePair<string, string> screenSize)
            {
                SelectedScreenSize = screenSize;
                if (!Equals(SelectedScreenSize.Key, ScreenSizeList[17].Key))
                {
                    ScreenWidth = 1;
                    ScreenHeight = 1;
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoOutputConfigurationDialog), nameof(OnScreenWidthValueChanged), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoOutputConfigurationDialog), nameof(OnScreenHeightValueChanged), 1, e);
                }
            }
        }

        #endregion 第一部分：视频输出配置对话框——挂载的事件

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Initialize()
        {
            SelectedItem = VideoOutputConfigurationSelectorBar.Items[0];
            SizeLimitationList.Add(new KeyValuePair<string, string>("None", NoneString));
            SizeLimitationList.Add(new KeyValuePair<string, string>("10", "10MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("15", "15MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("20", "20MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("25", "25MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("30", "30MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("35", "35MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("40", "40MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("50", "50MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("60", "60MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("70", "70MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("80", "80MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("90", "90MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("100", "100MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("150", "150MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("200", "200MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("300", "300MB"));
            SizeLimitationList.Add(new KeyValuePair<string, string>("500", "500MB"));

            ScreenSizeList.Add(new KeyValuePair<string, string>("DefaultSize", DefaultSizeString));
            ScreenSizeList.Add(new KeyValuePair<string, string>("360P", "360p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("480P", "480p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("720P", "720p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1080P", "1080p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("2160P", "2160p"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("480I", "480i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("720I", "720i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1280I", "1280i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("1920I", "1920i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("3840I", "3840i"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("25%", "25%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("50%", "50%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("75%", "75%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("125%", "125%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("150%", "150%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("200%", "200%"));
            ScreenSizeList.Add(new KeyValuePair<string, string>("Custom", CustomString));
        }

        /// <summary>
        /// 获取选中的屏幕大小项
        /// </summary>
        private Visibility GetSelectedScreenSize(string selectedScreenSize, string comparedScreenSize)
        {
            return string.Equals(selectedScreenSize, comparedScreenSize, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
