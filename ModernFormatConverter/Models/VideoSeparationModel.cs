using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频分离数据模型
    /// </summary>
    public class VideoSeparationModel
    {
        public WinRTObservableCollection<VideoSeparationFileModel> VideoSeparationFileCollection { get; } = [];
    }
}
