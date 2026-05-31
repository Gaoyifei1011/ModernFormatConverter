using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频导出图片数据模型
    /// </summary>
    public class VideoExportPictureModel
    {
        /// <summary>
        /// 视频导出图片列表
        /// </summary>
        public WinRTObservableCollection<VideoExportPictureFileModel> VideoExportPictureFileCollection { get; } = [];
    }
}
