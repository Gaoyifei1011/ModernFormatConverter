using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 视频转换导航参数
    /// </summary>
    public class VideoConversionNavigationParameter
    {
        /// <summary>
        /// 视频转换类型
        /// </summary>
        public VideoConversionTypeKind VideoConversionTypeKind { get; set; }

        /// <summary>
        /// 是否修改全部数据
        /// </summary>
        public bool IsGlobalSettings { get; set; }

        /// <summary>
        /// 视频转换数据
        /// </summary>
        public object VideoConversionData { get; set; }
    }
}
