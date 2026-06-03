using System.Windows.Media.Imaging;
using Windows.UI;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频转换输出配置数据类型
    /// </summary>
    public class VideoConversionOutputConfigurationModel
    {
        #region 视频参数部分

        /// <summary>
        /// 格式转换类型
        /// </summary>
        public string FormatConversionType { get; set; }

        /// <summary>
        /// 大小限制
        /// </summary>
        public string SizeLimitation { get; set; }

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
        /// 固定速率系数
        /// </summary>
        public int CRF { get; set; }

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
        /// 倍速播放速度
        /// </summary>
        public double SpeedPlayback { get; set; }

        /// <summary>
        /// 倒放视频
        /// </summary>
        public bool ReverseVideo { get; set; }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public Rotation Rotation { get; set; }

        /// <summary>
        /// 镜像反转
        /// </summary>
        public bool MirrorReversal { get; set; }

        /// <summary>
        /// 淡入效果
        /// </summary>
        public string VideoFadeInEffect { get; set; }

        /// <summary>
        /// 淡出效果
        /// </summary>
        public string VideoFadeOutEffect { get; set; }

        /// <summary>
        /// 输出文件夹
        /// </summary>
        public string OutputFolder { get; set; }

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
        public string Volume { get; set; }

        /// <summary>
        /// 保留所有源输入流
        /// </summary>
        public bool PreserveAllSourceInputAudioStream { get; set; }

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

        #region 字幕参数部分

        /// <summary>
        /// 保留所有源输入流
        /// </summary>
        public bool PreserveAllSourceInputSubtitleStream { get; set; }

        /// <summary>
        /// 附加字幕路径
        /// </summary>
        public string AdditionalSubtitlePath { get; set; }

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
        public int FontSize { get; set; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color? FontColor { get; set; }

        /// <summary>
        /// 字体边框风格
        /// </summary>
        public string FontBorderStyle { get; set; }

        /// <summary>
        /// 轮廓线大小
        /// </summary>
        public int CounterLineSize { get; set; }

        /// <summary>
        /// 轮廓线颜色
        /// </summary>
        public Color? CounterLineColor { get; set; }

        /// <summary>
        /// 阴影大小
        /// </summary>
        public int ShadowSize { get; set; }

        #endregion 字幕参数部分
    }
}
