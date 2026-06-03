using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 图片转换类型数据模型
    /// </summary>
    public class PhotoConversionTypeModel
    {
        /// <summary>
        /// 图片转换类型名称
        /// </summary>
        public string PhotoConversionType { get; set; }

        /// <summary>
        /// 图片转换类型图标
        /// </summary>
        public string PhotoConversionIcon { get; set; }

        /// <summary>
        /// 图片转换类型
        /// </summary>
        public PhotoConversionTypeKind PhotoConversionTypeKind { get; set; }

        /// <summary>
        /// 图片格式转换数据类型（仅用于图片格式转换）
        /// </summary>
        public PhotoFormatConversionModel PhotoFormatConversion { get; set; }
    }
}
