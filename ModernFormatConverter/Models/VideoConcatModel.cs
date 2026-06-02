using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频合并数据模型
    /// </summary>
    public class VideoConcatModel
    {
        /// <summary>
        /// 视频合并文件列表
        /// </summary>
        public WinRTObservableCollection<VideoConcatFileModel> VideoConcatFileCollection { get; } = [];

        /// <summary>
        /// 视频转换输出配置
        /// </summary>
        public VideoConversionOutputConfigurationModel VideoConversionOutputConfiguration { get; set; }
    }
}
