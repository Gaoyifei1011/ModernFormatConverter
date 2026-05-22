using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 音频转换文件类型
    /// </summary>
    public class PhotoConversionFileModel : INotifyPropertyChanged
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
        /// 图片高度
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
