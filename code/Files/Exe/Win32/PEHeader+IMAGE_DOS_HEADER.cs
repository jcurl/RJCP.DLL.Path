namespace RJCP.IO.Files.Exe.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class PEHeader
    {
        /// <summary>
        /// DOS .EXE header
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DOS_HEADER
        {
            /// <summary>
            /// Magic Number.
            /// </summary>
            public ushort e_magic;     // Offset 0

            /// <summary>
            /// Bytes on last page of file.
            /// </summary>
            public ushort e_cblp;      // Offset 2

            /// <summary>
            /// Pages in file.
            /// </summary>
            public ushort e_cp;        // Offset 4

            /// <summary>
            /// Relocations.
            /// </summary>
            public ushort e_crlc;      // Offset 6

            /// <summary>
            /// Size of header in paragraphs.
            /// </summary>
            public ushort e_cparhdr;   // Offset 8

            /// <summary>
            /// Minimum extra paragraphs needed.
            /// </summary>
            public ushort e_minalloc;  // Offset 10

            /// <summary>
            /// Maximum extra paragraphs needed.
            /// </summary>
            public ushort e_maxalloc;  // Offset 12

            /// <summary>
            /// Initial (relative) SS value.
            /// </summary>
            public ushort e_ss;        // Offset 14

            /// <summary>
            /// Initial SP value.
            /// </summary>
            public ushort e_sp;        // Offset 16

            /// <summary>
            /// Checksum.
            /// </summary>
            public ushort e_csum;      // Offset 18

            /// <summary>
            /// Initial IP value.
            /// </summary>
            public ushort e_ip;        // Offset 20

            /// <summary>
            /// Initial (relative) CS value.
            /// </summary>
            public ushort e_cs;        // Offset 22

            /// <summary>
            /// File address of relocation table.
            /// </summary>
            public ushort e_lfarlc;    // Offset 24

            /// <summary>
            /// Overlay number.
            /// </summary>
            public ushort e_ovno;      // Offset 26

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            private ushort[] e_res;     // Offset 28

            /// <summary>
            /// OEM identifier (for e_oeminfo).
            /// </summary>
            public ushort e_oemid;     // Offset 36

            /// <summary>
            /// OEM information; e_oemid specific.
            /// </summary>
            public ushort e_oeminfo;   // Offset 38

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private ushort[] e_res2;    // Offset 40

            /// <summary>
            /// File address of new exe header.
            /// </summary>
            public uint e_lfanew;      // offset 60
        }
    }
}
