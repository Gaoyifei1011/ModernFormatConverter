using System.Collections.Generic;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 视频信息类型
    /// </summary>
    public class VideoInformation
    {
        public GeneralInfo GeneralInfo { get; set; }

        public List<VideoDetailInfo> VideoDetailInfoList { get; } = [];

        public List<AudioDetailInfo> AudioDetailInfoList { get; } = [];

        public List<TextDetailInfo> TextDetailInfoList { get; } = [];

        public string VideoOverviewInformation { get; set; }
    }
}
