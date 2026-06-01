using ModernFormatConverter.Extensions.DataType.Class;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频合并数据模型
    /// </summary>
    public class AudioConcatModel
    {
        /// <summary>
        /// 音频合并文件列表
        /// </summary>
        public WinRTObservableCollection<AudioConcatFileModel> AudioConcatFileCollection { get; } = [];

        /// <summary>
        /// 音频转换输出配置
        /// </summary>
        public AudioConversionOutputConfigurationModel AudioConversionOutputConfiguration { get; set; }
    }
}
