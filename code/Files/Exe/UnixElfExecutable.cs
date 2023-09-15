//#define DUMPELF

namespace RJCP.IO.Files.Exe
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using RJCP.Core;
    using Unix;

    /// <summary>
    /// A Unix ELF <see cref="FileExecutable"/>.
    /// </summary>
    public sealed class UnixElfExecutable : FileExecutable
    {
        internal static UnixElfExecutable GetFile(BinaryReader br)
        {
            try {
                byte[] elfHdr = br.ReadBytes(ElfHeader.EI_NIDENT);
                if (elfHdr == null) return null;
                if (elfHdr.Length != ElfHeader.EI_NIDENT) return null;

                if (elfHdr[ElfHeader.EI_MAG0] != ElfHeader.EI_MAG0_VALUE) return null;
                if (elfHdr[ElfHeader.EI_MAG1] != ElfHeader.EI_MAG1_VALUE) return null;
                if (elfHdr[ElfHeader.EI_MAG2] != ElfHeader.EI_MAG2_VALUE) return null;
                if (elfHdr[ElfHeader.EI_MAG3] != ElfHeader.EI_MAG3_VALUE) return null;
                if (elfHdr[ElfHeader.EI_VERSION] != ElfHeader.EV_CURRENT) return null;

                if (elfHdr[ElfHeader.EI_CLASS] != ElfHeader.ELFCLASS32 &&
                    elfHdr[ElfHeader.EI_CLASS] != ElfHeader.ELFCLASS64) return null;
                if (elfHdr[ElfHeader.EI_DATA] != ElfHeader.ELFDATA2LSB &&
                    elfHdr[ElfHeader.EI_DATA] != ElfHeader.ELFDATA2MSB) return null;

                UnixElfExecutable exe = new UnixElfExecutable() {
                    m_ArchitectureSize = elfHdr[ElfHeader.EI_CLASS] == ElfHeader.ELFCLASS32 ? 32 : 64,
                    m_IsLittleEndian = elfHdr[ElfHeader.EI_DATA] == ElfHeader.ELFDATA2LSB,
                    m_TargetOs = GetFileTargetOs(elfHdr[ElfHeader.EI_OSABI])
                };

                br.BaseStream.Position = 0;
                if (elfHdr[ElfHeader.EI_CLASS] == ElfHeader.ELFCLASS32) {
                    if (!GetElf32Header(br).TryGet(out ElfHeader.elf32_hdr hdr32)) return null;
                    Dump(br, ref hdr32);

                    exe.m_IsDll = GetIsDll(br, ref hdr32);
                    exe.m_IsExe = GetIsExe(br, ref hdr32);
                    exe.m_IsCore = hdr32.e_type == ElfHeader.ET_CORE;
                    exe.m_IsPositionIndependent = hdr32.e_type == ElfHeader.ET_DYN;
                    exe.m_MachineType = GetFileMachineType(hdr32.e_machine);
                } else {
                    if (!GetElf64Header(br).TryGet(out ElfHeader.elf64_hdr hdr64)) return null;
                    Dump(br, ref hdr64);

                    exe.m_IsDll = GetIsDll(br, ref hdr64);
                    exe.m_IsExe = GetIsExe(br, ref hdr64);
                    exe.m_IsCore = hdr64.e_type == ElfHeader.ET_CORE;
                    exe.m_IsPositionIndependent = hdr64.e_type == ElfHeader.ET_DYN;
                    exe.m_MachineType = GetFileMachineType(hdr64.e_machine);
                }
                return exe;
            } catch (EndOfStreamException) {
                return null;
            }
        }

        private static FileTargetOs GetFileTargetOs(byte osabi)
        {
            switch (osabi) {
            case ElfHeader.ELFOSABI_SYSV: return FileTargetOs.SysV;
            case ElfHeader.ELFOSABI_ARM: return FileTargetOs.Arm;
            case ElfHeader.ELFOSABI_LINUX: return FileTargetOs.Linux;
            case ElfHeader.ELFOSABI_FREEBSD: return FileTargetOs.FreeBSD;
            case ElfHeader.ELFOSABI_NETBSD: return FileTargetOs.NetBSD;
            case ElfHeader.ELFOSABI_OPENBSD: return FileTargetOs.OpenBSD;
            case ElfHeader.ELFOSABI_SOLARIS: return FileTargetOs.Solaris;
            case ElfHeader.ELFOSABI_HPUX: return FileTargetOs.HPUX;
            default: return FileTargetOs.Unknown;
            }
        }

        private static FileMachineType GetFileMachineType(ushort machine)
        {
            switch (machine) {
            case ElfHeader.EM_ARM: return FileMachineType.Arm;
            case ElfHeader.EM_AARCH64: return FileMachineType.Arm64;
            case ElfHeader.EM_386: return FileMachineType.Intel386;
            case ElfHeader.EM_IA_64: return FileMachineType.Itanium64;
            case ElfHeader.EM_X86_64: return FileMachineType.Amd64;
            case ElfHeader.EM_ALPHA: return FileMachineType.Alpha;
            case 0x9026: return FileMachineType.Alpha;  // Found on old Debian Linux.
            case ElfHeader.EM_PPC: return FileMachineType.PowerPC;
            case ElfHeader.EM_PPC64: return FileMachineType.PowerPC64;
            case ElfHeader.EM_SPARC: return FileMachineType.Sparc;
            case ElfHeader.EM_SPARCV9: return FileMachineType.SparcV9;
            case ElfHeader.EM_RISCV: return FileMachineType.RiscV;
            case ElfHeader.EM_MIPS: return FileMachineType.Mips;
            case ElfHeader.EM_MIPS_RS4_BE: return FileMachineType.Mips;
            case ElfHeader.EM_PARISC: return FileMachineType.PARISC;
            case ElfHeader.EM_S390: return FileMachineType.S390x;
            case ElfHeader.EM_68K: return FileMachineType.M68k;
            case ElfHeader.EM_VAX: return FileMachineType.Vax;
            case ElfHeader.EM_SH: return FileMachineType.SuperH;
            default: return FileMachineType.Unknown;
            }
        }

        private static Result<ElfHeader.elf32_hdr> GetElf32Header(BinaryReader br)
        {
            var hdr32 = br.ReadStruct<ElfHeader.elf32_hdr>();
            hdr32.FixEndianness();

            if (hdr32.e_version != ElfHeader.EV_CURRENT)
                return Result.FromException<ElfHeader.elf32_hdr>(new BadImageFormatException("Not a valid ELF image. Invalid Version."));

            // Check that the file can contain the Program Header.
            if (hdr32.e_phnum == 0 ||
                hdr32.e_phentsize < Marshal.SizeOf(typeof(ElfHeader.elf32_phdr)) ||
                hdr32.e_phoff >= br.BaseStream.Length ||
                hdr32.e_phoff + hdr32.e_phentsize * hdr32.e_phnum > br.BaseStream.Length)
                return Result.FromException<ElfHeader.elf32_hdr>(new BadImageFormatException("Not a valid ELF image"));

            if (hdr32.e_shnum != 0) {
                if (hdr32.e_shentsize < Marshal.SizeOf(typeof(ElfHeader.elf32_shdr)) ||
                    hdr32.e_shoff >= br.BaseStream.Length ||
                    hdr32.e_shoff + hdr32.e_shentsize * hdr32.e_shnum > br.BaseStream.Length)
                    return Result.FromException<ElfHeader.elf32_hdr>(new BadImageFormatException("Not a valid ELF image"));
                if (hdr32.e_shstrndx >= hdr32.e_shnum)
                    return Result.FromException<ElfHeader.elf32_hdr>(new BadImageFormatException("Not a valid ELF image"));
            }

            return hdr32;
        }

        private static Result<ElfHeader.elf64_hdr> GetElf64Header(BinaryReader br)
        {
            var hdr64 = br.ReadStruct<ElfHeader.elf64_hdr>();
            hdr64.FixEndianness();

            if (hdr64.e_version != ElfHeader.EV_CURRENT)
                return Result.FromException<ElfHeader.elf64_hdr>(new BadImageFormatException("Not a valid ELF image. Invalid Version."));

            // Check that the file can contain the Program Header.
            if (hdr64.e_phnum == 0 ||
                hdr64.e_phentsize < Marshal.SizeOf(typeof(ElfHeader.elf64_phdr)) ||
                hdr64.e_phoff > 0x7FFFFFFF_FFFFFFFF || hdr64.e_phoff >= (ulong)br.BaseStream.Length ||
                hdr64.e_phoff + (ulong)(hdr64.e_phnum * hdr64.e_phentsize) > (ulong)br.BaseStream.Length)
                return Result.FromException<ElfHeader.elf64_hdr>(new BadImageFormatException("Not a valid ELF image"));

            if (hdr64.e_shnum != 0) {
                if (hdr64.e_shentsize < Marshal.SizeOf(typeof(ElfHeader.elf64_shdr)) ||
                    hdr64.e_shoff > 0x7FFFFFFF_FFFFFFFF || hdr64.e_shoff >= (ulong)br.BaseStream.Length ||
                    hdr64.e_shoff + (ulong)(hdr64.e_shentsize * hdr64.e_shnum) > (ulong)br.BaseStream.Length)
                    return Result.FromException<ElfHeader.elf64_hdr>(new BadImageFormatException("Not a valid ELF image"));
                if (hdr64.e_shstrndx >= hdr64.e_shnum)
                    return Result.FromException<ElfHeader.elf64_hdr>(new BadImageFormatException("Not a valid ELF image"));
            }

            return hdr64;
        }

        private static Result<ElfHeader.elf32_phdr> GetPrgHeader(BinaryReader br, ref ElfHeader.elf32_hdr hdr, int index)
        {
            if (index >= hdr.e_phnum)
                return Result.FromException<ElfHeader.elf32_phdr>(new ArgumentOutOfRangeException(nameof(index), "Index exceeds e_phnum"));

            br.BaseStream.Position = hdr.e_phoff + index * hdr.e_phentsize;
            var phdr = br.ReadStruct<ElfHeader.elf32_phdr>();
            phdr.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return phdr;
        }

        private static Result<ElfHeader.elf64_phdr> GetPrgHeader(BinaryReader br, ref ElfHeader.elf64_hdr hdr, int index)
        {
            if (index >= hdr.e_phnum)
                return Result.FromException<ElfHeader.elf64_phdr>(new ArgumentOutOfRangeException(nameof(index), "Index exceeds e_phnum"));

            br.BaseStream.Position = (long)(hdr.e_phoff + unchecked((ulong)(index * hdr.e_phentsize)));
            var phdr = br.ReadStruct<ElfHeader.elf64_phdr>();
            phdr.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return phdr;
        }

        private static Result<ElfHeader.elf32_shdr> GetSectionHeader(BinaryReader br, ref ElfHeader.elf32_hdr hdr, int index)
        {
            if (index >= hdr.e_shnum)
                return Result.FromException<ElfHeader.elf32_shdr>(new ArgumentOutOfRangeException(nameof(index), "Index exceeds e_shnum"));

            br.BaseStream.Position = hdr.e_shoff + index * hdr.e_shentsize;
            var shdr = br.ReadStruct<ElfHeader.elf32_shdr>();
            shdr.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return shdr;
        }

        private static Result<ElfHeader.elf64_shdr> GetSectionHeader(BinaryReader br, ref ElfHeader.elf64_hdr hdr, int index)
        {
            if (index >= hdr.e_shnum)
                return Result.FromException<ElfHeader.elf64_shdr>(new ArgumentOutOfRangeException(nameof(index), "Index exceeds e_shnum"));

            br.BaseStream.Position = (long)(hdr.e_shoff + unchecked((ulong)(index * hdr.e_shentsize)));
            var shdr = br.ReadStruct<ElfHeader.elf64_shdr>();
            shdr.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return shdr;
        }

        private static Result<ElfHeader.elf32_dyn> GetDynElement(BinaryReader br, ref ElfHeader.elf32_hdr hdr, ref ElfHeader.elf32_phdr phdr)
        {
            if (br.BaseStream.Length <= phdr.p_offset + phdr.p_filesz)
                return Result.FromException<ElfHeader.elf32_dyn>(new ArgumentOutOfRangeException(nameof(br), "Position out of bounds"));

            var dyn = br.ReadStruct<ElfHeader.elf32_dyn>();
            dyn.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return dyn;
        }

        private static Result<ElfHeader.elf64_dyn> GetDynElement(BinaryReader br, ref ElfHeader.elf64_hdr hdr, ref ElfHeader.elf64_phdr phdr)
        {
            if (br.BaseStream.Length <= (long)(phdr.p_offset + phdr.p_filesz))
                return Result.FromException<ElfHeader.elf64_dyn>(new ArgumentOutOfRangeException(nameof(br), "Position out of bounds"));

            var dyn = br.ReadStruct<ElfHeader.elf64_dyn>();
            dyn.FixEndianness(hdr.e_ident[ElfHeader.EI_DATA]);
            return dyn;
        }

        private static byte[] GetStringTable(BinaryReader br, ref ElfHeader.elf32_hdr hdr)
        {
            // Get from the section table, index e_shstrndx, which is the string table.
            if (!GetSectionHeader(br, ref hdr, hdr.e_shstrndx).TryGet(out var secstr)) return null;
            if (secstr.sh_offset >= br.BaseStream.Length) return null;
            if (secstr.sh_size > br.BaseStream.Length - secstr.sh_offset) return null;

            br.BaseStream.Position = secstr.sh_offset;
            byte[] strBuffer = br.ReadBytes((int)secstr.sh_size);
            if (strBuffer.Length != secstr.sh_size) return null;

            return strBuffer;
        }

        private static byte[] GetStringTable(BinaryReader br, ref ElfHeader.elf64_hdr hdr)
        {
            // Get from the section table, index e_shstrndx, which is the string table.
            if (!GetSectionHeader(br, ref hdr, hdr.e_shstrndx).TryGet(out var secstr)) return null;
            if (secstr.sh_size > 0x7FFFFFFF || secstr.sh_offset > 0x7FFFFFFF_FFFFFFFF) return null;
            if (secstr.sh_offset >= (ulong)br.BaseStream.Length) return null;
            if (secstr.sh_size > (ulong)br.BaseStream.Length - secstr.sh_offset) return null;

            br.BaseStream.Position = (long)secstr.sh_offset;
            byte[] strBuffer = br.ReadBytes((int)secstr.sh_size);
            if (strBuffer.Length != (int)secstr.sh_size) return null;

            return strBuffer;
        }

        private static unsafe string GetString(byte[] strBuffer, uint offset)
        {
            if (offset >= strBuffer.Length) return null;
            int pos = (int)offset;
            while (strBuffer[pos] != 0 && pos < strBuffer.Length)
                pos++;

            fixed (byte* buffPtr = strBuffer) {
                return Marshal.PtrToStringAnsi(new IntPtr(buffPtr + offset), pos - (int)offset);
            }
        }

        private static bool HasSymTab(BinaryReader br, ref ElfHeader.elf32_hdr hdr)
        {
            byte[] strTable = GetStringTable(br, ref hdr);
            if (strTable == null) return false;

            for (int i = 0; i < hdr.e_shnum; i++) {
                if (!GetSectionHeader(br, ref hdr, i).TryGet(out var shdr)) return false;
                if (shdr.sh_type == ElfHeader.SHT_SYMTAB) {
                    if (shdr.sh_name == 0) return false;
                    string symtabName = GetString(strTable, shdr.sh_name);
                    if (symtabName == null) return false;
                    if (symtabName.Equals(".symtab", StringComparison.InvariantCultureIgnoreCase)) return true;
                }
            }
            return false;
        }

        private static bool HasSymTab(BinaryReader br, ref ElfHeader.elf64_hdr hdr)
        {
            byte[] strTable = GetStringTable(br, ref hdr);
            if (strTable == null) return false;

            for (int i = 0; i < hdr.e_shnum; i++) {
                if (!GetSectionHeader(br, ref hdr, i).TryGet(out var shdr)) return false;
                if (shdr.sh_type == ElfHeader.SHT_SYMTAB) {
                    if (shdr.sh_name == 0) return false;
                    string symtabName = GetString(strTable, shdr.sh_name);
                    if (symtabName == null) return false;
                    if (symtabName.Equals(".symtab", StringComparison.InvariantCultureIgnoreCase)) return true;
                }
            }
            return false;
        }

        [Conditional("DUMPELF")]
        private static void Dump(BinaryReader br, ref ElfHeader.elf32_hdr hdr)
        {
            Console.WriteLine("Class: {0}", hdr.e_ident[ElfHeader.EI_CLASS] == ElfHeader.ELFCLASS32 ? 32 : 64);
            Console.WriteLine("Data: {0}", hdr.e_ident[ElfHeader.EI_DATA] == ElfHeader.ELFDATA2LSB ? "LE" : "BE");
            Console.WriteLine("ABI: {0}", hdr.e_ident[ElfHeader.EI_OSABI]);
            Console.WriteLine("ABIVer: {0}", hdr.e_ident[ElfHeader.EI_ABIVERSION]);
            Console.WriteLine("Type: {0}", hdr.e_type);
            Console.WriteLine("Machine: 0x{0:x4}", hdr.e_machine);
            Console.WriteLine("Entry: 0x{0:x8}", hdr.e_entry);
            Console.WriteLine("Flags: 0x{0:x8}", hdr.e_flags);
            Console.WriteLine("ELF Program Headers");
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return;
                switch (phdr.p_type) {
                case ElfHeader.PT_PHDR:
                    Console.WriteLine("  PT_PHDR");
                    break;
                case ElfHeader.PT_INTERP:
                    Console.WriteLine("  PT_INTERP");
                    break;
                case ElfHeader.PT_TLS:
                    Console.WriteLine("  PT_TLS");
                    break;
                case ElfHeader.PT_DYNAMIC:
                    ulong dynlen = phdr.p_filesz;
                    if (dynlen > 0) {
                        br.BaseStream.Position = phdr.p_offset;
                        while (true) {
                            if (!GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn)) break;
                            Console.WriteLine($"  PT_DYNAMIC: {dyn.d_tag:x8} = {dyn.d_val:x8} ({GetDynTag(dyn.d_tag)})");
                            if (dyn.d_tag == ElfHeader.DT_NULL) break;
                        }
                    }
                    break;
                }
            }

            Console.WriteLine($"ELF Section Headers: {hdr.e_shnum}");
            byte[] strTable = GetStringTable(br, ref hdr);
            if (strTable == null) return;

            Console.WriteLine($"Section: {hdr.e_shnum}");
            for (int i = 0; i < hdr.e_shnum; i++) {
                if (!GetSectionHeader(br, ref hdr, i).TryGet(out var shdr)) return;
                Console.WriteLine($"  shdr.sh_name = {shdr.sh_name:x8}; .sh_type = {shdr.sh_type:x} ({GetString(strTable, shdr.sh_name)})");
            }
        }

        [Conditional("DUMPELF")]
        private static void Dump(BinaryReader br, ref ElfHeader.elf64_hdr hdr)
        {
            Console.WriteLine("Class: {0}", hdr.e_ident[ElfHeader.EI_CLASS] == ElfHeader.ELFCLASS32 ? 32 : 64);
            Console.WriteLine("Data: {0}", hdr.e_ident[ElfHeader.EI_DATA] == ElfHeader.ELFDATA2LSB ? "LE" : "BE");
            Console.WriteLine("ABI: {0}", hdr.e_ident[ElfHeader.EI_OSABI]);
            Console.WriteLine("ABIVer: {0}", hdr.e_ident[ElfHeader.EI_ABIVERSION]);
            Console.WriteLine("Type: {0}", hdr.e_type);
            Console.WriteLine("Machine: 0x{0:x4}", hdr.e_machine);
            Console.WriteLine("Entry: 0x{0:x8}", hdr.e_entry);
            Console.WriteLine("Flags: 0x{0:x8}", hdr.e_flags);
            Console.WriteLine("ELF Program Headers");
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return;
                switch (phdr.p_type) {
                case ElfHeader.PT_PHDR:
                    Console.WriteLine("  PT_PHDR");
                    break;
                case ElfHeader.PT_INTERP:
                    Console.WriteLine("  PT_INTERP");
                    break;
                case ElfHeader.PT_TLS:
                    Console.WriteLine("  PT_TLS");
                    break;
                case ElfHeader.PT_DYNAMIC:
                    ulong dynlen = phdr.p_filesz;
                    if (dynlen > 0) {
                        br.BaseStream.Position = (long)phdr.p_offset;
                        while (true) {
                            if (!GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn)) break;
                            Console.WriteLine($"  PT_DYNAMIC: {dyn.d_tag:x8} = {dyn.d_val:x8} ({GetDynTag(dyn.d_tag)})");
                            if (dyn.d_tag == ElfHeader.DT_NULL) break;
                        }
                    }
                    break;
                }
            }

            byte[] strTable = GetStringTable(br, ref hdr);
            if (strTable == null) return;

            Console.WriteLine($"ELF Section Headers: {hdr.e_shnum}");
            for (int i = 0; i < hdr.e_shnum; i++) {
                if (!GetSectionHeader(br, ref hdr, i).TryGet(out var shdr)) return;
                Console.WriteLine($"  shdr.sh_name = {shdr.sh_name:x8}; .sh_type = {shdr.sh_type:x8} .sh_offset = {shdr.sh_offset:x8} ({GetString(strTable, shdr.sh_name)})");
            }
        }

        private static string GetDynTag(long dynTag)
        {
            switch (dynTag) {
            case ElfHeader.DT_NEEDED: return "DT_NEEDED";
            case ElfHeader.DT_HASH: return "DT_HASH";
            case ElfHeader.DT_SONAME: return "DT_SONAME";
            case ElfHeader.DT_RPATH: return "DT_RPATH";
            case ElfHeader.DT_FLAGS: return "DT_FLAGS";
            case ElfHeader.DT_FLAGS_1: return "DT_FLAGS_1";
            default: return "";
            }
        }

        /// <summary>
        /// Reads the Program Header to see if there is a P_INTERP header.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> for the file.</param>
        /// <param name="hdr">The 32-bit ELF header.</param>
        /// <returns><see langword="true"/> if this is likely an executable, <see langword="false"/> otherwise.</returns>
        private static bool GetIsExe(BinaryReader br, ref ElfHeader.elf32_hdr hdr)
        {
            if (hdr.e_type != ElfHeader.ET_DYN && hdr.e_type != ElfHeader.ET_EXEC) return false;

            bool pt_interp = false;
            bool dt_soname = false;
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return false;
                switch (phdr.p_type) {
                case ElfHeader.PT_INTERP:
                    if (pt_interp) return false; // Can't contain it twice.
                    pt_interp = true;
                    break;
                case ElfHeader.PT_DYNAMIC:
                    if (hdr.e_type == ElfHeader.ET_DYN) {
                        ulong dynlen = phdr.p_filesz;
                        if (dynlen > 0) {
                            br.BaseStream.Position = phdr.p_offset;
                            bool iterating;
                            do {
                                iterating = GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn);
                                if (iterating) {
                                    switch (dyn.d_tag) {
                                    case ElfHeader.DT_SONAME:
                                        dt_soname = true;
                                        break;
                                    case ElfHeader.DT_FLAGS_1:
                                        if ((dyn.d_val & ElfHeader.DF_1_PIE) != 0) return true;
                                        break;
                                    case ElfHeader.DT_NULL:
                                        iterating = false;
                                        break;
                                    }
                                }
                            } while (iterating);
                        }
                    }
                    break;
                }
            }

            if (hdr.e_type == ElfHeader.ET_EXEC || pt_interp) return true;

            // If it's ET_DYN, check to ensure that we don't export symbols.
            return !(dt_soname || HasSymTab(br, ref hdr));
        }

        /// <summary>
        /// Reads the Program Header to see if there is a P_INTERP header.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> for the file.</param>
        /// <param name="hdr">The 32-bit ELF header.</param>
        /// <returns><see langword="true"/> if this is likely an executable, <see langword="false"/> otherwise.</returns>
        private static bool GetIsExe(BinaryReader br, ref ElfHeader.elf64_hdr hdr)
        {
            if (hdr.e_type != ElfHeader.ET_DYN && hdr.e_type != ElfHeader.ET_EXEC) return false;

            bool pt_interp = false;
            bool dt_soname = false;
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return false;
                switch (phdr.p_type) {
                case ElfHeader.PT_INTERP:
                    if (pt_interp) return false; // Can't contain it twice.
                    pt_interp = true;
                    break;
                case ElfHeader.PT_DYNAMIC:
                    if (hdr.e_type == ElfHeader.ET_DYN) {
                        ulong dynlen = phdr.p_filesz;
                        if (dynlen > 0) {
                            br.BaseStream.Position = (long)phdr.p_offset;
                            bool iterating;
                            do {
                                iterating = GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn);
                                if (iterating) {
                                    switch (dyn.d_tag) {
                                    case ElfHeader.DT_SONAME:
                                        dt_soname = true;
                                        break;
                                    case ElfHeader.DT_FLAGS_1:
                                        if ((dyn.d_val & ElfHeader.DF_1_PIE) != 0) return true;
                                        break;
                                    case ElfHeader.DT_NULL:
                                        iterating = false;
                                        break;
                                    }
                                }
                            } while (iterating);
                        }
                    }
                    break;
                }
            }

            if (hdr.e_type == ElfHeader.ET_EXEC || pt_interp) return true;

            // If it's ET_DYN, check to ensure that we don't export symbols.
            return !(dt_soname || HasSymTab(br, ref hdr));
        }

        /// <summary>
        /// Reads the Program Header to find an SONAME.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> for the file.</param>
        /// <param name="hdr">The 32-bit ELF header.</param>
        /// <returns><see langword="true"/> if this is likely a DLL, <see langword="false"/> otherwise.</returns>
        private static bool GetIsDll(BinaryReader br, ref ElfHeader.elf32_hdr hdr)
        {
            if (hdr.e_type != ElfHeader.ET_DYN) return false;
            if (hdr.e_shnum == 0) return false;

            bool pt_interp = false;
            bool ispie = false;
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return false;
                switch (phdr.p_type) {
                case ElfHeader.PT_INTERP:
                    pt_interp = true;
                    break;
                case ElfHeader.PT_DYNAMIC:
                    ulong dynlen = phdr.p_filesz;
                    if (dynlen > 0) {
                        br.BaseStream.Position = phdr.p_offset;
                        bool iterating;
                        do {
                            iterating = GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn);
                            if (iterating) {
                                switch (dyn.d_tag) {
                                case ElfHeader.DT_SONAME:
                                    if (dyn.d_val != 0) return true;
                                    break;
                                case ElfHeader.DT_FLAGS_1:
                                    if ((dyn.d_val & ElfHeader.DF_1_PIE) != 0) ispie = true;
                                    break;
                                case ElfHeader.DT_NULL:
                                    iterating = false;
                                    break;
                                }
                            }
                        } while (iterating);
                    }
                    break;
                }
            }

            if (pt_interp || ispie) return false;

            // If we got here, it's because it's ET_DYN, no PT_interp, no SONAME found, and DF_1_PIE is not found. So
            // it's not an EXE, we don't have proof it's a shared library yet. Scan the section headers for `.symtab`
            // which is sh_type=2.
            return HasSymTab(br, ref hdr);
        }

        /// <summary>
        /// Reads the Program Header to find an SONAME.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> for the file.</param>
        /// <param name="hdr">The 32-bit ELF header.</param>
        /// <returns><see langword="true"/> if this is likely a DLL, <see langword="false"/> otherwise.</returns>
        private static bool GetIsDll(BinaryReader br, ref ElfHeader.elf64_hdr hdr)
        {
            if (hdr.e_type != ElfHeader.ET_DYN) return false;
            if (hdr.e_shnum == 0) return false;

            bool pt_interp = false;
            bool ispie = false;
            for (int i = 0; i < hdr.e_phnum; i++) {
                if (!GetPrgHeader(br, ref hdr, i).TryGet(out var phdr)) return false;
                switch (phdr.p_type) {
                case ElfHeader.PT_INTERP:
                    pt_interp = true;
                    break;
                case ElfHeader.PT_DYNAMIC:
                    ulong dynlen = phdr.p_filesz;
                    if (dynlen > 0) {
                        br.BaseStream.Position = (long)phdr.p_offset;
                        bool iterating;
                        do {
                            iterating = GetDynElement(br, ref hdr, ref phdr).TryGet(out var dyn);
                            if (iterating) {
                                switch (dyn.d_tag) {
                                case ElfHeader.DT_SONAME:
                                    if (dyn.d_val != 0) return true;
                                    break;
                                case ElfHeader.DT_FLAGS_1:
                                    if ((dyn.d_val & ElfHeader.DF_1_PIE) != 0) ispie = true;
                                    break;
                                case ElfHeader.DT_NULL:
                                    iterating = false;
                                    break;
                                }
                            }
                        } while (iterating);
                    }
                    break;
                }
            }

            if (pt_interp || ispie) return false;

            // If we got here, it's because it's ET_DYN, no PT_interp, no SONAME found, and DF_1_PIE is not found. So
            // it's not an EXE, we don't have proof it's a shared library yet. Scan the section headers for `.symtab`
            // which is sh_type=2.
            return HasSymTab(br, ref hdr);
        }

        internal UnixElfExecutable() { }

        private FileMachineType m_MachineType;

        /// <summary>
        /// Gets the type of the machine the executable targets.
        /// </summary>
        /// <value>The type of the machine the executable targets.</value>
        public override FileMachineType MachineType { get { return m_MachineType; } }

        private FileTargetOs m_TargetOs;

        /// <summary>
        /// Gets the OS the executable targets.
        /// </summary>
        /// <value>The OS the executable targets.</value>
        public override FileTargetOs TargetOs { get { return m_TargetOs; } }

        private bool m_IsLittleEndian;

        /// <summary>
        /// Gets a value indicating whether this instance is little endian.
        /// </summary>
        /// <value><see langword="true"/> if this instance is little endian; otherwise, <see langword="false"/>.</value>
        public override bool IsLittleEndian { get { return m_IsLittleEndian; } }

        private bool m_IsExe;

        /// <summary>
        /// Gets a value indicating whether this instance is an executable.
        /// </summary>
        /// <value><see langword="true"/> if this instance is executable; otherwise, <see langword="false"/>.</value>
        public override bool IsExe { get { return m_IsExe; } }

        private bool m_IsDll;

        /// <summary>
        /// Gets a value indicating whether this instance is DLL.
        /// </summary>
        /// <value><see langword="true"/> if this instance is DLL; otherwise, <see langword="false"/>.</value>
        public override bool IsDll { get { return m_IsDll; } }

        private int m_ArchitectureSize;

        /// <summary>
        /// Gets the architecture address size for the executable binary.
        /// </summary>
        /// <value>The architecture address size of the executable binary.</value>
        public override int ArchitectureSize { get { return m_ArchitectureSize; } }

        private bool m_IsCore;

        /// <summary>
        /// Gets a value indicating whether this instance is a core dump file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is a core dump file; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCore { get { return m_IsCore; } }

        private bool m_IsPositionIndependent;

        /// <summary>
        /// Gets a value indicating whether this instance is position independent.
        /// </summary>
        /// <value><see langword="true"/> if this instance is position independent; otherwise, <see langword="false"/>.</value>
        public bool IsPositionIndependent { get { return m_IsPositionIndependent; } }
    }
}
