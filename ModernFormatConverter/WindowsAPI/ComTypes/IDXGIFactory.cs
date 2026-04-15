using System;
using System.Runtime.InteropServices;

namespace ModernFormatConverter.WindowsAPI.ComTypes
{
    [ComImport, Guid("7B7166EC-21C7-44AE-B21A-C9AE321AE369"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXGIFactory
    {
        /// <summary>
        /// 将应用程序定义的数据设置为 对象，并将该数据与 GUID 相关联。
        /// </summary>
        /// <param name="Name">标识数据的 GUID。 在调用 GetPrivateData 时使用此 GUID 获取数据。</param>
        /// <param name="DataSize">对象数据的大小。</param>
        /// <param name="pData">指向对象数据的指针。</param>
        /// <returns>返回 DXGI_ERROR 值之一。</returns>
        [PreserveSig]
        int SetPrivateData(Guid Name, uint DataSize, nint pData);

        /// <summary>
        /// 在对象的专用数据中设置接口。
        /// </summary>
        /// <param name="Name">标识接口的 GUID。</param>
        /// <param name="pUnknown">指向要与设备对象关联的 IUnknown 派生接口的指针。 其引用计数在设置时递增，当 IDXGIObject 被销毁时或通过调用 SetPrivateData 或 SetPrivateDataInterface 使用相同的 GUID 覆盖数据时递减。</param>
        /// <returns>返回以下 DXGI_ERROR之一。</returns>
        [PreserveSig]
        int SetPrivateDataInterface(Guid Name, nint pUnknown);

        /// <summary>
        /// 获取指向对象数据的指针。
        /// </summary>
        /// <param name="Name">标识数据的 GUID。</param>
        /// <param name="pDataSize">数据的大小。</param>
        /// <param name="pData">指向数据的指针。</param>
        /// <returns>返回以下 DXGI_ERROR之一。</returns>
        [PreserveSig]
        int GetPrivateData(Guid Name, ref uint pDataSize, out nint pData);

        /// <summary>
        /// 获取对象的父级。
        /// </summary>
        /// <param name="riid">所请求接口的 ID。</param>
        /// <param name="ppParent">指向父对象的指针的地址。</param>
        /// <returns>返回 DXGI_ERROR 值之一。</returns>
        [PreserveSig]
        int GetParent(Guid riid, out nint ppParent);

        /// <summary>
        /// 枚举适配器 (视频卡) 。
        /// </summary>
        /// <param name="Adapter">要枚举的适配器的索引。</param>
        /// <param name="ppAdapter">指向适配器参数指定位置的 IDXGIAdapter 接口的指针的地址。 此参数不得为 NULL。</param>
        /// <returns>如果成功，则返回S_OK;否则，如果索引大于或等于本地系统中的适配器数，则返回 DXGI_ERROR_NOT_FOUND;如果 ppAdapter 参数为 NULL，则返回 DXGI_ERROR_INVALID_CALL。</returns>
        [PreserveSig]
        int EnumAdapters(uint Adapter, [MarshalAs(UnmanagedType.Interface)] out IDXGIAdapter ppAdapter);

        /// <summary>
        /// 允许 DXGI 监视应用程序的消息队列中的 alt-enter 键序列 (这会导致应用程序从窗口切换到全屏，反之亦然) 。
        /// </summary>
        /// <param name="WindowHandle">要监视的窗口的句柄。 此参数可以为 NULL;但前提是 Flags 也为 0。</param>
        /// <param name="Flags">多个值。</param>
        /// <returns>DXGI_ERROR_INVALID_CALLWindowHandle 是否无效或E_OUTOFMEMORY。</returns>
        [PreserveSig]
        int MakeWindowAssociation(nint WindowHandle, uint Flags);

        /// <summary>
        /// 获取一个窗口，用户通过该窗口控制全屏切换和切换。
        /// </summary>
        /// <param name="pWindowHandle">指向窗口句柄的指针。</param>
        /// <returns>返回指示成功或失败的代码。 S_OK 指示成功， DXGI_ERROR_INVALID_CALL 指示 pWindowHandle 已作为 NULL 传入。</returns>
        [PreserveSig]
        int GetWindowAssociation(out nint pWindowHandle);

        /// <summary>
        /// 从 Direct3D 11.1 开始，建议不要再使用 CreateSwapChain 来创建交换链。 请改用 CreateSwapChainForHwnd、 CreateSwapChainForCoreWindow 或 CreateSwapChainForComposition ，具体取决于要如何创建交换链。
        /// </summary>
        /// <param name="pDevice">对于 Direct3D 11 和早期版本的 Direct3D，这是指向交换链的 Direct3D 设备的指针。 对于 Direct3D 12，这是指向直接命令队列的指针， (引用 ID3D12CommandQueue) 。 此参数不能为 NULL。</param>
        /// <param name="pDesc">指向交换链说明 DXGI_SWAP_CHAIN_DESC 结构的指针。 此参数不能为 NULL。</param>
        /// <param name="ppSwapChain">指向变量的指针，该变量接收指向 CreateSwapChain 创建的交换链的 IDXGISwapChain 接口的指针。</param>
        /// <returns>DXGI_ERROR_INVALID_CALL 如果 pDesc 或 ppSwapChain 为 NULL，则DXGI_STATUS_OCCLUDED请求全屏模式且不可用，或者E_OUTOFMEMORY。 也可能返回由传入的设备类型定义的其他错误代码。</returns>
        [PreserveSig]
        int CreateSwapChain(nint pDevice, nint pDesc, out nint ppSwapChain);

        /// <summary>
        /// 创建表示软件适配器的适配器接口。
        /// </summary>
        /// <param name="Module">软件适配器的 dll 的句柄。 可以使用 GetModuleHandle 或 LoadLibrary 获取 HMODULE。</param>
        /// <param name="ppAdapter">指向适配器 (指针的地址，请参阅 IDXGIAdapter) 。</param>
        /// <returns>指示成功或失败的返回代码 。</returns>
        [PreserveSig]
        int CreateSoftwareAdapter(nint Module, [MarshalAs(UnmanagedType.Interface)] out IDXGIAdapter ppAdapter);
    }
}
