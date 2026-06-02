using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Enums;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 语音转文本数据模型
    /// </summary>
    public class TextToAudioModel : INotifyPropertyChanged
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

        private TextToAudioType _textToAudioType;

        public TextToAudioType TextToAudioType
        {
            get { return _textToAudioType; }

            set
            {
                if (!Equals(_textToAudioType, value))
                {
                    _textToAudioType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextToAudioType)));
                }
            }
        }

        /// <summary>
        /// 转换输入文本
        /// </summary>
        private string _inputText;

        public string InputText
        {
            get { return _inputText; }

            set
            {
                if (!string.Equals(_inputText, value))
                {
                    _inputText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputText)));
                }
            }
        }

        private bool _isTextFileSelected;

        public bool IsTextFileSelected
        {
            get { return _isTextFileSelected; }

            set
            {
                if (!Equals(_isTextFileSelected, value))
                {
                    _isTextFileSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTextFileSelected)));
                }
            }
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        private string _fileName;

        public string FileName
        {
            get { return _fileName; }

            set
            {
                if (!string.Equals(_fileName, value))
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }

            set
            {
                if (!string.Equals(_filePath, value))
                {
                    _filePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
                }
            }
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        private string _fileSize;

        public string FileSize
        {
            get { return _fileSize; }

            set
            {
                if (!string.Equals(_fileSize, value))
                {
                    _fileSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSize)));
                }
            }
        }

        /// <summary>
        /// 文件字符数
        /// </summary>
        private string _fileCharacterSize;

        public string FileCharacterSize
        {
            get { return _fileCharacterSize; }

            set
            {
                if (!string.Equals(_fileCharacterSize, value))
                {
                    _fileCharacterSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileCharacterSize)));
                }
            }
        }

        /// <summary>
        /// 文本转语音输出配置
        /// </summary>
        public TextToAudioOutputConfigurationModel TextToAudioOutputConfiguration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
