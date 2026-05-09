using ModernFormatConverter.Extensions.DataType.Enums;
using System;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频导出图片配置数据模型
    /// </summary>
    public class VideoExportPictureOutputConfigurationModel
    {
        public VideoConversionTypeKind VideoConversionTypeKind { get; set; }

        /// <summary>
        /// 保存图片格式
        /// </summary>
        public string SavePictureFormat { get; set; }

        /// <summary>
        /// 视频导出图片方式：时间点，时间段
        /// </summary>
        public string VideoExportPictureKind { get; set; }

        /// <summary>
        /// 导出图片方式：时间点对应的时间
        /// </summary>
        public TimeSpan ExportTime { get; set; }

        /// <summary>
        /// 导出图片方式：时间段起始时间
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 导出图片方式：时间段结束时间
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// 每秒导出图片数量（单位：毫秒）
        /// </summary>
        public int PictureExportPerSecond { get; set; }
    }
}
