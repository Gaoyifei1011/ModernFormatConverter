using System;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频合并文件数据模型
    /// </summary>
    public class AudioConcatFileModel
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
        /// 视频持续时间
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public string SamplingRate { get; set; }

        /// <summary>
        /// 比特率
        /// </summary>
        public string BitRate { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 视频转换输出配置
        /// </summary>
        public AudioConversionOutputConfigurationModel AudioConversionOutputConfiguration { get; set; }
    }
}
