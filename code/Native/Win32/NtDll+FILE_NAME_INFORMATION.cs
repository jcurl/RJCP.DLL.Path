namespace RJCP.IO.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class NtDll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_NAME_INFORMATION
        {
            public uint FileNameLength;
            public byte PathBuffer;  // First byte to the path
        }
    }
}
