using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 图片格式转换数据模型
    /// </summary>
    public class PhotoFormatConversionModel
    {
        public WinRTObservableCollection<PhotoFormatConversionFileModel> PhotoFormatConversionFileCollection { get; } = [];
    }
}
