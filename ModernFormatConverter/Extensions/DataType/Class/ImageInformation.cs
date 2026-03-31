using System.Collections.Generic;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 图片信息
    /// </summary>
    public class ImageInformation
    {
        public GeneralInfo GeneralInfo { get; set; }

        public List<ImageDetailInfo> ImageDetailInfoList { get; set; } = [];


        public string ImageOverviewInformation { get; set; }
    }
}
