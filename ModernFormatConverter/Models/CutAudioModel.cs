using System;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 剪辑音频数据模型
    /// </summary>
    public class CutAudioModel
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
        /// 视频封面文件路径
        /// </summary>
        public string AudioCoverFilePath { get; set; }
    }
}
