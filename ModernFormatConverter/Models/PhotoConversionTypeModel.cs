using ModernFormatConverter.Extensions.DataType.Class;
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
        /// 图片转换文件配置列表
        /// </summary>
        public WinRTObservableCollection<PhotoConversionFileModel> PhotoConversionFileCollection { get; } = [];

    }
}
