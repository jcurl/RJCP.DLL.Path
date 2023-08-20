namespace RJCP.IO.Files.Exe.Win32
{
    using System;

    internal static partial class PEHeader
    {
        /// <summary>
        /// The Characteristics field contains flags that indicate attributes of the object or image file.
        /// </summary>
        [Flags]
        public enum IMAGE_FILE_MachineType : ushort
        {
            /// <summary>
            /// The content of this field is assumed to be applicable to any machine type.
            /// </summary>
            UNKNOWN = 0,

            /// <summary>
            /// Useful for indicating we want to interact with the host and not a WoW guest.
            /// </summary>
            TARGET_HOST = 0x0001,

            /// <summary>
            /// Alpha AXP, 32-bit address space.
            /// </summary>
            ALPHA = 0x0184,

            /// <summary>
            /// Alpha 64, 64-bit address space.
            /// </summary>
            ALPHA64 = 0x0284,

            /// <summary>
            /// Matsushita AM33.
            /// </summary>
            AM33 = 0x01d3,

            /// <summary>
            /// x64.
            /// </summary>
            AMD64 = 0x8664,

            /// <summary>
            /// ARM little endian.
            /// </summary>
            ARM = 0x01c0,

            /// <summary>
            /// ARM64 little endian.
            /// </summary>
            ARM64 = 0xaa64,

            /// <summary>
            /// ARM Thumb-2 Little-Endian.
            /// </summary>
            ARMNT = 0x01c4,

            /// <summary>
            /// AXP 64 (Same as Alpha 64).
            /// </summary>
            AXP64 = 0x0284,

            /// <summary>
            /// CEE.
            /// </summary>
            CEE = 0xC0EE,

            /// <summary>
            /// CEF.
            /// </summary>
            CEF = 0x0cef,

            /// <summary>
            /// EFI byte code.
            /// </summary>
            EBC = 0x0ebc,

            /// <summary>
            /// Intel 386 or later processors and compatible processors.
            /// </summary>
            I386 = 0x014c,

            /// <summary>
            /// Intel Itanium processor family.
            /// </summary>
            IA64 = 0x0200,

            /// <summary>
            /// LoongArch 32-bit processor family.
            /// </summary>
            LOONGARCH32 = 0x6232,

            /// <summary>
            /// LoongArch 64-bit processor family.
            /// </summary>
            LOONGARCH64 = 0x6264,

            /// <summary>
            /// Mitsubishi M32R little endian.
            /// </summary>
            M32R = 0x9041,

            /// <summary>
            /// MIPS16.
            /// </summary>
            MIPS16 = 0x0266,

            /// <summary>
            /// MIPS with FPU.
            /// </summary>
            MIPSFPU = 0x0366,

            /// <summary>
            /// MIPS16 with FPU.
            /// </summary>
            MIPSFPU16 = 0x0466,

            /// <summary>
            /// Power PC little endian.
            /// </summary>
            POWERPC = 0x01f0,

            /// <summary>
            /// Power PC with floating point support.
            /// </summary>
            POWERPCFP = 0x01f1,

            /// <summary>
            /// MIPS little-endian, 0x160 big-endian
            /// </summary>
            R3000 = 0x0162,

            /// <summary>
            /// MIPS little-endian, 0x160 big-endian
            /// </summary>
            R3000BE = 0x0160,

            /// <summary>
            /// MIPS little endian.
            /// </summary>
            R4000 = 0x0166,

            /// <summary>
            /// MIPS little-endian.
            /// </summary>
            R10000 = 0x0168,

            /// <summary>
            /// RISC-V 32-bit address space.
            /// </summary>
            RISCV32 = 0x5032,

            /// <summary>
            /// RISC-V 64-bit address space.
            /// </summary>
            RISCV64 = 0x5064,

            /// <summary>
            /// RISC-V 128-bit address space.
            /// </summary>
            RISCV128 = 0x5128,

            /// <summary>
            /// Hitachi SH3.
            /// </summary>
            SH3 = 0x01a2,

            /// <summary>
            /// Hitachi SH3 DSP.
            /// </summary>
            SH3DSP = 0x01a3,

            /// <summary>
            /// SH3E little-endian.
            /// </summary>
            SH3E = 0x01a4,

            /// <summary>
            /// Hitachi SH4 little-endian.
            /// </summary>
            SH4 = 0x01a6,

            /// <summary>
            /// Hitachi SH5 little-endian.
            /// </summary>
            SH5 = 0x01a8,

            /// <summary>
            /// ARM Thumb/Thumb-2 Little-Endian.
            /// </summary>
            THUMB = 0x01c2,

            /// <summary>
            /// Infineon.
            /// </summary>
            TRICORE = 0x0520,

            /// <summary>
            /// MIPS little-endian WCE v2.
            /// </summary>
            WCEMIPSV2 = 0x0169
        }
    }
}
