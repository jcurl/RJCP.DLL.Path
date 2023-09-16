namespace RJCP.IO.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class NtDll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct IO_STATUS_BLOCK
        {
            public uint Status;
            public ulong Information;
        }
    }
}
