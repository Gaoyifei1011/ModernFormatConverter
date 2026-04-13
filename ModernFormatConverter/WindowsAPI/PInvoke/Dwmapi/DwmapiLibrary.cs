using System.Runtime.InteropServices;

namespace ModernFormatConverter.WindowsAPI.PInvoke.Dwmapi
{
    /// <summary>
    /// Dwmapi.dll 函数库
    /// </summary>
    public static class DwmapiLibrary
    {
        private const string Dwmapi = "dwmapi.dll";

        /// <summary>
        /// 设置窗口管理器（DWM）非客户端呈现属性的值。 有关编程指南和代码示例，请参阅 控制非客户端区域呈现。
        /// </summary>
        /// <param name="hwnd">要为其设置属性值的窗口的句柄。</param>
        /// <param name="dwAttribute">描述要设置的值的标志，指定为 DWMWINDOWATTRIBUTE 枚举的值。 此参数指定要设置的属性，pvAttribute 参数指向包含属性值的对象。</param>
        /// <param name="pvAttribute">指向包含要设置的属性值的对象的指针。 值集的类型取决于 dwAttribute 参数的值。 DWMWINDOWATTRIBUTE 枚举主题指示，在每个标志的行中，应向 pvAttribute 参数传递指向的值类型。</param>
        /// <param name="cbAttribute"></param>
        /// <returns></returns>
        [DllImport(Dwmapi, CharSet = CharSet.Unicode, EntryPoint = "DwmSetWindowAttribute", PreserveSig = true, SetLastError = false)]
        public static extern int DwmSetWindowAttribute(nint hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);
    }
}
