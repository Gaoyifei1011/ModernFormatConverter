using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 音频转换导航参数
    /// </summary>
    public class AudioConversionNavigationParameter
    {
        /// <summary>
        /// 音频转换类型
        /// </summary>
        public AudioConversionTypeKind AudioConversionTypeKind { get; set; }

        /// <summary>
        /// 是否修改全部数据
        /// </summary>
        public bool IsGlobalSettings { get; set; }

        /// <summary>
        /// 音频转换数据
        /// </summary>
        public object AudioConversionData { get; set; }
    }
}
