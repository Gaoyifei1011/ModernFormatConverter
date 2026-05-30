using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频格式转换文件数据模型
    /// </summary>
    public class VideoFormatConversionFileModel : INotifyPropertyChanged
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
        public long FileSize { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSizeString { get; set; }

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public string DurationString { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public string ScreenSizeHeight { get; set; }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public string ScreenSizeWidth { get; set; }

        /// <summary>
        /// 视频编辑
        /// </summary>
        public VideoEditModel VideoEdit { get; set; }

        /// <summary>
        /// 视频转换输出配置
        /// </summary>
        public VideoConversionOutputConfigurationModel VideoConversionOutputConfiguration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
