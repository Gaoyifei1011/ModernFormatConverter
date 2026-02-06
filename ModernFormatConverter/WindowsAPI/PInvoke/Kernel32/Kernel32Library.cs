using System.Runtime.InteropServices;
using System.Text;

// 抑制 CA1401 警告
#pragma warning disable CA1401

namespace ModernFormatConverter.WindowsAPI.PInvoke.Kernel32
{
    /// <summary>
    /// Kernel32.dll 函数库
    /// </summary>
    public static class Kernel32Library
    {
        private const string Kernel32 = "kernel32.dll";

        public const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

        /// <summary>
        /// 获取当前进程的 应用程序用户模型 ID 。
        /// </summary>
        /// <param name="applicationUserModelIdLength">输入时， applicationUserModelId 缓冲区的大小（以宽字符为单位）。 成功时，使用的缓冲区大小，包括 null 终止符。</param>
        /// <param name="applicationUserModelId">指向接收应用程序用户模型 ID 的缓冲区的指针。</param>
        /// <returns>如果该函数成功，则返回 ERROR_SUCCESS。 否则，该函数将返回错误代码。</returns>
        [DllImport(Kernel32, CharSet = CharSet.Unicode, EntryPoint = "GetCurrentApplicationUserModelId", PreserveSig = true, SetLastError = false)]
        public static extern int GetCurrentApplicationUserModelId(ref uint applicationUserModelIdLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder applicationUserModelId);

        /// <summary>
        /// 获取调用进程的包系列名称。
        /// </summary>
        /// <param name="packageFamilyNameLength">输入时， packageFamilyName 缓冲区的大小（以字符为单位），包括 null 终止符。 输出时，返回的包系列名称的大小（以字符为单位），包括 null 终止符。</param>
        /// <param name="packageFamilyName">包系列名称。</param>
        /// <returns>如果函数成功，则返回 ERROR_SUCCESS。 否则，函数将返回错误代码。</returns>
        [DllImport(Kernel32, CharSet = CharSet.Unicode, EntryPoint = "GetCurrentPackageFamilyName", PreserveSig = true, SetLastError = false)]
        public static extern int GetCurrentPackageFamilyName(ref int packageFamilyNameLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder packageFamilyName);

        /// <summary>
        /// 检索有关指定磁盘的信息，包括磁盘上的可用空间量。
        /// </summary>
        /// <param name="lpRootPathName">要为其返回信息的磁盘的根目录。 如果此参数 NULL，则该函数使用当前磁盘的根目录。 如果此参数是 UNC 名称，则必须包含尾随反斜杠（例如“\\MyServer\MyShare\”）。 此外，驱动器规范必须具有尾随反斜杠（例如，“C：\”）。 调用应用程序必须对此目录具有 FILE_LIST_DIRECTORY 访问权限。</param>
        /// <param name="lpSectorsPerCluster">指向接收每个群集扇区数的变量的指针。</param>
        /// <param name="lpBytesPerSector">指向接收每个扇区字节数的变量的指针。</param>
        /// <param name="lpNumberOfFreeClusters">
        /// 指向一个变量的指针，该变量接收磁盘上可用的可用群集总数，该群集可供与调用线程关联的用户使用。
        /// 如果使用每用户磁盘配额，此值可能小于磁盘上的可用群集总数。
        /// </param>
        /// <param name="lpTotalNumberOfClusters">
        /// 指向一个变量的指针，该变量接收磁盘上可供与调用线程关联的用户使用的群集总数。
        /// 如果使用每用户磁盘配额，此值可能小于磁盘上的群集总数。
        /// </param>
        /// <returns>如果函数成功，则返回值为非零。如果函数失败，则返回值为零。</returns>
        [DllImport(Kernel32, CharSet = CharSet.Unicode, EntryPoint = "GetDiskFreeSpaceW", PreserveSig = true, SetLastError = false)]
        public static extern bool GetDiskFreeSpace([MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName, out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters, out uint lpTotalNumberOfClusters);
    }
}
