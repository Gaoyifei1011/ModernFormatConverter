namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 文本转语音输出配置
    /// </summary>
    public class TextToAudioOutputConfigurationModel
    {
        /// <summary>
        /// 语音类型
        /// </summary>
        public string VoiceType { get; set; }

        /// <summary>
        /// 阅读速率
        /// </summary>
        public int ReadingSpeed { get; set; }

        /// <summary>
        /// 音量
        /// </summary>
        public int Volume { get; set; }
    }
}
