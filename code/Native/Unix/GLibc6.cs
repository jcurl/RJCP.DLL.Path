namespace RJCP.IO.Native.Unix
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
#if NETFRAMEWORK
    using System.Runtime.ConstrainedExecution;
#endif

    [SuppressUnmanagedCodeSecurity]
    [SupportedOSPlatform("linux")]
    [SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "UTF-8 should be used, not wide strings")]
    internal static partial class GLibc6
    {
        [DllImport("libc.so.6", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern unsafe int readlink(string pathname, byte* buffer, int bufLen);

        [DllImport("libc.so.6", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern SafeMallocHandle realpath(string pathname, IntPtr buffer);

#if NETFRAMEWORK
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        [DllImport("libc.so.6")]
        public static extern void free(IntPtr buffer);
    }
}
