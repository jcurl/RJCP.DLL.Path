namespace RJCP.IO.Files.Exe.Unix
{
    using System;
    using System.Runtime.InteropServices;

#if NET6_0_OR_GREATER
    using System.Buffers.Binary;
#endif

    internal static partial class ElfHeader
    {
        /// <summary>
        /// 32-bit ELF Header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct elf64_hdr
        {
            /// <summary>
            /// This array of bytes specifies how to interpret the file, independent of the processor or the file's
            /// remaining contents.Within this array everything is named by macros, which start with the prefix EI_ and
            /// may contain values which start with the prefix ELF.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = EI_NIDENT)]
            public byte[] e_ident;

            /// <summary>
            /// This member of the structure identifies the object file type.
            /// </summary>
            public ushort e_type;

            /// <summary>
            /// This member specifies the required architecture for an individual file.
            /// </summary>
            public ushort e_machine;

            /// <summary>
            /// This member identifies the file version.
            /// </summary>
            public uint e_version;

            /// <summary>
            /// This member gives the virtual address to which the system first transfers control, thus starting the
            /// process.If the file has no associated entry point, this member holds zero.
            /// </summary>
            public ulong e_entry;

            /// <summary>
            /// This member holds the program header table's file offset in bytes.If the file has no program header
            /// table, this member holds zero.
            /// </summary>
            public ulong e_phoff;

            /// <summary>
            /// This member holds the section header table's file offset in bytes.If the file has no section header
            /// table, this member holds zero.
            /// </summary>
            public ulong e_shoff;

            /// <summary>
            /// This member holds processor-specific flags associated with the file.Flag names take the form
            /// EF_`machine_flag'. Currently, no flags have been defined.
            /// </summary>
            public uint e_flags;

            /// <summary>
            /// This member holds the ELF header's size in bytes.
            /// </summary>
            public ushort e_ehsize;

            /// <summary>
            /// This member holds the size in bytes of one entry in the file's program header table; all entries are the
            /// same size.
            /// </summary>
            public ushort e_phentsize;

            /// <summary>
            /// This member holds the number of entries in the program header table. Thus the product of e_phentsize and
            /// e_phnum gives the table's size in bytes. If a file has no program header, e_phnum holds the value zero.
            /// </summary>
            public ushort e_phnum;

            /// <summary>
            /// This member holds a sections header's size in bytes. A section header is one entry in the section header
            /// table; all entries are the same size.
            /// </summary>
            public ushort e_shentsize;

            /// <summary>
            /// This member holds the number of entries in the section header table. Thus the product of e_shentsize and
            /// e_shnum gives the section header table's size in bytes. If a file has no section header table, e_shnum
            /// holds the value of zero. If the number of entries in the section header table is larger than or equal to
            /// SHN_LORESERVE (0xff00), e_shnum holds the value zero and the real number of entries in the section
            /// header table is held in the sh_size member of the initial entry in section header table. Otherwise, the
            /// sh_size member of the initial entry in the section header table holds the value zero.
            /// </summary>
            public ushort e_shnum;

            /// <summary>
            /// This member holds the section header table index of the entry associated with the section name string
            /// table. If the file has no section name string table, this member holds the value SHN_UNDEF. If the index
            /// of section name string table section is larger than or equal to SHN_LORESERVE (0xff00), this member
            /// holds SHN_XINDEX (0xffff) and the real index of the section name string table section is held in the
            /// sh_link member of the initial entry in section header table. Otherwise, the sh_link member of the
            /// initial entry in section header table contains the value zero.
            /// </summary>
            public ushort e_shstrndx;

            internal void FixEndianness()
            {
                // Only swap if we have to.
                if (BitConverter.IsLittleEndian) {
                    if (e_ident[EI_DATA] == ELFDATA2LSB) return;
                } else {
                    if (e_ident[EI_DATA] == ELFDATA2MSB) return;
                }

#if NET6_0_OR_GREATER
                e_type = BinaryPrimitives.ReverseEndianness(e_type);
                e_machine = BinaryPrimitives.ReverseEndianness(e_machine);
                e_version = BinaryPrimitives.ReverseEndianness(e_version);
                e_entry = BinaryPrimitives.ReverseEndianness(e_entry);
                e_phoff = BinaryPrimitives.ReverseEndianness(e_phoff);
                e_shoff = BinaryPrimitives.ReverseEndianness(e_shoff);
                e_flags = BinaryPrimitives.ReverseEndianness(e_flags);
                e_ehsize = BinaryPrimitives.ReverseEndianness(e_ehsize);
                e_phentsize = BinaryPrimitives.ReverseEndianness(e_phentsize);
                e_phnum = BinaryPrimitives.ReverseEndianness(e_phnum);
                e_shentsize = BinaryPrimitives.ReverseEndianness(e_shentsize);
                e_shnum = BinaryPrimitives.ReverseEndianness(e_shnum);
                e_shstrndx = BinaryPrimitives.ReverseEndianness(e_shstrndx);
#else
                e_type = ReverseEndianness(e_type);
                e_machine = ReverseEndianness(e_machine);
                e_version = ReverseEndianness(e_version);
                e_entry = ReverseEndianness(e_entry);
                e_phoff = ReverseEndianness(e_phoff);
                e_shoff = ReverseEndianness(e_shoff);
                e_flags = ReverseEndianness(e_flags);
                e_ehsize = ReverseEndianness(e_ehsize);
                e_phentsize = ReverseEndianness(e_phentsize);
                e_phnum = ReverseEndianness(e_phnum);
                e_shentsize = ReverseEndianness(e_shentsize);
                e_shnum = ReverseEndianness(e_shnum);
                e_shstrndx = ReverseEndianness(e_shstrndx);
#endif
            }
        }
    }
}
