using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频格式转换数据模型
    /// </summary>
    public class VideoFormatConversionModel
    {
        public WinRTObservableCollection<VideoFormatConversionFileModel> VideoFormatConversionFileCollection { get; } = [];
    }
}
