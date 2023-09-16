namespace RJCP.IO.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    internal static partial class Kernel32
    {
        /// <summary>
        /// Constant for invalid handle value.
        /// </summary>
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true, EntryPoint = "CreateFileW")]
        public static extern SafeFileHandle CreateFile(string fileName, ACCESS_MASK access, FileShare share,
            IntPtr securityAttributes, CreationDisposition creationDisposition, CreateFileFlags flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetFileInformationByHandleEx(SafeFileHandle hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_ID_INFO fileIdInfo, int dwBufferSize);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION fileInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "GetFinalPathNameByHandleW")]
        public static extern unsafe int GetFinalPathNameByHandle(SafeFileHandle hFile, char* buffer, int bufLen, int flags);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern unsafe bool DeviceIoControl(SafeFileHandle file, int dwIoControlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "QueryDosDeviceA")]
        public static extern unsafe int QueryDosDevice(string lpDeviceName, byte* lpTargetPath, int ucchMax);
    }
}
