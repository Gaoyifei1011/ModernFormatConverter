using System.Windows.Media.Imaging;
using Windows.UI;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 文件格式转换数据类型
    /// </summary>
    public class VideoFormatConversionModel : VideoConversionFileModel
    {
        #region 视频参数部分

        /// <summary>
        /// 格式转换类型
        /// </summary>
        public string VideoFormatConversion { get; set; }

        /// <summary>
        /// 大小限制
        /// </summary>
        public string FileSizeLimitation { get; set; }

        /// <summary>
        /// 视频编码
        /// </summary>
        public string VideoEncoding { get; set; }

        /// <summary>
        /// 屏幕大小
        /// </summary>
        public string ScreenSize { get; set; }

        /// <summary>
        /// 比特率
        /// </summary>
        public string VideoBitRate { get; set; }

        /// <summary>
        /// 恒定码率因子
        /// </summary>
        public string ConstantRateFactor { get; set; }

        /// <summary>
        /// 编码选用的 GPU 类型
        /// </summary>
        public string GPU { get; set; }

        /// <summary>
        /// 每秒帧数
        /// </summary>
        public string FramePerSecond { get; set; }

        /// <summary>
        /// 宽高比
        /// </summary>
        public string AspectRatio { get; set; }

        /// <summary>
        /// 二次编码
        /// </summary>
        public bool SecondaryEncoding { get; set; }

        /// <summary>
        /// 关键帧间隔
        /// </summary>
        public string KeyFrameInterval { get; set; }

        /// <summary>
        /// 反交错
        /// </summary>
        public bool DeInterlace { get; set; }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public Rotation Rotation { get; set; }

        /// <summary>
        /// 淡入效果
        /// </summary>
        public bool VideoFadeInEffect { get; set; }

        /// <summary>
        /// 淡出效果
        /// </summary>
        public bool VideoFadeOutEffect { get; set; }

        #endregion 视频参数部分

        #region 音频参数部分

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
        public int Volume { get; set; }

        /// <summary>
        /// 保留所有源输入流
        /// </summary>
        public bool PreserveAllSourceInputAudioStream { get; set; }

        /// <summary>
        /// 淡入效果
        /// </summary>
        public bool AudioFadeInEffect { get; set; }

        /// <summary>
        /// 淡出效果
        /// </summary>
        public bool AudioFadeOutEffect { get; set; }

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

        #region 字幕参数部分

        /// <summary>
        /// 保留所有源输入流
        /// </summary>
        public bool PreserveAllSourceInputSubtitleStream { get; set; }

        /// <summary>
        /// 附加字幕
        /// </summary>
        public string AdditionalSubtitles { get; set; }

        /// <summary>
        /// 字幕嵌入类型
        /// </summary>
        public string SubtitleNestType { get; set; }

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public string FontSize { get; set; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color FontColor { get; set; }

        /// <summary>
        /// 字体边框风格
        /// </summary>
        public string FontBorderStyle { get; set; }

        /// <summary>
        /// 轮廓线大小
        /// </summary>
        public string CounterLineSize { get; set; }

        /// <summary>
        /// 轮廓线颜色
        /// </summary>
        public Color CounterLineColor { get; set; }

        /// <summary>
        /// 阴影大小
        /// </summary>
        public int ShadowSize { get; set; }

        #endregion 字幕参数部分
    }
}
