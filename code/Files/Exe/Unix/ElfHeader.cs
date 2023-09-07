namespace RJCP.IO.Files.Exe.Unix
{
    using System.Diagnostics.CodeAnalysis;

    internal static partial class ElfHeader
    {
        // Identifiers obtained by `readelf` source code, downloaded from:
        //
        //   git clone git://sourceware.org/git/binutils-gdb.git
        //
        //   binutils/readelf.c
        //   elfcpp/elfcpp.h
        //   include/elf/common.h
        //
        // Also from
        //
        //   https://github.com/file/file/blob/445f387/src/readelf.h

        public const int EI_NIDENT = 16;

        // Offsets into the ELF header file `e_ident`
        public const byte EI_MAG0 = 0x00;
        public const byte EI_MAG1 = 0x01;
        public const byte EI_MAG2 = 0x02;
        public const byte EI_MAG3 = 0x03;
        public const byte EI_CLASS = 0x04;
        public const byte EI_DATA = 0x05;
        public const byte EI_VERSION = 0x06;
        public const byte EI_OSABI = 0x07;               // Not in the standard. See https://refspecs.linuxfoundation.org/elf/elf.pdf
        public const byte EI_ABIVERSION = 0x08;
        public const byte EI_PAD = 0x09;

        // Magic Value for `EI_MAGx` for an ELF file.
        public const byte EI_MAG0_VALUE = 0x7f;
        public const byte EI_MAG1_VALUE = 0x45;
        public const byte EI_MAG2_VALUE = 0x4C;
        public const byte EI_MAG3_VALUE = 0x46;

        // Valid values for e_ident[EI_CLASS]
        public const byte ELFCLASSNONE = 0;
        public const byte ELFCLASS32 = 1;
        public const byte ELFCLASS64 = 2;

        // Valid values for e_ident[EI_DATA].
        public const byte ELFDATANONE = 0;
        public const byte ELFDATA2LSB = 1;
        public const byte ELFDATA2MSB = 2;

        // Valid values for e_ident[EI_VERSION] and e_version.
        public const byte EV_NONE = 0;
        public const byte EV_CURRENT = 1;

        // Valid values for e_ident[EI_OSABI].
        public const byte ELFOSABI_SYSV = 0;             // UNIX System V ABI
        public const byte ELFOSABI_HPUX = 1;             // HP-UX operating system
        public const byte ELFOSABI_NETBSD = 2;           // NetBSD
        public const byte ELFOSABI_LINUX = 3;            // GNU
        public const byte ELFOSABI_HURD = 4;             // GNU-HURD
        public const byte ELFOSABI_86OPEN = 5;           // 86Open Common IA32
        public const byte ELFOSABI_SOLARIS = 6;          // Solaris
        public const byte ELFOSABI_AIX = 7;              // AIX
        public const byte ELFOSABI_IRIX = 8;             // IRIX
        public const byte ELFOSABI_FREEBSD = 9;          // FreeBSD
        public const byte ELFOSABI_TRU64 = 10;           // TRU64 UNIX
        public const byte ELFOSABI_MODESTO = 11;         // Novell Modesto
        public const byte ELFOSABI_OPENBSD = 12;         // OpenBSD
        public const byte ELFOSABI_OPENVMS = 13;         // OpenVMS
        public const byte ELFOSABI_NSK = 14;             // Hewlett-Packard Non-Stop Kernel
        public const byte ELFOSABI_AROS = 15;            // AROS
        public const byte ELFOSABI_FENIXOS = 16;         // FenixOS
        public const byte ELFOSABI_CLOUDABI = 17;        // Nuxi CloudABI
        public const byte ELFOSABI_OPENVOS = 18;         // Stratus Technologies OpenVOS
        public const byte ELFOSABI_C6000_ELFABI = 64;    // Bare-metal TMS320C6000
        public const byte ELFOSABI_AMDGPU_HSA = 64;      // AMD HSA Runtime
        public const byte ELFOSABI_C6000_LINUX = 65;     // Linux TMS320C6000
        public const byte ELFOSABI_AMDGPU_PAL = 65;      // AMD PAL Runtime
        public const byte ELFOSABI_ARM_FDPIC = 65;       // ARM FDPIC
        public const byte ELFOSABI_AMDGPU_MESA3D = 66;   // AMD Mesa3D Runtime
        public const byte ELFOSABI_ARM = 97;             // GNU extension for ARM
        public const byte ELFOSABI_STANDALONE = 255;	 // Standalone (embedded) application

        // Valid values for the e_type field.
        public const byte ET_NONE = 0;                   // No file type
        public const byte ET_REL = 1;                    // Relocatable file
        public const byte ET_EXEC = 2;                   // Executable file
        public const byte ET_DYN = 3;                    // Shared object file
        public const byte ET_CORE = 4;                   // Core file

        // e_machine
        public const ushort EM_NONE = 0;                 // No machine
        public const ushort EM_M32 = 1;                  // AT&T WE 32100
        public const ushort EM_SPARC = 2;                // SUN SPARC
        public const ushort EM_386 = 3;                  // Intel 80386
        public const ushort EM_68K = 4;                  // Motorola 68000
        public const ushort EM_88K = 5;                  // Motorola 88000
        public const ushort EM_IAMCU = 6;                // Intel MCU
        public const ushort EM_860 = 7;                  // Intel 80860
        public const ushort EM_MIPS = 8;                 // MIPS RS3000 (officially, big-endian only)
        public const ushort EM_S370 = 9;                 // IBM System/370
        public const ushort EM_MIPS_RS4_BE = 10;         // MIPS RS4000 big-endian
        public const ushort EM_OLD_SPARCV9 = 11;         // Old version of Sparc v9, from before the ABI. Deprecated.
        public const ushort EM_PARISC = 15;              // HPPA
        public const ushort EM_PPC_OLD = 17;             // Old version of PowerPC. Deprecated.
        public const ushort EM_VPP550 = 17;              // Fujitsu VPP500
        public const ushort EM_SPARC32PLUS = 18;         // Sun's "v8plus"
        public const ushort EM_960 = 19;	             // Intel 80960
        public const ushort EM_PPC = 20;                 // PowerPC
        public const ushort EM_PPC64 = 21;               // 64-bit PowerPC
        public const ushort EM_S390 = 22;                // IBM S/390
        public const ushort EM_V800 = 36;                // NEC V800 series
        public const ushort EM_FR20 = 37;                // Fujitsu FR20
        public const ushort EM_RH32 = 38;                // TRW RH-32
        public const ushort EM_RCE = 39;                 // Motorola RCE
        public const ushort EM_ARM = 40;                 // Arm (Up to ARMv7/AArch32)
        public const ushort EM_ALPHA = 41;               // Digital Alpha
        public const ushort EM_SH = 42;                  // SuperH
        public const ushort EM_SPARCV9 = 43;             // Sparc Version 9
        public const ushort EM_TRICORE = 44;             // Siemens TriCore embedded processor
        public const ushort EM_ARC = 45;                 // Argonaut RISC core
        public const ushort EM_H8_300 = 46;              // Hitachi H8/300
        public const ushort EM_H8_300H = 47;             // Hitachi H8/300H
        public const ushort EM_H8S = 48;                 // Hitachi H8S
        public const ushort EM_H8_500 = 49;
        public const ushort EM_IA_64 = 50;               // Intel Itanium-64
        public const ushort EM_MIPS_X = 51;              // Standford MIPS-X
        public const ushort EM_COLDFIRE = 52;            // Motorola ColdFire
        public const ushort EM_68HC12 = 53;              // Motorola M68HC12
        public const ushort EM_MMA = 54;                 // Fujitsu MMA Multimedia Accelerator
        public const ushort EM_PCP = 55;                 // Siemens PCP
        public const ushort EM_NCPU = 56;                // Sony nCPU embedded RISC processor
        public const ushort EM_NDR1 = 57;                // Denso NDR1 microprocessor
        public const ushort EM_STARCORE = 58;            // Motorola Star*Core processor
        public const ushort EM_ME16 = 59;                // Toyota ME16 processor
        public const ushort EM_ST100 = 60;               // STMicroelectronics ST100 processor
        public const ushort EM_TINYJ = 61;               // Advanced Logic Corp TinyJ embedded processor family
        public const ushort EM_X86_64 = 62;              // AMD x86-64
        public const ushort EM_PDSP = 63;                // Sony DSP Processor
        public const ushort EM_PDP10 = 64;               // Digital Equipment Corp. PDP-10
        public const ushort EM_PDP11 = 65;               // Digital Equipment Core. PDP-11
        public const ushort EM_FX66 = 66;                // Siemens FX66 microscontroller
        public const ushort EM_ST9PLUS = 67;             // STMicroelectronics STD9+ 8/16 bit microcontroller
        public const ushort EM_ST7 = 68;                 // STMicroelectronics ST7 8-bit microcontroller
        public const ushort EM_68HC16 = 69;              // Motorola MC68HC16 microcontroller
        public const ushort EM_68HC11 = 70;              // Motorola MC68HC11 microcontroller
        public const ushort EM_68HC08 = 71;              // Motorola MC68HC08 microcontroller
        public const ushort EM_68HC05 = 72;              // Motorola MC68H051 microcontroller
        public const ushort EM_SVX = 73;                 // Silicon Graphics SVx
        public const ushort EM_ST19 = 74;                // STMicroelectronics ST19 8-bit microcontroller
        public const ushort EM_VAX = 75;                 // Digital VAX
        public const ushort EM_CRIS = 76;                // Axis Communications 32-bit embedded processor
        public const ushort EM_JAVELIN = 77;             // Infineon Technologies 32-bit embedded processor
        public const ushort EM_FIREPATH = 78;            // Element 14 64-bit DSP Processor
        public const ushort EM_ZSP = 79;                 // LSI Logic 16-bit DSP Processor
        public const ushort EM_MMIX = 80;
        public const ushort EM_HUANY = 81;
        public const ushort EM_PRISM = 82;
        public const ushort EM_AVR = 83;
        public const ushort EM_FR30 = 84;
        public const ushort EM_D10V = 85;
        public const ushort EM_D30V = 86;
        public const ushort EM_V850 = 87;
        public const ushort EM_M32R = 88;
        public const ushort EM_MN10300 = 89;
        public const ushort EM_MN10200 = 90;
        public const ushort EM_PJ = 91;
        public const ushort EM_OR1K = 92;
        public const ushort EM_ARC_A5 = 93;
        public const ushort EM_XTENSA = 94;
        public const ushort EM_VIDEOCORE = 95;
        public const ushort EM_TMM_GPP = 96;
        public const ushort EM_NS32K = 97;
        public const ushort EM_TPC = 98;
        public const ushort EM_SNP1K = 99;
        public const ushort EM_ST200 = 100;
        public const ushort EM_IP2K = 101;
        public const ushort EM_MAX = 102;
        public const ushort EM_CR = 103;
        public const ushort EM_F2MC16 = 104;
        public const ushort EM_MSP430 = 105;
        public const ushort EM_BLACKFIN = 106;
        public const ushort EM_SE_C33 = 107;
        public const ushort EM_SEP = 108;
        public const ushort EM_ARCA = 109;
        public const ushort EM_UNICORE = 110;
        public const ushort EM_ALTERA_NIOS2 = 113;
        public const ushort EM_CRX = 114;
        public const ushort EM_TMS320C6000 = 140;        // TMS320C6000 Family
        public const ushort EM_TI_PRU = 144;
        public const ushort EM_ELBRUS = 175;             // MCST Elbrus e2k
        public const ushort EM_AARCH64 = 183;            // Arm 64-bits (ARMv8/AArch64)
        public const ushort EM_TILEGX = 191;
        public const ushort EM_Z80 = 220;                // Zilog Z80
        public const ushort EM_RISCV = 243;              // RISC-V
        public const ushort EM_BPF = 247;                // Berkeley Packet Filter
        public const ushort EM_MT = 0x2530;
        public const ushort EM_DLX = 0x5aa5;
        public const ushort EM_FRV = 0x5441;             // FRV.
        public const ushort EM_X16X = 0x4688;            // Infineon Technologies 16-bit microcontroller with C166-V2 core.
        public const ushort EM_XSTORMY16 = 0xad45;       // Xstorym16
        public const ushort EM_M32C = 0xfeb0;            // Renesas M32C
        public const ushort EM_IQ2000 = 0xfeba;          // Vitesse IQ2000
        public const ushort EM_NIOS32 = 0xfebb;          // NIOS

        // p_type
        public const uint PT_NULL = 0;
        public const uint PT_LOAD = 1;
        public const uint PT_DYNAMIC = 2;
        public const uint PT_INTERP = 3;
        public const uint PT_NOTE = 4;
        public const uint PT_SHLIB = 5;
        public const uint PT_PHDR = 6;
        public const uint PT_TLS = 7;                    // Thread local storage segment
        public const uint PT_GNU_EH_FRAME = 0x6474e550;
        public const uint PT_GNU_STACK = 0x6474e551;
        public const uint PT_GNU_RELRO = 0x6474e552;
        public const uint PT_GNU_PROPERTY = 0x6474e553;

        // p_flags
        public const uint PF_X = 1;
        public const uint PF_W = 2;
        public const uint PF_R = 4;

        // d_tag
        public const int DT_NULL = 0;                   // Marks end of dynamic array
        public const int DT_NEEDED = 1;                 // Name of needed library (DT_STRTAB offset)
        public const int DT_PLTRELSZ = 2;               // Size, in bytes, of relocations in PLT
        public const int DT_PLTGOT = 3;                 // Address of PLT and/or GOT
        public const int DT_HASH = 4;                   // Address of symbol hash table
        public const int DT_STRTAB = 5;                 // Address of string table
        public const int DT_SYMTAB = 6;                 // Address of symbol table
        public const int DT_RELA = 7;                   // Address of Rela relocation table
        public const int DT_RELASZ = 8;                 // Size, in bytes, of DT_RELA table
        public const int DT_RELAENT = 9;                // Size, in bytes, of one DT_RELA entry
        public const int DT_STRSZ = 10;                 // Size, in bytes, of DT_STRTAB table
        public const int DT_SYMENT = 11;                // Size, in bytes, of one DT_SYMTAB entry
        public const int DT_INIT = 12;                  // Address of initialization function
        public const int DT_FINI = 13;                  // Address of termination function
        public const int DT_SONAME = 14;                // Shared object name (DT_STRTAB offset)
        public const int DT_RPATH = 15;                 // Library search path (DT_STRTAB offset)
        public const int DT_SYMBOLIC = 16;              // Start symbol search within local object
        public const int DT_REL = 17;                   // Address of Rel relocation table
        public const int DT_RELSZ = 18;                 // Size, in bytes, of DT_REL table
        public const int DT_RELENT = 19;                // Size, in bytes, of one DT_REL entry
        public const int DT_PLTREL = 20;                // Type of PLT relocation entries
        public const int DT_DEBUG = 21;                 // Used for debugging; unspecified
        public const int DT_TEXTREL = 22;               // Relocations might modify non-writable seg
        public const int DT_JMPREL = 23;                // Address of relocations associated with PLT
        public const int DT_BIND_NOW = 24;              // Process all relocations at load-time
        public const int DT_INIT_ARRAY = 25;            // Address of initialization function array
        public const int DT_FINI_ARRAY = 26;            // Size, in bytes, of DT_INIT_ARRAY array
        public const int DT_INIT_ARRAYSZ = 27;          // Address of termination function array
        public const int DT_FINI_ARRAYSZ = 28;          // Size, in bytes, of DT_FINI_ARRAY array
        public const int DT_RUNPATH = 29;               // overrides DT_RPATH
        public const int DT_FLAGS = 30;                 // Encodes ORIGIN, SYMBOLIC, TEXTREL, BIND_NOW, STATIC_TLS
        public const int DT_ENCODING = 31;              // ???
        public const int DT_PREINIT_ARRAY = 32;         // Address of pre-init function array
        public const int DT_PREINIT_ARRAYSZ = 33;       // Size, in bytes, of DT_PREINIT_ARRAY array
        public const int DT_FLAGS_1 = 0x6ffffffb;       // ELF dynamic flags (Linux, OpenBSD)

        // Flag values for DT_FLAGS
        public const uint DF_ORIGIN = 0x00000001;        // uses $ORIGIN
        public const uint DF_SYMBOLIC = 0x00000002;
        public const uint DF_TEXTREL = 0x00000004;
        public const uint DF_BIND_NOW = 0x00000008;
        public const uint DF_STATIC_TLS = 0x00000010;

        // Flag values for DT_FLAGS_1
        public const uint DF_1_NOW = 0x00000001;         // Same as DF_BIND_NOW
        public const uint DF_1_GLOBAL = 0x00000002;      // Unused
        public const uint DF_1_GROUP = 0x00000004;       // Is member of group
        public const uint DF_1_NODELETE = 0x00000008;    // Cannot be deleted from process
        public const uint DF_1_LOADFLTR = 0x00000010;    // Immediate loading of filters
        public const uint DF_1_INITFIRST = 0x00000020;   // init/fini takes priority
        public const uint DF_1_NOOPEN = 0x00000040;      // Do not allow loading on dlopen()
        public const uint DF_1_ORIGIN = 0x00000080;      // Require $ORIGIN processing
        public const uint DF_1_DIRECT = 0x00000100;      // Enable direct bindings
        public const uint DF_1_INTERPOSE = 0x00000400;   // Is an interposer
        public const uint DF_1_NODEFLIB = 0x00000800;    // Ignore default library search path
        public const uint DF_1_NODUMP = 0x00001000;      // Cannot be dumped with dldump(3C)
        public const uint DF_1_CONFALT = 0x00002000;     // Configuration alternative
        public const uint DF_1_ENDFILTEE = 0x00004000;   // Filtee ends filter's search
        public const uint DF_1_DISPRELDNE = 0x00008000;  // Did displacement relocation
        public const uint DF_1_DISPRELPND = 0x00010000;  // Pending displacement relocation
        public const uint DF_1_NODIRECT = 0x00020000;    // Has non-direct bindings
        public const uint DF_1_IGNMULDEF = 0x00040000;   // Used internally
        public const uint DF_1_NOKSYMS = 0x00080000;     // Used internally
        public const uint DF_1_NOHDR = 0x00100000;       // Used internally
        public const uint DF_1_EDITED = 0x00200000;      // Has been modified since build
        public const uint DF_1_NORELOC = 0x00400000;     // Used internally
        public const uint DF_1_SYMINTPOSE = 0x00800000;  // Has individual symbol interposers
        public const uint DF_1_GLOBAUDIT = 0x01000000;   // Require global auditing
        public const uint DF_1_SINGLETON = 0x02000000;   // Has singleton symbols
        public const uint DF_1_STUB = 0x04000000;        // Stub
        public const uint DF_1_PIE = 0x08000000;         // Position Independent Executable

        // sh_type
        public const uint SHT_NULL = 0;
        public const uint SHT_PROGBITS = 1;
        public const uint SHT_SYMTAB = 2;
        public const uint SHT_STRTAB = 3;
        public const uint SHT_RELA = 4;
        public const uint SHT_HASH = 5;
        public const uint SHT_DYNAMIC = 6;
        public const uint SHT_NOTE = 7;
        public const uint SHT_NOBITS = 8;
        public const uint SHT_REL = 9;
        public const uint SHT_SHLIB = 10;
        public const uint SHT_DYNSYM = 11;
        public const uint SHT_INIT_ARRAY = 14;
        public const uint SHT_FINI_ARRAY = 15;
        public const uint SHT_PREINIT_ARRAY = 16;
        public const uint SHT_GROUP = 17;
        public const uint SHT_SYMTAB_SHNDX = 18;
        public const uint SHT_NUM = 19;

#if NETFRAMEWORK
        private static ushort ReverseEndianness(ushort value)
        {
            return (ushort)((value >> 8) + (value << 8));
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Completeness")]
        private static short ReverseEndianness(short value)
        {
            return unchecked((short)ReverseEndianness(unchecked((ushort)value)));
        }

        private static uint ReverseEndianness(uint value)
        {
            return (value >> 24) +
                ((value & 0x00FF0000) >> 8) +
                ((value & 0x0000FF00) << 8) +
                ((value & 0x000000FF) << 24);
        }

        private static int ReverseEndianness(int value)
        {
            return unchecked((int)ReverseEndianness(unchecked((uint)value)));
        }

        private static ulong ReverseEndianness(ulong value)
        {
            return (value >> 56) +
                ((value & 0x00FF000000000000) >> 40) +
                ((value & 0x0000FF0000000000) >> 24) +
                ((value & 0x000000FF00000000) >> 8) +
                ((value & 0x00000000FF000000) << 8) +
                ((value & 0x0000000000FF0000) << 24) +
                ((value & 0x000000000000FF00) << 40) +
                ((value & 0x00000000000000FF) << 56);
        }

        private static long ReverseEndianness(long value)
        {
            return unchecked((long)ReverseEndianness(unchecked((ulong)value)));
        }
#endif
    }
}
