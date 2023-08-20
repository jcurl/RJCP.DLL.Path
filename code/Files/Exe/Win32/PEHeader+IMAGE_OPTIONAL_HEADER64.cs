namespace RJCP.IO.Files.Exe.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class PEHeader
    {
        public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            #region Standard Fields
            /// <summary>
            /// The unsigned integer that identifies the state of the image file. The most common number is 0x10B, which
            /// identifies it as a normal executable file. 0x107 identifies it as a ROM image, and 0x20B identifies it
            /// as a PE32+ executable.
            /// </summary>
            public ushort Magic;

            /// <summary>
            /// The linker major version number.
            /// </summary>
            public byte MajorLinkerVersion;

            /// <summary>
            /// The linker minor version number.
            /// </summary>
            public byte MinorLinkerVersion;

            /// <summary>
            /// The size of the code (text) section, or the sum of all code sections if there are multiple sections.
            /// </summary>
            public uint SizeOfCode;

            /// <summary>
            /// The size of the initialized data section, or the sum of all such sections if there are multiple data
            /// sections.
            /// </summary>
            public uint SizeOfInitializedData;

            /// <summary>
            /// The size of the uninitialized data section (BSS), or the sum of all such sections if there are multiple
            /// BSS sections.
            /// </summary>
            public uint SizeOfUninitializedData;

            /// <summary>
            /// The address of the entry point relative to the image base when the executable file is loaded into
            /// memory. For program images, this is the starting address. For device drivers, this is the address of the
            /// initialization function. An entry point is optional for DLLs. When no entry point is present, this field
            /// must be zero.
            /// </summary>
            public uint AddressOfEntryPoint;

            /// <summary>
            /// The address that is relative to the image base of the beginning-of-code section when it is loaded into
            /// memory.
            /// </summary>
            public uint BaseOfCode;
            #endregion

            #region Windows Specific Fields
            /// <summary>
            /// The preferred address of the first byte of image when loaded into memory; must be a multiple of 64 K.
            /// The default for DLLs is 0x10000000. The default for Windows CE EXEs is 0x00010000. The default for
            /// Windows NT, Windows 2000, Windows XP, Windows 95, Windows 98, and Windows Me is 0x00400000.
            /// </summary>
            public ulong ImageBase;

            /// <summary>
            /// The alignment (in bytes) of sections when they are loaded into memory. It must be greater than or equal
            /// to FileAlignment. The default is the page size for the architecture.
            /// </summary>
            public uint SectionAlignment;

            /// <summary>
            /// The alignment factor (in bytes) that is used to align the raw data of sections in the image file. The
            /// value should be a power of 2 between 512 and 64 K, inclusive. The default is 512. If the
            /// SectionAlignment is less than the architecture's page size, then FileAlignment must match
            /// SectionAlignment.
            /// </summary>
            public uint FileAlignment;

            /// <summary>
            /// The major version number of the required operating system.
            /// </summary>
            public ushort MajorOperatingSystemVersion;

            /// <summary>
            /// The minor version number of the required operating system.
            /// </summary>
            public ushort MinorOperatingSystemVersion;

            /// <summary>
            /// The major version number of the image.
            /// </summary>
            public ushort MajorImageVersion;

            /// <summary>
            /// The minor version number of the image.
            /// </summary>
            public ushort MinorImageVersion;

            /// <summary>
            /// The major version number of the subsystem.
            /// </summary>
            public ushort MajorSubsystemVersion;

            /// <summary>
            /// The minor version number of the subsystem.
            /// </summary>
            public ushort MinorSubsystemVersion;

            /// <summary>
            /// Reserved, must be zero.
            /// </summary>
            public uint Win32VersionValue;

            /// <summary>
            /// The size (in bytes) of the image, including all headers, as the image is loaded in memory. It must be a
            /// multiple of SectionAlignment.
            /// </summary>
            public uint SizeOfImage;

            /// <summary>
            /// The combined size of an MS-DOS stub, PE header, and section headers rounded up to a multiple of
            /// FileAlignment.
            /// </summary>
            public uint SizeOfHeaders;

            /// <summary>
            /// The image file checksum. The algorithm for computing the checksum is incorporated into IMAGHELP.DLL. The
            /// following are checked for validation at load time: all drivers, any DLL loaded at boot time, and any DLL
            /// that is loaded into a critical Windows process.
            /// </summary>
            public uint CheckSum;

            /// <summary>
            /// The subsystem that is required to run this image.
            /// </summary>
            public IMAGE_SUBSYSTEM Subsystem;

            /// <summary>
            /// The DLL characteristics.
            /// </summary>
            public IMAGE_DLLCHARACTERISTICS DllCharacteristics;

            /// <summary>
            /// The size of the stack to reserve. Only SizeOfStackCommit is committed; the rest is made available one
            /// page at a time until the reserve size is reached.
            /// </summary>
            public ulong SizeOfStackReserve;

            /// <summary>
            /// The size of the stack to commit.
            /// </summary>
            public ulong SizeOfStackCommit;

            /// <summary>
            /// The size of the local heap space to reserve. Only SizeOfHeapCommit is committed; the rest is made
            /// available one page at a time until the reserve size is reached.
            /// </summary>
            public ulong SizeOfHeapReserve;

            /// <summary>
            /// The size of the local heap space to commit.
            /// </summary>
            public ulong SizeOfHeapCommit;

            /// <summary>
            /// Reserved, must be zero.
            /// </summary>
            public uint LoaderFlags;

            /// <summary>
            /// The number of data-directory entries in the remainder of the optional header. Each describes a location
            /// and size.
            /// </summary>
            public uint NumberOfRvaAndSizes;
            #endregion

#if false
            // We don't need these sections, so don't define it and we won't read them in.
            #region Data Directories
            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY Debug;
            public IMAGE_DATA_DIRECTORY Architecture;
            public IMAGE_DATA_DIRECTORY GlobalPtr;
            public IMAGE_DATA_DIRECTORY TLSTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImport;
            public IMAGE_DATA_DIRECTORY IAT;
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
            public IMAGE_DATA_DIRECTORY Reserved;
            #endregion
#endif
        }
    }
}
