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
        /// The dynamic section contains a series of structures that hold relevant dynamic linking information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct elf32_dyn
        {
            /// <summary>
            /// The tag for the data on how to interpret.
            /// </summary>
            public int d_tag;

            /// <summary>
            /// The value or address for the tag.
            /// </summary>
            public uint d_val;

            internal void FixEndianness(byte ei_data)
            {
                // Only swap if we have to.
                if (BitConverter.IsLittleEndian) {
                    if (ei_data == ELFDATA2LSB) return;
                } else {
                    if (ei_data == ELFDATA2MSB) return;
                }

#if NET6_0_OR_GREATER
                d_tag = BinaryPrimitives.ReverseEndianness(d_tag);
                d_val = BinaryPrimitives.ReverseEndianness(d_val);
#else
                d_tag = ReverseEndianness(d_tag);
                d_val = ReverseEndianness(d_val);
#endif
            }
        }
    }
}
