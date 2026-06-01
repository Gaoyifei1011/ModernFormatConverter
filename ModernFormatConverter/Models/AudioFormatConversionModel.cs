using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频格式转换数据模型
    /// </summary>
    public class AudioFormatConversionModel
    {
        public WinRTObservableCollection<AudioFormatConversionFileModel> AudioFormatConversionFileCollection { get; } = [];
    }
}
