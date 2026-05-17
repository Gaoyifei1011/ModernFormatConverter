using System.Runtime.InteropServices;

#pragma warning disable CA1401

namespace ModernFormatConverter.WindowsAPI.PInvoke.MediaInfo
{
    /// <summary>
    /// MediaInfo.dll 函数库
    /// </summary>
    public static class MediaInfoLibrary
    {
        private const string MediaInfo = "MediaInfo.dll";

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Close", PreserveSig = true, SetLastError = false)]
        public static extern void MediaInfo_Close(nint handle);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Count_Get", PreserveSig = true, SetLastError = false)]
        public static extern int MediaInfo_Count_Get(nint handle, StreamKind streamKind, int streamNumber);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Delete", PreserveSig = true, SetLastError = false)]
        public static extern void MediaInfo_Delete(nint handle);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Get", PreserveSig = true, SetLastError = false)]
        public static extern nint MediaInfo_Get(nint Handle, StreamKind streamKind, int streamNumber, string parameter, InfoKind infoKind, InfoKind searchKind);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Inform", PreserveSig = true, SetLastError = false)]
        public static extern nint MediaInfo_Inform(nint handle, int reserved);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_New", PreserveSig = true, SetLastError = false)]
        public static extern nint MediaInfo_New();

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Open", PreserveSig = true, SetLastError = false)]
        public static extern int MediaInfo_Open(nint handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

        [DllImport(MediaInfo, CharSet = CharSet.Unicode, EntryPoint = "MediaInfo_Option", PreserveSig = true, SetLastError = false)]
        public static extern nint MediaInfo_Option(nint handle, [MarshalAs(UnmanagedType.LPWStr)] string option, [MarshalAs(UnmanagedType.LPWStr)] string value);
    }
}
