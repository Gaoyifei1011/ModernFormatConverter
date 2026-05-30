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
        /// 视频转换类型图标
        /// </summary>
        public string VideoConversionIcon { get; set; }

        /// <summary>
        /// 视频转换类型
        /// </summary>
        public VideoConversionTypeKind VideoConversionTypeKind { get; set; }

        /// <summary>
        /// 视频格式转换数据模型（仅用于视频格式转换）
        /// </summary>
        public VideoFormatConversionModel VideoFormatConversion { get; set; }

        /// <summary>
        /// 视频合并数据模型（仅用于视频合并）
        /// </summary>
        public VideoConcatModel VideoConcat { get; set; }

        /// <summary>
        /// 视频混流数据模型（仅用于视频混流）
        /// </summary>
        public VideoMixedFlowModel VideoMixedFlow { get; set; }

        /// <summary>
        /// 视频分离数据模型（仅用于视频分离）
        /// </summary>
        public VideoSeparationModel VideoSeparation { get; set; }

        /// <summary>
        /// 视频导出图片数据模型（仅用于视频导出图片）
        /// </summary>
        public VideoExportPictureModel VideoExportPicture { get; set; }
    }
}
