using System.Speech.Synthesis;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 语音类型数据模型
    /// </summary>
    public class VoiceTypeModel
    {
        /// <summary>
        /// 选中值
        /// </summary>
        public object SelectedValue { get; set; }

        /// <summary>
        /// 显示值
        /// </summary>
        public string DisplayMember { get; set; }

        /// <summary>
        /// 语音信息
        /// </summary>
        public VoiceInfo VoiceInfo { get; set; }
    }
}
