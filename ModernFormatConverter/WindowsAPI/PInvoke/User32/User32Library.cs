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
        /// 在用户界面特权隔离 (UIPI) 消息筛选器中添加或删除消息。
        /// </summary>
        /// <param name="msg">要向筛选器添加或从中删除的消息。</param>
        /// <param name="flags">要执行的操作。</param>
        /// <returns>如果成功，则为 TRUE;否则为 FALSE。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "ChangeWindowMessageFilter", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ChangeWindowMessageFilter(WindowMessage message, MSGFLT dwFlag);

        /// <summary>
        /// EnumDisplayMonitors 函数枚举显示监视器 (包括与镜像驱动程序关联的不可见伪监视器) ，这些监视器与指定剪裁矩形的交集与设备上下文的可见区域相交而形成的区域相交。 EnumDisplayMonitors 为枚举的每个监视器调用一次应用程序定义的 MonitorEnumProc 回调函数。 请注意， GetSystemMetrics (SM_CMONITORS) 仅对显示监视器进行计数。
        /// </summary>
        /// <param name="hdc">
        /// 显示设备上下文的句柄，用于定义感兴趣的可见区域。
        /// 如果此参数为 NULL，则传递给回调函数的 hdcMonitor 参数将为 NULL，并且关注的可见区域是包含桌面上所有显示器的虚拟屏幕。
        /// </param>
        /// <param name="lprcClip">
        /// 指向指定剪裁矩形的 RECT 结构的指针。 感兴趣区域是剪裁矩形与 hdc 指定的可见区域的交集。
        /// 如果 hdc 为非 NULL，则剪裁矩形的坐标相对于 hdc 的原点。 如果 hdc 为 NULL，则坐标为虚拟屏幕坐标。
        /// 如果不想剪辑 hdc 指定的区域，此参数可以为 NULL。</param>
        /// <param name="lpfnEnum">指向 MonitorEnumProc 应用程序定义的回调函数的指针。</param>
        /// <param name="dwData">EnumDisplayMonitors 直接传递给 MonitorEnumProc 函数的应用程序定义数据。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。</returns>
        [DllImport(User32, EntryPoint = "EnumDisplayMonitors", SetLastError = false), PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, MONITORENUMPROC lpfnEnum, nint dwData);

        /// <summary>
        /// 返回指定窗口的每英寸点数 (dpi) 值。
        /// </summary>
        /// <param name="hwnd">要获取其相关信息的窗口。</param>
        /// <returns>窗口的 DPI，取决于窗口 DPI_AWARENESS 。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "GetDpiForWindow", PreserveSig = true, SetLastError = false)]
        public static extern int GetDpiForWindow(nint hwnd);

        /// <summary>
        /// 检索指定窗口的显示状态以及还原、最小化和最大化的位置。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="lpwndpl">指向接收显示状态和位置信息的 WINDOWPLACEMENT 结构的指针。 在调用 GetWindowPlacement 之前，请将 length 成员设置为 sizeof(WINDOWPLACEMENT)。 如果未正确设置 lpwndpl-length，GetWindowPlacement > 将失败。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "GetWindowPlacement", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(nint hWnd, ref WINDOWPLACEMENT lpwndpl);

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
        /// 更改指定窗口的属性。 该函数还将指定偏移量处的32位（long类型）值设置到额外的窗口内存中。
        /// </summary>
        /// <param name="hWnd">窗口的句柄，间接地是窗口所属的类。</param>
        /// <param name="nIndex">要设置的值的从零开始的偏移量。 有效值的范围为零到额外窗口内存的字节数，减去整数的大小。</param>
        /// <param name="dwNewLong">替换值。</param>
        /// <returns>如果函数成功，则返回值是指定 32 位整数的上一个值。如果函数失败，则返回值为零。 </returns>
        [DllImport(User32, EntryPoint = "SetWindowLongW", SetLastError = false)]
        public static extern nint SetWindowLong(nint hWnd, WINDOW_LONG_PTR_INDEX nIndex, ulong dwNewLong);

        /// <summary>
        /// 更改指定窗口的属性。 该函数还将指定偏移量处的64位（long类型）值设置到额外的窗口内存中。
        /// </summary>
        /// <param name="hWnd">窗口的句柄，间接地是窗口所属的类。</param>
        /// <param name="nIndex">要设置的值的从零开始的偏移量。 有效值的范围为零到额外窗口内存的字节数，减去整数的大小。</param>
        /// <param name="dwNewLong">替换值。</param>
        /// <returns>如果函数成功，则返回值是指定偏移量的上一个值。如果函数失败，则返回值为零。 </returns>
        [DllImport(User32, EntryPoint = "SetWindowLongPtrW", SetLastError = false)]
        public static extern nint SetWindowLongPtr(nint hWnd, WINDOW_LONG_PTR_INDEX nIndex, ulong dwNewLong);

        /// <summary>
        /// 设置指定窗口的显示状态和还原、最小化和最大化的位置。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="lpwndpl">指向 WINDOWPLACEMENT 结构的指针，该结构指定新的显示状态和窗口位置。在调用 SetWindowPlacement 之前，请将 WINDOWPLACEMENT 结构的长度成员设置为 sizeof (WINDOWPLACEMENT) 。 如果未正确设置长度成员，SetWindowPlacement 将失败。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。</returns>
        [DllImport(User32, CharSet = CharSet.Unicode, EntryPoint = "SetWindowPlacement", PreserveSig = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(nint hWnd, WINDOWPLACEMENT lpwndpl);

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
        public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, SET_WINDOW_POS_FLAGS uFlags);
    }
}
