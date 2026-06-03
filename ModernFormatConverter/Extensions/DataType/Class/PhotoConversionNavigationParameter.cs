using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 图片转换导航参数
    /// </summary>
    public class PhotoConversionNavigationParameter
    {
        /// <summary>
        /// 图片转换类型
        /// </summary>
        public PhotoConversionTypeKind PhotoConversionTypeKind { get; set; }

        /// <summary>
        /// 是否修改全部数据
        /// </summary>
        public bool IsGlobalSettings { get; set; }

        /// <summary>
        /// 图片转换数据
        /// </summary>
        public object PhotoConversionData { get; set; }
    }
}
