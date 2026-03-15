using System.Collections.Generic;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 视频信息类型
    /// </summary>
    public class VideoInformation
    {
        public GeneralInfo GeneralInfo { get; set; }

        public List<VideoDetailInfo> VideoInfoList { get; } = [];

        public List<AudioDetailInfo> AudioInfoList { get; } = [];

        public List<TextDetailInfo> TextInfoList { get; } = [];

        public string VideoAllInformation { get; set; }
    }
}
