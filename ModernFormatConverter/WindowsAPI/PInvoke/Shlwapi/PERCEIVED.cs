// 抑制 CA1069 警告
#pragma warning disable CA1069

namespace ModernFormatConverter.WindowsAPI.PInvoke.Shlwapi
{
    /// <summary>
    /// 指定文件的感知类型。 这组常量用于 AssocGetPerceivedType 函数。
    /// </summary>
    public enum PERCEIVED
    {
        /// <summary>
        /// 注册表中定义的文件的感知类型不是已知类型。
        /// </summary>
        PERCEIVED_TYPE_CUSTOM = -3,

        /// <summary>
        /// 该文件没有感知的类型。
        /// </summary>
        PERCEIVED_TYPE_UNSPECIFIED = -2,

        /// <summary>
        /// 未使用。
        /// </summary>
        PERCEIVED_TYPE_FOLDER = -1,

        /// <summary>
        /// 尚未请求文件的感知类型。 这是创建对象时的缓存类型。 该值永远不会由 AssocGetPerceivedType 返回。
        /// </summary>
        PERCEIVED_TYPE_UNKNOWN = 0,

        /// <summary>
        /// 该文件的感知类型为“text”。
        /// </summary>
        PERCEIVED_TYPE_TEXT = 1,

        /// <summary>
        /// 文件的感知类型为“图像”。
        /// </summary>
        PERCEIVED_TYPE_IMAGE = 2,

        /// <summary>
        /// 文件的感知类型为“音频”。
        /// </summary>
        PERCEIVED_TYPE_AUDIO = 3,

        /// <summary>
        /// 文件的感知类型为“视频”。
        /// </summary>
        PERCEIVED_TYPE_VIDEO = 4,

        /// <summary>
        /// 文件的感知类型是“压缩的”。
        /// </summary>
        PERCEIVED_TYPE_COMPRESSED = 5,

        /// <summary>
        /// 文件的感知类型为“文档”。
        /// </summary>
        PERCEIVED_TYPE_DOCUMENT = 6,

        /// <summary>
        /// 该文件的感知类型为“系统”。
        /// </summary>
        PERCEIVED_TYPE_SYSTEM = 7,

        /// <summary>
        /// 该文件的感知类型为“应用”。
        /// </summary>
        PERCEIVED_TYPE_APPLICATION = 8,

        /// <summary>
        /// Windows Vista 及更高版本。 该文件的感知类型为“gamemedia”。
        /// </summary>
        PERCEIVED_TYPE_GAMEMEDIA = 9,

        /// <summary>
        /// Windows Vista 及更高版本。文件的感知类型为“contacts”
        /// </summary>
        PERCEIVED_TYPE_CONTACTS = 10,

        PERCEIVED_TYPE_LAST = 10,
    }
}
