using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频混流文件数据模型
    /// </summary>
    public class VideoMixedFlowFileModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 文件缩略图
        /// </summary>
        private BitmapSource _fileThumbnailSource;

        public BitmapSource FileThumbnailSource
        {
            get { return _fileThumbnailSource; }

            set
            {
                if (!Equals(_fileThumbnailSource, value))
                {
                    _fileThumbnailSource = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileThumbnailSource)));
                }
            }
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSize { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        #region 视频，音频部分

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public string Duration { get; set; }

        #endregion 视频，音频部分

        #region 视频部分

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public string ScreenSizeHeight { get; set; }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public string ScreenSizeWidth { get; set; }

        #endregion 视频部分

        #region 音频部分

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

        #endregion 音频部分

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
