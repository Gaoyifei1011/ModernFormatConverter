using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频转换类型数据模型
    /// </summary>
    public class AudioConversionTypeModel
    {
        /// <summary>
        /// 音频转换类型名称
        /// </summary>
        public string AudioConversionType { get; set; }

        /// <summary>
        /// 音频转换类型图标
        /// </summary>
        public string AudioConversionIcon { get; set; }

        /// <summary>
        /// 音频转换类型
        /// </summary>
        public AudioConversionTypeKind AudioConversionTypeKind { get; set; }

        /// <summary>
        /// 音频格式转换数据类型（仅用于音频格式转换）
        /// </summary>
        public AudioFormatConversionModel AudioFormatConversion { get; set; }

        /// <summary>
        /// 音频合并数据模型（仅用于音频合并）
        /// </summary>
        public AudioConcatModel AudioConcat { get; set; }

        /// <summary>
        /// 文本转语音数据模型（仅用于文本转语音）
        /// </summary>
        public TextToAudioModel TextToAudio { get; set; }
    }
}
