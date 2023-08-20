namespace RJCP.IO.Files.Exe.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class PEHeader
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }
    }
}
