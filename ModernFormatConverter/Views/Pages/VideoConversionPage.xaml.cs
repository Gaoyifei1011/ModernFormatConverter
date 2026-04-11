using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using System.ComponentModel;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 视频转换页面
    /// </summary>
    public sealed partial class VideoConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string VideoAngleAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoAngleAdjustment");
        private readonly string VideoConcatString = ResourceService.VideoConversionResource.GetString("VideoConcat");
        private readonly string VideoExportFrameString = ResourceService.VideoConversionResource.GetString("VideoExportFrame");
        private readonly string VideoFormatConversionString = ResourceService.VideoConversionResource.GetString("VideoFormatConversion");
        private readonly string VideoMixedFlowString = ResourceService.VideoConversionResource.GetString("VideoMixedFlow");
        private readonly string VideoSeparationString = ResourceService.VideoConversionResource.GetString("VideoSeparation");
        private readonly string VideoSplitString = ResourceService.VideoConversionResource.GetString("VideoSplit");
        private readonly string VideoSpeedAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoSpeedAdjustment");
        private readonly string VideoRewindString = ResourceService.VideoConversionResource.GetString("VideoRewind");
        private readonly string VideoSplitScreenString = ResourceService.VideoConversionResource.GetString("VideoSplitScreen");

        private ConversionTypeModel _selectedConversionType;

        public ConversionTypeModel SelectedConversionType
        {
            get { return _selectedConversionType; }

            set
            {
                if (!Equals(_selectedConversionType, value))
                {
                    _selectedConversionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedConversionType)));
                }
            }
        }

        public WinRTObservableCollection<VideoConversionTypeModel> ConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoConversionPage()
        {
            InitializeComponent();
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoFormatConversionString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoConcatString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoConcat
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoMixedFlowString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSplitString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSplit
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSeparationString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoExportFrameString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoExportFrame
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSpeedAdjustmentString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSpeedAdjustment
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoAngleAdjustmentString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoAngleAdjustment
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoRewindString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoRewind
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSplitScreenString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSplitScreen
            });
            SelectedConversionType = ConversionTypeCollection[0];
        }

        #region 第一部分：视频转换页面——挂载的事件

        /// <summary>
        /// 视频转换列表选中项发生变化时触发的事件
        /// </summary>
        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as VideoConversionTypeModel;
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            ConversionToolsPage.Current?.Close();
        }

        #endregion 第一部分：视频转换页面——挂载的事件
    }
}
