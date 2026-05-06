using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频转换类型数据模型
    /// </summary>
    public class VideoConversionTypeModel
    {
        /// <summary>
        /// 视频转换类型名称
        /// </summary>
        public string VideoConversionType { get; set; }

        /// <summary>
        /// 视频转换类型
        /// </summary>
        public VideoConversionTypeKind VideoConversionTypeKind { get; set; }

        /// <summary>
        /// 视频混流数据模型（仅用于视频混流页面）
        /// </summary>
        public VideoMixedFlowModel VideoMixedFlow { get; set; }

        /// <summary>
        /// 视频转换文件配置列表
        /// </summary>
        public WinRTObservableCollection<VideoConversionFileModel> VideoConversionFileCollection { get; } = [];
    }
}
