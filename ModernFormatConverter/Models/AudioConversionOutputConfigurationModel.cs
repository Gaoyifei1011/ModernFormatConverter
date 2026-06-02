namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频转换输出配置数据类型
    /// </summary>
    public class AudioConversionOutputConfigurationModel
    {
        #region 音频参数部分

        /// <summary>
        /// 格式转换类型
        /// </summary>
        public string FormatConversionType { get; set; }

        /// <summary>
        /// 音频编码
        /// </summary>
        public string AudioEncoding { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public string SamplingRate { get; set; }

        /// <summary>
        /// 比特率
        /// </summary>
        public string AudioBitRate { get; set; }

        /// <summary>
        /// 声道
        /// </summary>
        public string SoundTrack { get; set; }

        /// <summary>
        /// 关闭音效
        /// </summary>
        public bool CloseSoundEffect { get; set; }

        /// <summary>
        /// 音量
        /// </summary>
        public string Volume { get; set; }

        /// <summary>
        /// 可变采样率
        /// </summary>
        public string VariableBitRate { get; set; }

        /// <summary>
        /// 采样格式
        /// </summary>
        public string SamplingFormat { get; set; }

        /// <summary>
        /// 淡入效果
        /// </summary>
        public string AudioFadeInEffect { get; set; }

        /// <summary>
        /// 淡出效果
        /// </summary>
        public string AudioFadeOutEffect { get; set; }

        /// <summary>
        /// 回声
        /// </summary>
        public bool Echo { get; set; }

        /// <summary>
        /// 降噪
        /// </summary>
        public bool DeNoise { get; set; }

        /// <summary>
        /// 反向
        /// </summary>
        public bool Reverse { get; set; }

        #endregion 音频参数部分
    }
}
