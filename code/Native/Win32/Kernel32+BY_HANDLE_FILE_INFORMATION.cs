﻿namespace RJCP.IO.Native.Win32
{
    using System.Runtime.InteropServices;

    internal partial class Kernel32
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public FileAttributeFlags FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }
    }
}
