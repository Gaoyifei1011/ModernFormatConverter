using System;
using System.Collections.Generic;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 剪辑视频数据模型
    /// </summary>
    public class VideoEditModel
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// 选中区域操作类型
        /// </summary>
        public string SelectRegionOperation { get; set; }

        public List<SelectRegionOperationModel> SelectRegionOperationList { get; } = [];

        /// <summary>
        /// 视频封面文件路径
        /// </summary>
        public string VideoCoverFilePath { get; set; }
    }
}
