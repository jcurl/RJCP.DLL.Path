namespace RJCP.IO.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        public const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
        public const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;

        public const uint SYMLINK_FLAG_RELATIVE = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct REPARSE_DATA_BUFFER_Generic
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by: (4 * sizeof(ushort)) +
            /// SubstituteNameLength + PrintNameLength + (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;
            public ushort Reserved;

            /// <summary>
            /// Space filler, to be the same size as the other structures.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4000)]
            public byte[] PathBuffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct REPARSE_DATA_BUFFER_SymbolicLink
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by: (4 * sizeof(ushort)) +
            /// SubstituteNameLength + PrintNameLength + (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;
            public ushort Reserved;

            /// <summary>
            /// Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            /// Length, in bytes, of the substitute name string. If this string is null-terminated, SubstituteNameLength
            /// does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            /// Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            /// Length, in bytes, of the print name string. If this string is null-terminated, PrintNameLength does not
            /// include space for the null character.
            /// </summary>
            public ushort PrintNameLength;

            public uint Flags;

            /// <summary>
            /// A buffer containing the unicode-encoded path string. The path string contains the substitute name string
            /// and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct REPARSE_DATA_BUFFER_Junction
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by: (4 * sizeof(ushort)) +
            /// SubstituteNameLength + PrintNameLength + (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;
            public ushort Reserved;

            /// <summary>
            /// Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            /// Length, in bytes, of the substitute name string. If this string is null-terminated, SubstituteNameLength
            /// does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            /// Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            /// Length, in bytes, of the print name string. If this string is null-terminated, PrintNameLength does not
            /// include space for the null character.
            /// </summary>
            public ushort PrintNameLength;

            /// <summary>
            /// A buffer containing the unicode-encoded path string. The path string contains the substitute name string
            /// and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }
    }
}
