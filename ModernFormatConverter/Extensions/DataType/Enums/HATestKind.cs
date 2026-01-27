namespace ModernFormatConverter.Extensions.DataType.Enums
{
    /// <summary>
    /// 硬件加速测试类型
    /// </summary>
    public enum HATestKind
    {
        H264_QSV = 0,
        HEVC_QSV = 1,
        AV1_QSV = 2,
        VP9_QSV = 3,
        H264_MF = 4,
        HEVC_MF = 5,
        AV1_MF = 6,
        H264_NVENC = 7,
        HEVC_NVENC = 8,
        AV1_NVENC = 9,
        H264_AMF = 10,
        HEVC_AMF = 11,
        AV1_AMF = 12
    }
}
