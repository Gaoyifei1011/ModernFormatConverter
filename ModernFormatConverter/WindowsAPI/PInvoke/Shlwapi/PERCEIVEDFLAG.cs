using System;

namespace ModernFormatConverter.WindowsAPI.PInvoke.Shlwapi
{
    /// <summary>
    /// 感知到的类型信息的源
    /// </summary>
    [Flags]
    public enum PERCEIVEDFLAG : uint
    {
        /// <summary>
        /// PERCEIVED_TYPE_UNSPECIFIED (找不到可感知的类型) 。
        /// </summary>
        PERCEIVEDFLAG_UNDEFINED = 0x0000,

        /// <summary>
        /// 感知的类型是通过注册表中的关联确定的。
        /// </summary>
        PERCEIVEDFLAG_SOFTCODED = 0x0001,

        /// <summary>
        /// Windows 固有已知感知的类型。
        /// </summary>
        PERCEIVEDFLAG_HARDCODED = 0x0002,

        /// <summary>
        /// 感知到的类型是通过 Windows 提供的编解码器确定的。
        /// </summary>
        PERCEIVEDFLAG_NATIVESUPPORT = 0x0004,

        /// <summary>
        /// GDI+ 库支持感知的类型。
        /// </summary>
        PERCEIVEDFLAG_GDIPLUS = 0x0010,

        /// <summary>
        /// Windows Media SDK 支持感知的类型。
        /// </summary>
        PERCEIVEDFLAG_WMSDK = 0x0020,

        /// <summary>
        /// Windows 压缩文件夹支持感知的类型。
        /// </summary>
        PERCEIVEDFLAG_ZIPFOLDER = 0x0040,
    }
}
