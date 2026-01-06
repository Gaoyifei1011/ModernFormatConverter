using System.Drawing;
using System.Runtime.InteropServices;

// 抑制 CA1401 警告
#pragma warning disable CA1401

namespace ModernFormatConverter.WindowsAPI.PInvoke.User32
{
    /// <summary>
    /// User32.dll 函数库
    /// </summary>
    public static class User32Library
    {
        private const string User32 = "user32.dll";

        /// <summary>
        /// 返回指定窗口的每英寸点数 (dpi) 值。
        /// </summary>
        /// <param name="hwnd">要获取其相关信息的窗口。</param>
        /// <returns>窗口的 DPI，取决于窗口 DPI_AWARENESS 。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "GetDpiForWindow", PreserveSig = true, SetLastError = false)]
        public static extern int GetDpiForWindow(nint hwnd);

        /// <summary>
        /// 检索指定窗口的边框的尺寸。 尺寸以相对于屏幕左上角的屏幕坐标提供。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="lpRect">指向 RECT 结构的指针，该结构接收窗口左上角和右下角的屏幕坐标。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "GetWindowRect", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        /// <summary>
        /// MapWindowPoints 函数将 (映射) 一组点从相对于一个窗口的坐标空间转换为相对于另一个窗口的坐标空间。
        /// </summary>
        /// <param name="hWndFrom">从中转换点的窗口的句柄。 如果此参数为 NULL 或HWND_DESKTOP，则假定这些点位于屏幕坐标中。</param>
        /// <param name="hWndTo">指向要向其转换点的窗口的句柄。 如果此参数为 NULL 或HWND_DESKTOP，则点将转换为屏幕坐标。</param>
        /// <param name="lpPoints">指向 POINT 结构的数组的指针，该数组包含要转换的点集。 这些点以设备单位为单位。 此参数还可以指向 RECT 结构，在这种情况下， cPoints 参数应设置为 2。</param>
        /// <param name="cPoints">lpPoints 参数指向的数组中的 POINT 结构数。</param>
        /// <returns>如果函数成功，则返回值的低序字是添加到每个源点的水平坐标以计算每个目标点的水平坐标的像素数。 (除此之外，如果正对 hWndFrom 和 hWndTo 之一进行镜像，则每个生成的水平坐标乘以 -1.) 高序字是添加到每个源点垂直坐标的像素数，以便计算每个目标点的垂直坐标。
        /// 如果函数失败，则返回值为零。 在调用此方法之前调用 SetLastError ，以将错误返回值与合法的“0”返回值区分开来。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "MapWindowPoints", PreserveSig = true, SetLastError = false)]
        public static extern int MapWindowPoints(nint hWndFrom, nint hWndTo, ref Point lpPoints, uint cPoints);

        /// <summary>
        /// 将指定的消息发送到窗口或窗口。SendMessage 函数调用指定窗口的窗口过程，在窗口过程处理消息之前不会返回。
        /// </summary>
        /// <param name="hWnd">
        /// 窗口过程的句柄将接收消息。 如果此参数 HWND_BROADCAST ( (HWND) 0xffff) ，则会将消息发送到系统中的所有顶级窗口，
        /// 包括已禁用或不可见的未所有者窗口、重叠窗口和弹出窗口;但消息不会发送到子窗口。消息发送受 UIPI 的约束。
        /// 进程的线程只能将消息发送到较低或等于完整性级别的线程的消息队列。
        /// </param>
        /// <param name="wMsg">要发送的消息。</param>
        /// <param name="wParam">其他的消息特定信息。</param>
        /// <param name="lParam">其他的消息特定信息。</param>
        /// <returns>返回值指定消息处理的结果;这取决于发送的消息。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", PreserveSig = true, SetLastError = false)]
        public static extern nint SendMessage(nint hWnd, WindowMessage wMsg, nuint wParam, nint lParam);

        /// <summary>
        /// 当仅针对当前正在运行的进程创建没有父级或所有者的窗口时，更改默认布局。
        /// </summary>
        /// <param name="dwDefaultLayout">默认进程布局。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "SetProcessDefaultLayout", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDefaultLayout(uint dwDefaultLayout);

        /// <summary>
        /// 更改子窗口、弹出窗口或顶级窗口的大小、位置和 Z 顺序。 这些窗口根据屏幕上的外观进行排序。 最上面的窗口接收最高排名，是 Z 顺序中的第一个窗口。
        /// </summary>
        /// <param name="hWnd">更改子窗口、弹出窗口或顶级窗口的大小、位置和 Z 顺序。 这些窗口根据屏幕上的外观进行排序。 最上面的窗口接收最高排名，是 Z 顺序中的第一个窗口。</param>
        /// <param name="hWndInsertAfter">在 Z 顺序中定位窗口之前窗口的句柄。 </param>
        /// <param name="X">以客户端坐标表示的窗口左侧的新位置。 </param>
        /// <param name="Y">窗口顶部的新位置，以客户端坐标表示。</param>
        /// <param name="cx">窗口的新宽度（以像素为单位）。</param>
        /// <param name="cy">窗口的新高度（以像素为单位）。</param>
        /// <param name="uFlags">窗口大小调整和定位标志。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。 </returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "SetWindowPos", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
    }
}
