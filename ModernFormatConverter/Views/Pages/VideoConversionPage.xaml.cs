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

        public WinRTObservableCollection<ConversionTypeModel> ConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoConversionPage()
        {
            InitializeComponent();
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoFormatConversionString,
                ConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoConcatString,
                ConversionTypeKind = VideoConversionTypeKind.VideoConcat
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoMixedFlowString,
                ConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoSplitString,
                ConversionTypeKind = VideoConversionTypeKind.VideoSplit
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoSeparationString,
                ConversionTypeKind = VideoConversionTypeKind.VideoSeparation
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoExportFrameString,
                ConversionTypeKind = VideoConversionTypeKind.VideoExportFrame
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoSpeedAdjustmentString,
                ConversionTypeKind = VideoConversionTypeKind.VideoSpeedAdjustment
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoAngleAdjustmentString,
                ConversionTypeKind = VideoConversionTypeKind.VideoAngleAdjustment
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoRewindString,
                ConversionTypeKind = VideoConversionTypeKind.VideoRewind
            });
            ConversionTypeCollection.Add(new ConversionTypeModel
            {
                ConversionType = VideoSplitScreenString,
                ConversionTypeKind = VideoConversionTypeKind.VideoSplitScreen
            });
            SelectedConversionType = ConversionTypeCollection[0];
        }

        #region 第一部分：视频转换页面——挂载的事件

        /// <summary>
        /// 视频转换列表选中项发生变化时触发的事件
        /// </summary>
        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as ConversionTypeModel;
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
