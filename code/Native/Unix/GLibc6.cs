namespace RJCP.IO.Native.Unix
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    [SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "UTF-8 should be used, not wide strings")]
    internal static partial class GLibc6
    {
        [DllImport("libc.so.6", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern unsafe int readlink(string pathname, byte* buffer, int bufLen);
    }
}
