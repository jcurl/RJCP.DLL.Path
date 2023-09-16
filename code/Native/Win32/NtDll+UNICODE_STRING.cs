namespace RJCP.IO.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;

    internal static partial class NtDll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }
    }
}
