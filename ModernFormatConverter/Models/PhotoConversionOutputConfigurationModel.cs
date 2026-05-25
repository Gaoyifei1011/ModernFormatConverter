using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 图片转换输出配置数据类型
    /// </summary>
    public class PhotoConversionOutputConfigurationModel
    {
        public PhotoConversionTypeKind PhotoConversionTypeKind { get; set; }

        #region 图片参数部分

        /// <summary>
        /// 格式转换类型
        /// </summary>
        public string FormatConversionType { get; set; }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        public bool IsImageCropped { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 裁剪图片：X 坐标
        /// </summary>
        public int XCoordinate { get; set; }

        /// <summary>
        /// 裁剪图片：Y 坐标
        /// </summary>
        public int YCoordinate { get; set; }

        /// <summary>
        /// 裁剪后的宽度
        /// </summary>
        public int ClipWidth { get; set; }

        /// <summary>
        /// 裁剪后的高度
        /// </summary>
        public int ClipHeight { get; set; }

        /// <summary>
        /// 对比度
        /// </summary>
        public double ConstrastRatio { get; set; }

        /// <summary>
        /// 曝光
        /// </summary>
        public double Exposure { get; set; }

        /// <summary>
        /// 饱和度
        /// </summary>
        public double Saturation { get; set; }

        /// <summary>
        /// 色温
        /// </summary>
        public double ColorTemperature { get; set; }

        /// <summary>
        /// 色调
        /// </summary>
        public double Tone { get; set; }

        /// <summary>
        /// 模糊
        /// </summary>
        public double Blur { get; set; }

        /// <summary>
        /// 灰度
        /// </summary>
        public bool GrayScale { get; set; }

        /// <summary>
        /// 反相
        /// </summary>
        public bool Reversal { get; set; }

        #endregion 图片参数部分
    }
}
