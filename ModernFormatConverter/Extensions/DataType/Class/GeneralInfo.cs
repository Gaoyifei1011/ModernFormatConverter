namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 通用信息
    /// </summary>
    public class GeneralInfo
    {
        public string CompleteName { get; set; }

        public string Format { get; set; }

        public string FormatVersion { get; set; }

        public string FormatProfile { get; set; }

        public string CodecID { get; set; }

        public string FileSize { get; set; }

        public string UniqueID { get; set; }

        public string EncodedDate { get; set; }

        public string Duration { get; set; }

        public string OverallBitRate { get; set; }

        public string FrameRate { get; set; }

        public string StreamSize { get; set; }

        public string RecordedDate { get; set; }

        public string EncodedApplication { get; set; }

        public string EncodedLibrary { get; set; }

        #region 音频专用信息

        public string Album { get; set; }

        public string TrackName { get; set; }

        public string Performer { get; set; }

        #endregion 音频专用信息
    }
}
