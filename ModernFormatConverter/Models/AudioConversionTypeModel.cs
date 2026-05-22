using ModernFormatConverter.Extensions.DataType.Class;
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
        /// 文本转语音数据模型（仅用于文本转语音页面）
        /// </summary>
        public TextToAudioModel TextToAudio { get; set; }

        /// <summary>
        /// 音频转换文件配置列表
        /// </summary>
        public WinRTObservableCollection<AudioConversionFileModel> AudioConversionFileCollection { get; } = [];
    }
}
