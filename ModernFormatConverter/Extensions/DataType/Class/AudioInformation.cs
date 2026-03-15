using System.Collections.Generic;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 音频信息
    /// </summary>
    public class AudioInformation
    {
        public GeneralInfo GeneralInfo { get; set; }

        public List<AudioDetailInfo> AudioInfoList { get; } = [];

        public string AudioAllInformation { get; set; }
    }
}
