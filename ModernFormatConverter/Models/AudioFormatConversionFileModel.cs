using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频格式转换文件数据模型
    /// </summary>
    public class AudioFormatConversionFileModel : INotifyPropertyChanged
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
        /// 音频持续时间
        /// </summary>
        public TimeSpan Duration { get; set; }

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

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 音频编辑
        /// </summary>
        public AudioEditModel AudioEdit { get; set; }

        /// <summary>
        /// 音频转换输出配置
        /// </summary>
        public AudioConversionOutputConfigurationModel AudioConversionOutputConfiguration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
