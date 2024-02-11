namespace RJCP.IO.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    [SupportedOSPlatform("windows")]
    internal static partial class NtDll
    {
        [DllImport("ntdll.dll", ExactSpelling = true)]
        public static extern int NtQueryObject(SafeHandle ObjectHandle, OBJECT_INFORMATION_CLASS ObjectInformationClass,
            IntPtr ObjectInformation, int ObjectInformationLength, out int returnLength);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        public static extern int NtQueryInformationFile(SafeFileHandle fileHandle, out IO_STATUS_BLOCK IoStatusBlock,
            IntPtr fileInformation, int length, FILE_INFORMATION_CLASS fileInformationClass);
    }
}
