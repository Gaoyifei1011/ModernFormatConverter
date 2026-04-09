using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 转换类型数据模型
    /// </summary>
    public class ConversionTypeModel
    {
        public string ConversionType { get; set; }

        public VideoConversionTypeKind ConversionTypeKind { get; set; }
    }
}
