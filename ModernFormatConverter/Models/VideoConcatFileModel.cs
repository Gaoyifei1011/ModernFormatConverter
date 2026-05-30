using System;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频合并文件数据模型
    /// </summary>
    public class VideoConcatFileModel
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSizeString { get; set; }

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public string DurationString { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public string ScreenSizeHeight { get; set; }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public string ScreenSizeWidth { get; set; }
    }
}
