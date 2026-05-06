using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 视频混流所有数据模型（包含视频、音频、字幕部分）
    /// </summary>
    public class VideoMixedFlowModel : INotifyPropertyChanged
    {
        private bool _isVideoFileExisted;

        public bool IsVideoFileExisted
        {
            get { return _isVideoFileExisted; }

            set
            {
                if (!Equals(_isVideoFileExisted, value))
                {
                    _isVideoFileExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVideoFileExisted)));
                }
            }
        }

        /// <summary>
        /// 视频文件部分
        /// </summary>
        private VideoMixedFlowFileModel _videoFile;

        public VideoMixedFlowFileModel VideoFile
        {
            get { return _videoFile; }

            set
            {
                if (!Equals(_videoFile, value))
                {
                    _videoFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoFile)));
                }
            }
        }

        private bool _isAudioFileExisted;

        public bool IsAudioFileExisted
        {
            get { return _isAudioFileExisted; }

            set
            {
                if (!Equals(_isAudioFileExisted, value))
                {
                    _isAudioFileExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAudioFileExisted)));
                }
            }
        }

        /// <summary>
        /// 音频文件部分
        /// </summary>
        private VideoMixedFlowFileModel _audioFile;

        public VideoMixedFlowFileModel AudioFile
        {
            get { return _audioFile; }

            set
            {
                if (!Equals(_audioFile, value))
                {
                    _audioFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioFile)));
                }
            }
        }

        private bool _isSubtitleFileExisted;

        public bool IsSubtitleFileExisted
        {
            get { return _isSubtitleFileExisted; }

            set
            {
                if (!Equals(_isSubtitleFileExisted, value))
                {
                    _isSubtitleFileExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSubtitleFileExisted)));
                }
            }
        }

        /// <summary>
        /// 字幕文件部分
        /// </summary>
        private VideoMixedFlowFileModel _subtitleFile;

        public VideoMixedFlowFileModel SubtitleFile
        {
            get { return _subtitleFile; }

            set
            {
                if (!Equals(_subtitleFile, value))
                {
                    _subtitleFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubtitleFile)));
                }
            }
        }

        public VideoConversionConfigurationModel VideoConversionConfiguration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
