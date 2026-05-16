namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 选择区域操作数据模型
    /// </summary>
    public class SelectRegionOperationModel
    {
        /// <summary>
        /// X 坐标
        /// </summary>
        public int XCoordinate { get; set; }

        /// <summary>
        /// Y 坐标
        /// </summary>
        public int YCoordinate { get; set; }

        /// <summary>
        /// 截取宽度
        /// </summary>
        public int ClipWidth { get; set; }

        /// <summary>
        /// 截取高度
        /// </summary>
        public int ClipHeight { get; set; }
    }
}
