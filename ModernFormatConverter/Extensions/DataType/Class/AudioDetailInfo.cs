namespace ModernFormatConverter.Extensions.DataType.Class
{
    /// <summary>
    /// 音频详细信息
    /// </summary>
    public class AudioDetailInfo
    {
        public string ID { get; }

        public string Format { get; set; }

        public string FormatInfo { get; set; }

        public string CodecID { get; set; }

        public string Duration { get; set; }

        public string BitRateMode { get; set; }

        public string BitRate { get; set; }

        public string MaximumBitRate { get; set; }

        public string Channel { get; set; }

        public string ChannelLayout { get; set; }

        public string SamplingRate { get; set; }

        public string FrameRate { get; set; }

        public string CompressionMode { get; set; }

        public string StreamSize { get; set; }

        public string Default { get; set; }

        public string AlternateGroup { get; set; }
    }
}
