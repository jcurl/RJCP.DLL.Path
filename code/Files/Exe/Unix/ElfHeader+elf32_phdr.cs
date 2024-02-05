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
        /// 32-bit ELF Program Header.
        /// </summary>
        /// <remarks>
        /// An executable or shared object file's program header table is an array of structures, each describing a
        /// segment or other information the system needs to prepare the program for execution.An object file segment
        /// contains one or more sections. Program headers are meaningful only for executable and shared object files. A
        /// file specifies its own program header size with the ELF header's e_phentsize and e_phnum members. The ELF
        /// program header is described by the type Elf32_Phdr or Elf64_Phdr depending on the architecture
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct elf32_phdr
        {
            /// <summary>
            /// This member of the structure indicates what kind of segment this array element describes or how to
            /// interpret the array element's information.
            /// </summary>
            public uint p_type;

            /// <summary>
            /// This member holds the offset from the beginning of the file at which the first byte of the segment
            /// resides.
            /// </summary>
            public uint p_offset;

            /// <summary>
            /// This member holds the virtual address at which the first byte of the segment resides in memory.
            /// </summary>
            public uint p_vaddr;

            /// <summary>
            /// On systems for which physical addressing is relevant, this member is reserved for the segment's physical
            /// address. Under BSD this member is not used and must be zero.
            /// </summary>
            public uint p_paddr;

            /// <summary>
            /// This member holds the number of bytes in the file image of the segment. It may be zero.
            /// </summary>
            public uint p_filesz;

            /// <summary>
            /// This member holds the number of bytes in the memory image of the segment. It may be zero.
            /// </summary>
            public uint p_memsz;

            /// <summary>
            /// This member holds a bit mask of flags relevant to the segment.
            /// </summary>
            public uint p_flags;

            /// <summary>
            /// This member holds the value to which the segments are aligned in memory and in the file.Loadable process
            /// segments must have congruent values for p_vaddr and p_offset, modulo the page size. Values of zero and
            /// one mean no alignment is required.Otherwise, p_align should be a positive, integral power of two, and
            /// p_vaddr should equal p_offset, modulo p_align.
            /// </summary>
            public uint p_align;

            internal void FixEndianness(byte ei_data)
            {
                // Only swap if we have to.
                if (BitConverter.IsLittleEndian) {
                    if (ei_data == ELFDATA2LSB) return;
                } else {
                    if (ei_data == ELFDATA2MSB) return;
                }

#if NET6_0_OR_GREATER
                p_type = BinaryPrimitives.ReverseEndianness(p_type);
                p_offset = BinaryPrimitives.ReverseEndianness(p_offset);
                p_vaddr = BinaryPrimitives.ReverseEndianness(p_vaddr);
                p_paddr = BinaryPrimitives.ReverseEndianness(p_paddr);
                p_filesz = BinaryPrimitives.ReverseEndianness(p_filesz);
                p_memsz = BinaryPrimitives.ReverseEndianness(p_memsz);
                p_flags = BinaryPrimitives.ReverseEndianness(p_flags);
                p_align = BinaryPrimitives.ReverseEndianness(p_align);
#else
                p_type = ReverseEndianness(p_type);
                p_offset = ReverseEndianness(p_offset);
                p_vaddr = ReverseEndianness(p_vaddr);
                p_paddr = ReverseEndianness(p_paddr);
                p_filesz = ReverseEndianness(p_filesz);
                p_memsz = ReverseEndianness(p_memsz);
                p_flags = ReverseEndianness(p_flags);
                p_align = ReverseEndianness(p_align);
#endif
            }
        }
    }
}
