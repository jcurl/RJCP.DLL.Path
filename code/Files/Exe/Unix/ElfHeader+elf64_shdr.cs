namespace RJCP.IO.Files.Exe.Unix
{
    using System;
    using System.Runtime.InteropServices;

#if NETSTANDARD
    using System.Buffers.Binary;
#endif

    internal static partial class ElfHeader
    {
        /// <summary>
        /// A file's section header table lets one locate all the file's sections.The section header table is an array
        /// of Elf32_Shdr or Elf64_Shdr structures.The ELF header's e_shoff member gives the byte offset from the
        /// beginning of the file to the section header table.e_shnum holds the number of entries the section header
        /// table contains.e_shentsize holds the size in bytes of each entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct elf64_shdr
        {
            /// <summary>
            /// This member specifies the name of the section. Its value is an index into the section header string
            /// table section, giving the location of a null-terminated string.
            /// </summary>
            public uint sh_name;

            /// <summary>
            /// This member categorizes the section's contents and semantics.
            /// </summary>
            public uint sh_type;

            /// <summary>
            /// Sections support one-bit flags that describe miscellaneous attributes.If a flag bit is set in sh_flags,
            /// the attribute is "on" for the section.Otherwise, the attribute is "off" or does not apply. Undefined
            /// attributes are set to zero.
            /// </summary>
            public ulong sh_flags;

            /// <summary>
            /// If this section appears in the memory image of a process, this member holds the address at which the
            /// section's first byte should reside.Otherwise, the member contains zero.
            /// </summary>
            public ulong sh_addr;

            /// <summary>
            /// This member's value holds the byte offset from the beginning of the file to the first byte in the
            /// section. One section type, SHT_NOBITS, occupies no space in the file, and its sh_offset member locates
            /// the conceptual placement in the file.
            /// </summary>
            public ulong sh_offset;

            /// <summary>
            /// This member holds the section's size in bytes. Unless the section type is SHT_NOBITS, the section
            /// occupies sh_size bytes in the file.A section of type SHT_NOBITS may have a nonzero size, but it occupies
            /// no space in the file.
            /// </summary>
            public ulong sh_size;

            /// <summary>
            /// This member holds a section header table index link, whose interpretation depends on the section type.
            /// </summary>
            public uint sh_link;

            /// <summary>
            /// This member holds extra information, whose interpretation depends on the section type.
            /// </summary>
            public uint sh_info;

            /// <summary>
            /// Some sections have address alignment constraints. If a section holds a doubleword, the system must
            /// ensure doubleword alignment for the entire section.That is, the value of sh_addr must be congruent to
            /// zero, modulo the value of sh_addralign.Only zero and positive integral powers of two are allowed.The
            /// value 0 or 1 means that the section has no alignment constraints.
            /// </summary>
            public ulong sh_addralign;

            /// <summary>
            /// Some sections hold a table of fixed-sized entries, such as a symbol table.For such a section, this
            /// member gives the size in bytes for each entry.This member contains zero if the section does not hold a
            /// table of fixed-size entries.
            /// </summary>
            public ulong sh_entsize;

            internal void FixEndianness(byte ei_data)
            {
                // Only swap if we have to.
                if (BitConverter.IsLittleEndian) {
                    if (ei_data == ELFDATA2LSB) return;
                } else {
                    if (ei_data == ELFDATA2MSB) return;
                }

#if NETSTANDARD
                sh_name = BinaryPrimitives.ReverseEndianness(sh_name);
                sh_type = BinaryPrimitives.ReverseEndianness(sh_type);
                sh_flags = BinaryPrimitives.ReverseEndianness(sh_flags);
                sh_addr = BinaryPrimitives.ReverseEndianness(sh_addr);
                sh_offset = BinaryPrimitives.ReverseEndianness(sh_offset);
                sh_size = BinaryPrimitives.ReverseEndianness(sh_size);
                sh_link = BinaryPrimitives.ReverseEndianness(sh_link);
                sh_info = BinaryPrimitives.ReverseEndianness(sh_info);
                sh_addralign = BinaryPrimitives.ReverseEndianness(sh_addralign);
                sh_entsize = BinaryPrimitives.ReverseEndianness(sh_entsize);
#else
                sh_name = ReverseEndianness(sh_name);
                sh_type = ReverseEndianness(sh_type);
                sh_flags = ReverseEndianness(sh_flags);
                sh_addr = ReverseEndianness(sh_addr);
                sh_offset = ReverseEndianness(sh_offset);
                sh_size = ReverseEndianness(sh_size);
                sh_link = ReverseEndianness(sh_link);
                sh_info = ReverseEndianness(sh_info);
                sh_addralign = ReverseEndianness(sh_addralign);
                sh_entsize = ReverseEndianness(sh_entsize);
#endif
            }
        }
    }
}
