namespace RJCP.IO.Files.Exe
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ElfGen;
    using NUnit.Framework;
    using RJCP.CodeQuality.IO;
    using RJCP.Core;

    [TestFixture(true, true)]
    [TestFixture(true, false)]
    [TestFixture(false, true)]
    [TestFixture(false, false)]
    public class UnixElfExecutableTest
    {
        private readonly bool m_Is32Bit;
        private readonly bool m_IsLittleEndian;

        public UnixElfExecutableTest(bool is32Bit, bool isLittleEndian)
        {
            m_Is32Bit = is32Bit;
            m_IsLittleEndian = isLittleEndian;
        }

        private SparseStream GetElf(int machine, byte osabi, int etype, bool ptinterp, bool symtab, bool soname, bool pie)
        {
            return new SparseStream(GetExeDynElfBlocks(machine, osabi, etype, ptinterp, symtab, soname, pie), 0x1000);
        }

        private IList<SparseBlock> GetExeDynElfBlocks(int machine, byte osabi, int etype, bool ptinterp, bool symtab, bool soname, bool pie)
        {
            if (etype != 2 && etype != 3)
                throw new ArgumentOutOfRangeException(nameof(etype), "Unknown e_type");

            // General layout for testing
            // 0000 - 007F - File Header
            // 0080 - 0200 - Program Header Table
            // 0400 - 05FF - Strings section
            // 0600 - 07FF - Dynamic section
            // 0800 - 0FFF - Section Table

            List<SparseBlock> file = new List<SparseBlock>();
            ElfHdr elfGen = new ElfHdr() {
                PrgHdrOffset = 0x80, SectHdrOffset = 0x800, SectHdrStringIndex = 1,
                MachineArch = (ushort)machine, Abi = osabi, ObjectType = (ushort)etype,
                IsLittleEndian = m_IsLittleEndian, WordSize = m_Is32Bit ? 32 : 64
            };

            // Generate Program Headers.
            int phdridx = 0;
            elfGen.ProgramHeader.HdrType = 6;              // PT_PHDR
            elfGen.ProgramHeader.Offset = elfGen.PrgHdrOffset;
            elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            if (ptinterp) {
                elfGen.ProgramHeader.HdrType = 3;              // PT_INTERP
                elfGen.ProgramHeader.Offset = 0x2000;          // Ignored
                elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
                elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
                elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
                file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
                phdridx++;
            }

            if (soname || pie) {
                elfGen.ProgramHeader.HdrType = 2;              // PT_DYNAMIC
                elfGen.ProgramHeader.Offset = 0x600;           // Dynamic Section
                elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
                elfGen.ProgramHeader.FileSize = 0x100;         // Number of elements
                elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored

                // Build the Dynamic Section
                int dynOffset = 0;
                if (soname) {
                    file.Add(new SparseBlock(0x600 + dynOffset, elfGen.ProgramHeader.GetDynHeader(14, 0x1000)));
                    dynOffset += elfGen.ProgramHeader.DynHeaderLength;
                }
                if (pie) {
                    file.Add(new SparseBlock(0x600 + dynOffset, elfGen.ProgramHeader.GetDynHeader(0x6ffffffb, 0x08000000)));
                    dynOffset += elfGen.ProgramHeader.DynHeaderLength;
                }
                file.Add(new SparseBlock(0x600 + dynOffset, elfGen.ProgramHeader.GetDynHeader(0, 0)));
                dynOffset += elfGen.ProgramHeader.DynHeaderLength;

                elfGen.ProgramHeader.FileSize = (ulong)dynOffset;     // Number of elements
                file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
                phdridx++;
            }

            elfGen.PrgHdrRecords = phdridx;

            // The String Section content
            StringSection strings = new StringSection();
            strings.Strings.Add(string.Empty);
            strings.Strings.Add(".symtab");
            strings.Strings.Add(".shstrtab");
            var stringSection = strings.GenerateSection();

            file.Add(new SparseBlock(0x400, strings.Section));

            // Generate Section Headers
            int shdridx = 0;
            file.Add(new SparseBlock((long)elfGen.SectionHeader.GetIndexOffset(shdridx), elfGen.SectionHeader.GenerateSectionHeader()));
            shdridx++;

            elfGen.SectionHeader.Name = stringSection[2].Offset;
            elfGen.SectionHeader.HdrType = 3;              // SHT_STRTAB
            elfGen.SectionHeader.Offset = 0x400;           // Point to string table above.
            elfGen.SectionHeader.Size = (ulong)strings.Section.Length;
            elfGen.SectionHeader.Flags = 0x20;             // SHF_STRINGS
            file.Add(new SparseBlock((long)elfGen.SectionHeader.GetIndexOffset(shdridx), elfGen.SectionHeader.GenerateSectionHeader()));
            shdridx++;

            if (symtab) {
                elfGen.SectionHeader.Name = stringSection[1].Offset;
                elfGen.SectionHeader.HdrType = 2;              // SHT_SYMTAB
                elfGen.SectionHeader.Flags = 0;
                file.Add(new SparseBlock((long)elfGen.SectionHeader.GetIndexOffset(shdridx), elfGen.SectionHeader.GenerateSectionHeader()));
                shdridx++;
            }
            elfGen.SectHdrRecords = shdridx;

            // Now finally we have everything to write the header
            file.Add(new SparseBlock(0, elfGen.GenerateFileHeader()));

            return file;
        }

        private IList<SparseBlock> GetInvalidElfPtInterpMulti(int machine, byte osabi, int etype, bool dll)
        {
            if (etype != 2 && etype != 3)
                throw new ArgumentOutOfRangeException(nameof(etype), "Unknown e_type");

            List<SparseBlock> file = new List<SparseBlock>();
            //StringSection strings = new StringSection();
            //strings.Strings.Add(string.Empty);
            //if (dll) strings.Strings.Add(".symtab");
            //strings.Strings.Add(".shstrtab");
            //var stringSection = strings.GenerateSection();

            // General layout for testing
            // 0000 - 007F - File Header
            // 0080 - 0200 - Program Header Table
            // 0400 - 05FF - Strings section
            // 0600 - 07FF - Dynamic section
            // 0800 - 0FFF - Section Table

            ElfHdr elfGen = new ElfHdr() {
                PrgHdrOffset = 0x80, SectHdrOffset = 0x800, SectHdrStringIndex = 0,
                MachineArch = (ushort)machine, Abi = osabi, ObjectType = (ushort)etype,
                IsLittleEndian = m_IsLittleEndian, WordSize = m_Is32Bit ? 32 : 64
            };

            // Generate Program Headers.
            int phdridx = 0;
            elfGen.ProgramHeader.HdrType = 6;              // PT_PHDR
            elfGen.ProgramHeader.Offset = elfGen.PrgHdrOffset;
            elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            elfGen.ProgramHeader.HdrType = 3;              // PT_INTERP
            elfGen.ProgramHeader.Offset = 0x2000;          // Ignored
            elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            elfGen.ProgramHeader.HdrType = 3;              // PT_INTERP
            elfGen.ProgramHeader.Offset = 0x2000;          // Ignored
            elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            if (dll) {
                elfGen.ProgramHeader.HdrType = 2;              // PT_DYNAMIC
                elfGen.ProgramHeader.Offset = 0x600;           // Dynamic Section
                elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
                elfGen.ProgramHeader.FileSize = 0x100;         // Number of elements
                elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored

                int dynOffset = 0;
                file.Add(new SparseBlock(0x600 + dynOffset, elfGen.ProgramHeader.GetDynHeader(14, 0x1000)));
                dynOffset += elfGen.ProgramHeader.DynHeaderLength;

                file.Add(new SparseBlock(0x600 + dynOffset, elfGen.ProgramHeader.GetDynHeader(0, 0)));
                dynOffset += elfGen.ProgramHeader.DynHeaderLength;

                elfGen.ProgramHeader.FileSize = (ulong)dynOffset;     // Number of elements
                file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
                phdridx++;
            }

            elfGen.PrgHdrRecords = phdridx;

            //// The String Section content
            //file.Add(new SparseBlock(0x400, strings.Section));

            // Generate Section Headers
            int shdridx = 0;
            file.Add(new SparseBlock((long)elfGen.SectionHeader.GetIndexOffset(shdridx), elfGen.SectionHeader.GenerateSectionHeader()));
            shdridx++;

            elfGen.SectHdrRecords = shdridx;

            // Now finally we have everything to write the header
            file.Add(new SparseBlock(0, elfGen.GenerateFileHeader()));

            return file;
        }

        private IList<SparseBlock> GetCoreElfBlocks(int machine, byte osabi)
        {
            List<SparseBlock> file = new List<SparseBlock>();
            ElfHdr elfGen = new ElfHdr() {
                PrgHdrOffset = 0x80,
                MachineArch = (ushort)machine, Abi = osabi, ObjectType = 4,
                IsLittleEndian = m_IsLittleEndian, WordSize = m_Is32Bit ? 32 : 64
            };

            // Generate Program Headers.
            int phdridx = 0;
            elfGen.ProgramHeader.HdrType = 4;              // PT_NOTE
            elfGen.ProgramHeader.Offset = 0x1000;          // Ignored
            elfGen.ProgramHeader.VirtualAddress = 0x1000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            elfGen.ProgramHeader.HdrType = 1;              // PT_LOAD
            elfGen.ProgramHeader.Offset = 0x2000;          // Ignored
            elfGen.ProgramHeader.VirtualAddress = 0x2000;  // Ignored
            elfGen.ProgramHeader.FileSize = 0x100;         // Ignored
            elfGen.ProgramHeader.MemSegmentSize = 0x100;   // Ignored
            file.Add(new SparseBlock((long)elfGen.ProgramHeader.GetIndexOffset(phdridx), elfGen.ProgramHeader.GenerateProgramHeader()));
            phdridx++;

            elfGen.PrgHdrRecords = phdridx;

            // Now finally we have everything to write the header
            file.Add(new SparseBlock(0, elfGen.GenerateFileHeader()));

            return file;
        }

        /// <summary>
        /// Checks if elf. Checks ET_EXEC so it's always an elf. The PT_INTERP, symtab, soname, PIE are not checked.
        /// </summary>
        [Test]
        public void ElfExeTypeExe(
            [Values(false, true)] bool ptinterp,
            [Values(false, true)] bool symtab,
            [Values(false, true)] bool soname,
            [Values(false, true)] bool pie)
        {
            using (Stream stream = GetElf(3, 0, 2, ptinterp, symtab, soname, pie))   // i386, SysV, ET_EXEC, ptinterp, symtab, soname, pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(FileMachineType.Intel386));
                Assert.That(elf.TargetOs, Is.EqualTo(FileTargetOs.SysV));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.True);    // ET_EXEC -> true
                Assert.That(elf.IsDll, Is.False);   // ET_EXEC -> false
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.False);
            }
        }

        /// <summary>
        /// Checks if elf. This is ET_DYN, so is PosIndep. Has PT_INTERP is an elf. Has no SONOME so is not a DLL.
        /// </summary>
        [Test]
        public void ElfExeTypeDynPtInterp(
            [Values(false, true)] bool symtab,
            [Values(false, true)] bool pie)
        {
            // The symtab should only be used when no P_INTERP and no SONAME. Because many exe's have a SymTab.
            using (Stream stream = GetElf(3, 0, 3, true, symtab, false, pie))   // i386, SysV, ET_DYN, PT_INTERP, symtab, no SONAME, pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(FileMachineType.Intel386));
                Assert.That(elf.TargetOs, Is.EqualTo(FileTargetOs.SysV));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.True);    // PT_INTERP -> true
                Assert.That(elf.IsDll, Is.False);   // No SONAME or PIE -> false. Symtab is ignored.
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.True);  // -> ET_DYN
            }
        }

        /// <summary>
        /// Checks if elf. This is ET_DYN, so is PosIndep. No PT_INTERP, no SONAME, has PIE so must be an exe and symtab must be ignored.
        /// </summary>
        [Test]
        public void ElfExeTypeDynPie([Values(false, true)] bool symtab, [Values(false, true)] bool soname)
        {
            // The symtab should only be used when no P_INTERP and no SONAME. Because many exe's have a SymTab.
            using (Stream stream = GetElf(3, 0, 3, false, symtab, soname, true))   // i386, SysV, ET_DYN, no PT_INTERP, symtab, SONAME, pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(FileMachineType.Intel386));
                Assert.That(elf.TargetOs, Is.EqualTo(FileTargetOs.SysV));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.True);              // DF_1_PIE -> true
                Assert.That(elf.IsDll, Is.EqualTo(soname));   // SONAME overrides PIE.
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.True);  // -> ET_DYN
            }
        }

        /// <summary>
        /// Tests the rare case where there is no PT_INTERP, for an EXE or DLL or both. PIE is not set.
        /// </summary>
        [Test]
        public void ElfTypeDynNoPie([Values(false, true)] bool symtab, [Values(false, true)] bool soname)
        {
            // The symtab should only be used when no P_INTERP and no SONAME. Because many exe's have a SymTab.
            using (Stream stream = GetElf(3, 0, 3, false, symtab, soname, false))   // i386, SysV, ET_DYN, no PT_INTERP, symtab, SONAME, no pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(FileMachineType.Intel386));
                Assert.That(elf.TargetOs, Is.EqualTo(FileTargetOs.SysV));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.EqualTo(!symtab && !soname));
                Assert.That(elf.IsDll, Is.EqualTo(symtab || soname));   // SONAME overrides PIE.
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.True);  // -> ET_DYN
            }
        }

        [TestCase(0, FileMachineType.Unknown)]
        [TestCase(1, FileMachineType.Unknown)]    // AT&T WE 32100
        [TestCase(2, FileMachineType.Sparc)]
        [TestCase(3, FileMachineType.Intel386)]
        [TestCase(4, FileMachineType.M68k)]
        [TestCase(5, FileMachineType.Unknown)]    // M88k
        [TestCase(6, FileMachineType.Unknown)]    // Intel MCU
        [TestCase(7, FileMachineType.Unknown)]    // Intel 80860
        [TestCase(8, FileMachineType.Mips)]
        [TestCase(9, FileMachineType.Unknown)]    // IBM System/370
        [TestCase(10, FileMachineType.Mips)]      // MIPS RS3000
        [TestCase(15, FileMachineType.PARISC)]    // HP-PARISC
        [TestCase(19, FileMachineType.Unknown)]   // Intel 80960
        [TestCase(20, FileMachineType.PowerPC)]
        [TestCase(21, FileMachineType.PowerPC64)]
        [TestCase(22, FileMachineType.S390x)]
        [TestCase(23, FileMachineType.Unknown)]   // IBM SPU/SPC
        [TestCase(36, FileMachineType.Unknown)]   // NEC V800
        [TestCase(37, FileMachineType.Unknown)]   // Fujitsu FR20
        [TestCase(38, FileMachineType.Unknown)]   // TRW RH-32
        [TestCase(39, FileMachineType.Unknown)]   // Motorola RCE
        [TestCase(40, FileMachineType.Arm)]
        [TestCase(41, FileMachineType.Alpha)]
        [TestCase(42, FileMachineType.SuperH)]
        [TestCase(43, FileMachineType.SparcV9)]
        [TestCase(44, FileMachineType.Unknown)]   // Siemens TriCore
        [TestCase(45, FileMachineType.Unknown)]   // Argonaut RISC Core
        [TestCase(46, FileMachineType.Unknown)]   // Hitachi H8/300
        [TestCase(47, FileMachineType.Unknown)]   // Hitachi H8/300H
        [TestCase(48, FileMachineType.Unknown)]   // Hitachi H8S
        [TestCase(49, FileMachineType.Unknown)]   // Hitachi H8/500
        [TestCase(50, FileMachineType.Itanium64)]
        [TestCase(51, FileMachineType.Unknown)]   // Standford MIPS-X
        [TestCase(52, FileMachineType.Unknown)]   // Motorola ColdFire
        [TestCase(53, FileMachineType.Unknown)]   // Motorola M68HC12
        [TestCase(54, FileMachineType.Unknown)]   // Fujitsu MMA Multimedia Accelerator
        [TestCase(55, FileMachineType.Unknown)]   // Siemens PCP
        [TestCase(56, FileMachineType.Unknown)]   // Sony nCPU embedded RISC processor
        [TestCase(57, FileMachineType.Unknown)]   // Denso NDR1
        [TestCase(58, FileMachineType.Unknown)]   // Motorola Star*Core
        [TestCase(59, FileMachineType.Unknown)]   // Toyota ME16
        [TestCase(60, FileMachineType.Unknown)]   // STMicroelectronics ST100
        [TestCase(61, FileMachineType.Unknown)]   // Advanced Logic Corp TinyJ
        [TestCase(62, FileMachineType.Amd64)]
        [TestCase(63, FileMachineType.Unknown)]   // Sony DSP
        [TestCase(64, FileMachineType.Unknown)]   // DEC PDP-10
        [TestCase(65, FileMachineType.Unknown)]   // DEC PDP-11
        [TestCase(66, FileMachineType.Unknown)]   // Siemens FX66 microcontroller
        [TestCase(67, FileMachineType.Unknown)]   // STMicroelectronics ST9+ 8/16 bit microcontroller
        [TestCase(68, FileMachineType.Unknown)]   // STMicroelectronics ST7 8-bit microcontroller
        [TestCase(69, FileMachineType.Unknown)]   // Motorola MC68HC16 Microcontroller
        [TestCase(70, FileMachineType.Unknown)]   // Motorola MC68HC11 Microcontroller
        [TestCase(71, FileMachineType.Unknown)]   // Motorola MC68HC08 Microcontroller
        [TestCase(72, FileMachineType.Unknown)]   // Motorola MC68HC05 Microcontroller
        [TestCase(73, FileMachineType.Unknown)]   // Silicon Graphics SVx
        [TestCase(74, FileMachineType.Unknown)]   // STMicroelectronics ST19 8-bit microcontroller
        [TestCase(75, FileMachineType.Vax)]
        [TestCase(76, FileMachineType.Unknown)]   // Axis Communications 32-bit embedded processor
        [TestCase(77, FileMachineType.Unknown)]   // Infineon Technologies 32-bit embedded processor
        [TestCase(78, FileMachineType.Unknown)]   // Element 14 64-bit DSP Processor
        [TestCase(79, FileMachineType.Unknown)]   // LSI Logic 16-bit DSP Processor
        [TestCase(140, FileMachineType.Unknown)]  // TMS320C6000 Family
        [TestCase(171, FileMachineType.Unknown)]  // MCST Elbrus e2k
        [TestCase(183, FileMachineType.Arm64)]    // Arm 64-bits (Armv8/AArch64)
        [TestCase(220, FileMachineType.Unknown)]  // Zilog Z80
        [TestCase(243, FileMachineType.RiscV)]
        [TestCase(247, FileMachineType.Unknown)]  // Berkeley Packet Filter
        [TestCase(257, FileMachineType.Unknown)]  // WDC 65C816
        [TestCase(0x9026, FileMachineType.Alpha)] // Pulled from a binary from a Debian Distribution
        public void ElfExeMachine(int machine, FileMachineType machineType)
        {
            using (Stream stream = GetElf(machine, 0, 2, true, true, false, false))   // machine, SysV, ET_EXEC, PT_INTERP, SymTab, no SONAME, no pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(machineType));
                Assert.That(elf.TargetOs, Is.EqualTo(FileTargetOs.SysV));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.True);    // ET_EXE -> true
                Assert.That(elf.IsDll, Is.False);   // ET_EXE -> false
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.False);
            }
        }

        [TestCase(0, FileTargetOs.SysV)]
        [TestCase(1, FileTargetOs.HPUX)]
        [TestCase(2, FileTargetOs.NetBSD)]
        [TestCase(3, FileTargetOs.Linux)]
        [TestCase(4, FileTargetOs.Unknown)]     // GNU HURD
        [TestCase(5, FileTargetOs.Unknown)]
        [TestCase(6, FileTargetOs.Solaris)]
        [TestCase(7, FileTargetOs.Unknown)]     // AIX
        [TestCase(8, FileTargetOs.Unknown)]     // IRIX
        [TestCase(9, FileTargetOs.FreeBSD)]
        [TestCase(10, FileTargetOs.Unknown)]    // Tru64
        [TestCase(11, FileTargetOs.Unknown)]    // Novell Modesto
        [TestCase(12, FileTargetOs.OpenBSD)]
        [TestCase(13, FileTargetOs.Unknown)]    // OpenVMS
        [TestCase(14, FileTargetOs.Unknown)]    // NonStop Kernel
        [TestCase(15, FileTargetOs.Unknown)]    // AROS
        [TestCase(16, FileTargetOs.Unknown)]    // FenxiOS
        [TestCase(17, FileTargetOs.Unknown)]    // Nuxi CloudABI
        [TestCase(18, FileTargetOs.Unknown)]    // Stratus Technologies OpenVOS
        [TestCase(97, FileTargetOs.Arm)]
        public void ElfExeAbi(byte abi, FileTargetOs targetOs)
        {
            using (Stream stream = GetElf(3, abi, 2, true, true, false, false))   // i386, abi, ET_EXEC, PT_INTERP, SymTab, no SONAME, no pie
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Not.Null);
                Assert.That(elf.MachineType, Is.EqualTo(FileMachineType.Intel386));
                Assert.That(elf.TargetOs, Is.EqualTo(targetOs));
                Assert.That(elf.IsLittleEndian, Is.EqualTo(m_IsLittleEndian));
                Assert.That(elf.IsExe, Is.True);    // ET_EXE -> true
                Assert.That(elf.IsDll, Is.False);   // ET_EXE -> false
                Assert.That(elf.ArchitectureSize, Is.EqualTo(m_Is32Bit ? 32 : 64));
                Assert.That(elf.IsCore, Is.False);
                Assert.That(elf.IsPositionIndependent, Is.False);
            }
        }

        [TestCase(0x7F, (byte)'E', (byte)'L', 0x00)]
        [TestCase(0x7F, (byte)'E', 0x00, (byte)'F')]
        [TestCase(0x7F, 0x00, (byte)'L', (byte)'F')]
        [TestCase(0x00, (byte)'E', (byte)'L', (byte)'F')]
        public void InvalidElfHeader(byte mag0, byte mag1, byte mag2, byte mag3)
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.DirectWrite(0, new byte[] { mag0, mag1, mag2, mag3 });

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void InvalidElfVersionHead([Values(0, 2, 0xFF)] byte version)
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.DirectWrite(6, new byte[] { version });

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void InvalidElfClass([Values(0, 3, 255)] byte elfClass)
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.DirectWrite(4, new byte[] { elfClass });

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void InvalidElfData([Values(0, 3, 255)] byte elfData)
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.DirectWrite(5, new byte[] { elfData });

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void ShortElfFileEmpty()
        {
            using (SparseStream stream = new SparseStream())
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.SetLength(10);

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void ShortElfFile10()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.SetLength(10);

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void ElfHdrVersionLongInvalid()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                stream.DirectWrite(0x14, new byte[] { 0x55, 0x55, 0x55, 0x55 });

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void ElfHdrProgramHeadersCountZero()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x2c, new byte[] { 0x00, 0x00 });
                    break;
                case false:
                    stream.DirectWrite(0x38, new byte[] { 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        [Test]
        public void ElfHdrProgramHeadersInvalid()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x1c, new byte[] { 0x55, 0x55, 0x55, 0x55 });
                    break;
                case false:
                    stream.DirectWrite(0x20, new byte[] { 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if there are no section headers.
        /// </summary>
        [Test]
        public void ElfExeHdrSectionHeadersCountZero()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x30, new byte[] { 0x00, 0x00 });
                    break;
                case false:
                    stream.DirectWrite(0x3c, new byte[] { 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests if there are no section headers.
        /// </summary>
        [Test]
        public void ElfDllHdrSectionHeadersCountZero()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x30, new byte[] { 0x00, 0x00 });
                    break;
                case false:
                    stream.DirectWrite(0x3c, new byte[] { 0x00, 0x00 });
                    break;
                }

                // The symbol table is effectively removed, so it now looks like an elf.
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests if e_phentsize is smaller than expected.
        /// </summary>
        [Test]
        public void ElfHdrPrgHdrEntSizeTooSmall()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // 32-bit should be 0x20.
                    stream.DirectWrite(0x2A, m_IsLittleEndian ?
                        new byte[] { 0x1A, 0x00 } :
                        new byte[] { 0x00, 0x1A });
                    break;
                case false:
                    // 64-bit should be 0x38.
                    stream.DirectWrite(0x36, m_IsLittleEndian ?
                        new byte[] { 0x34, 0x00 } :
                        new byte[] { 0x00, 0x34 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if the e_phoff is beyond the end of the file. The file length should be 0x1000.
        /// </summary>
        [Test]
        public void ElfHdrPrgHdrOffsetTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x1C, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    stream.DirectWrite(0x20, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if the block partially exceeds the length of the file.
        /// </summary>
        [Test]
        public void ElfHdrPrgHdrBlockTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_phnum = 2. e_phentsize = 0x20. Set e_phoff = 0x1000 - 0x40 + 1 = 0xFC1
                    stream.DirectWrite(0x1C, m_IsLittleEndian ?
                        new byte[] { 0xC1, 0x0F, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x0F, 0xC1 });
                    break;
                case false:
                    // e_phnum = 2. e_phentsize = 0x38. Set e_phoff = 0x1000 - 0x70 + 1 = 0xF91
                    stream.DirectWrite(0x20, m_IsLittleEndian ?
                        new byte[] { 0x91, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x91 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if e_Shentsize is smaller than expected.
        /// </summary>
        [Test]
        public void ElfHdrSectHdrEntSizeTooSmall()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // 32-bit should be 0x28.
                    stream.DirectWrite(0x2E, m_IsLittleEndian ?
                        new byte[] { 0x26, 0x00 } :
                        new byte[] { 0x00, 0x26 });
                    break;
                case false:
                    // 64-bit should be 0x40.
                    stream.DirectWrite(0x3A, m_IsLittleEndian ?
                        new byte[] { 0x3E, 0x00 } :
                        new byte[] { 0x00, 0x3E });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if the e_shoff is beyond the end of the file. The file length should be 0x1000.
        /// </summary>
        [Test]
        public void ElfHdrSectHdrOffsetTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    stream.DirectWrite(0x20, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    stream.DirectWrite(0x28, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests if the block partially exceeds the length of the file.
        /// </summary>
        [Test]
        public void ElfHdrSectHdrBlockTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_shnum = 3. e_shentsize = 0x28. Set e_shoff = 0x1000 - 0x78 + 1 = 0xF89
                    stream.DirectWrite(0x20, m_IsLittleEndian ?
                        new byte[] { 0x89, 0x0F, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x0F, 0x89 });
                    break;
                case false:
                    // e_shnum = 3. e_shentsize = 0x40. Set e_shoff = 0x1000 - 0xC0 + 1 = 0xF41
                    stream.DirectWrite(0x28, m_IsLittleEndian ?
                        new byte[] { 0x41, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x41 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Tests when the string index number is more than the sections available.
        /// </summary>
        [Test]
        public void ElfHdrSectHdrStringIndexTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 2, true, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_shnum = 3
                    stream.DirectWrite(0x32, m_IsLittleEndian ?
                        new byte[] { 0x04, 0x00 } :
                        new byte[] { 0x00, 0x04 });
                    break;
                case false:
                    // e_shnum = 3
                    stream.DirectWrite(0x3E, m_IsLittleEndian ?
                        new byte[] { 0x04, 0x00 } :
                        new byte[] { 0x00, 0x04 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf, Is.Null);
            }
        }

        /// <summary>
        /// Test an invalid PT_DYNAMIC offset.
        /// </summary>
        [Test]
        public void ElfHdrPtDynamicSoNameOffsetTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 32. p_offset = 4.
                    // Offset = 0x80 + 0x20 + 0x04
                    stream.DirectWrite(0xA4, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 56. p_offset = 8.
                    // Offset = 0x80 + 0x38 + 0x08
                    stream.DirectWrite(0xC0, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The PT_DYNAMIC can't be read, so we don't see it's a DLL (we chose no symtab too). So it will
                // actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Test an invalid PT_DYNAMIC offset.
        /// </summary>
        [Test]
        public void ElfHdrPtDynamicSoNameOffsetSizeTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 32. p_offset = 0x10.
                    // Offset = 0x80 + 0x20 + 0x10
                    stream.DirectWrite(0xB0, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 56. p_offset = 0x20.
                    // Offset = 0x80 + 0x38 + 0x20
                    stream.DirectWrite(0xD8, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The PT_DYNAMIC can't be read, so we don't see it's a DLL (we chose no symtab too). So it will
                // actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Test a zero length PT_DYNAMIC.
        /// </summary>
        [Test]
        public void ElfHdrPtDynamicSoNameSizeZero()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 32. p_offset = 0x10.
                    // Offset = 0x80 + 0x20 + 0x10
                    stream.DirectWrite(0xB0, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                case false:
                    // e_phnum = 2 (PT_PHDR, PT_DYNAMIC). PT_DYNAMIC = e_phoff + 56. p_offset = 0x20.
                    // Offset = 0x80 + 0x38 + 0x20
                    stream.DirectWrite(0xD8, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The PT_DYNAMIC has zero length, so we don't know the contents.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests when the string table has an offset that is larger than the file.
        /// </summary>
        [Test]
        public void ElfStringTableOffsetTooLarge1()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x10
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x28;
                    // offset = 0x838
                    stream.DirectWrite(0x838, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x18
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x40;
                    // offset = 0x858
                    stream.DirectWrite(0x858, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests when the string table has an offset that is larger than the file.
        /// </summary>
        [Test]
        public void ElfStringTableOffsetTooLarge2()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x10
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x28;
                    // offset = 0x838
                    stream.DirectWrite(0x838, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                    break;
                case false:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x18
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x40;
                    // offset = 0x858
                    stream.DirectWrite(0x858, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests when the string table has a size that is larger than the file.
        /// </summary>
        [Test]
        public void ElfStringTableSizeTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x14
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x28;
                    // offset = 0x83C
                    stream.DirectWrite(0x83C, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x10, 0x00 });
                    break;
                case false:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x20
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x40;
                    // offset = 0x860
                    stream.DirectWrite(0x860, m_IsLittleEndian ?
                        new byte[] { 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests when the string section is the last part, such that the offset and length is the file length.
        /// </summary>
        [Test]
        public void ElfStringTableSizeExactLength([Values(false, true)] bool overflow)
        {
            // Move the string section to the end of the file.
            IList<SparseBlock> file = GetExeDynElfBlocks(3, 0, 3, false, true, false, false);
            file.Add(new SparseBlock(0x1000, file[1].Data));  // Index 1 is the string section.
            int strlength = file[1].Data.Length;
            file.RemoveAt(1);

            using (SparseStream stream = new SparseStream(file))
            using (BinaryReader br = new BinaryReader(stream)) {
                // Section header 1 (offset 0x800 + e_shentsize) must be patched for the offset of 0x1000.

                switch (m_Is32Bit) {
                case true:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x10
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x28;
                    // offset = 0x838
                    byte[] buf4 = new byte[4];
                    BitOperations.Copy32Shift(0x1000, buf4, 0, m_IsLittleEndian);
                    stream.DirectWrite(0x838, buf4);

                    if (overflow) {
                        // Update the length to be 1 byte too much. Should fail.
                        BitOperations.Copy32Shift(strlength + 1, buf4, 0, m_IsLittleEndian);
                        stream.DirectWrite(0x83C, buf4);
                    }
                    break;
                case false:
                    // String Table = e_shoff + e_shstrndx * e_shentsize + 0x18
                    // e_shoff = 0x800; e_shstrndx = 1; e_shentsize = 0x40;
                    // offset = 0x858
                    byte[] buf8 = new byte[8];
                    BitOperations.Copy64Shift(0x1000, buf8, 0, m_IsLittleEndian);
                    stream.DirectWrite(0x858, buf8);

                    if (overflow) {
                        // Update the length to be 1 byte too much. Should fail.
                        BitOperations.Copy64Shift(strlength + 1, buf8, 0, m_IsLittleEndian);
                        stream.DirectWrite(0x860, buf8);
                    }
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                if (overflow) {
                    // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME
                    // for ET_DYN). So it will actually look like an elf. See the original test results.
                    Assert.That(elf.IsDll, Is.False);
                    Assert.That(elf.IsExe, Is.True);
                    Assert.That(elf.IsCore, Is.False);
                } else {
                    Assert.That(elf.IsDll, Is.True);
                    Assert.That(elf.IsExe, Is.False);
                    Assert.That(elf.IsCore, Is.False);
                }
            }
        }

        /// <summary>
        /// Tests that the string offset for the section for the symtab is too large.
        /// </summary>
        [Test]
        public void ElfStringSymTabNameOffsetTooLarge()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x00
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x28;
                    // offset = 0x850
                    stream.DirectWrite(0x850, m_IsLittleEndian ?
                        new byte[] { 0x13, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x13 });
                    break;
                case false:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x00
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x40;
                    // offset = 0x880
                    stream.DirectWrite(0x880, m_IsLittleEndian ?
                        new byte[] { 0x13, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x13 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests that the string offset for the section for the symtab is too large.
        /// </summary>
        [Test]
        public void ElfStringSymTabNameOffsetZero()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x00
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x28;
                    // offset = 0x850
                    stream.DirectWrite(0x850, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                case false:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x00
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x40;
                    // offset = 0x880
                    stream.DirectWrite(0x880, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        /// <summary>
        /// Tests that the string section type is also checked. Set symtab to SHT_NOBITS
        /// </summary>
        [Test]
        public void ElfStringSymTabCheckType()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x04
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x28;
                    // offset = 0x850
                    stream.DirectWrite(0x854, m_IsLittleEndian ?
                        new byte[] { 0x08, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x08 });
                    break;
                case false:
                    // SymTab section Name = e_shoff + e_shstrndx * e_shentsize + 0x04
                    // e_shoff = 0x800; e_shstrndx = 2; e_shentsize = 0x40;
                    // offset = 0x880
                    stream.DirectWrite(0x884, m_IsLittleEndian ?
                        new byte[] { 0x08, 0x00, 0x00, 0x00 } :
                        new byte[] { 0x00, 0x00, 0x00, 0x08 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // The string table can't be read, so we don't see it's a DLL (we chose symtab, but not an SONAME for
                // ET_DYN). So it will actually look like an elf. See the original test results.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        [Test]
        public void ElfExePtInterpTwice([Values(2, 3)] byte e_type)
        {
            IList<SparseBlock> file = GetInvalidElfPtInterpMulti(3, 0, e_type, false);
            using (SparseStream stream = new SparseStream(file))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // PT_INTERP twice means it's not an elf. But it shouldn't affect a DLL.
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        [Test]
        public void ElfDllPtInterpTwice()
        {
            IList<SparseBlock> file = GetInvalidElfPtInterpMulti(3, 0, 3, true);
            using (SparseStream stream = new SparseStream(file))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // PT_INTERP twice means it's not an elf. But it shouldn't affect a DLL.
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        [Test]
        public void ElfDynExeNoPie()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, true))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.True);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, true, false, true))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // Dynamic Section offset to 0x6ffffffb.
                    stream.DirectWrite(0x604, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                case false:
                    // Dynamic Section offset to 0x6ffffffb.
                    stream.DirectWrite(0x608, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // We're ET_DYN, have no symtab, and no DT_1_PIE isn't present.
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        [Test]
        public void ElfDllSoNameLengthZero()
        {
            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);
                Assert.That(elf.IsDll, Is.True);
                Assert.That(elf.IsExe, Is.False);
            }

            using (SparseStream stream = GetElf(3, 0, 3, false, false, true, false))
            using (BinaryReader br = new BinaryReader(stream)) {
                switch (m_Is32Bit) {
                case true:
                    // Dynamic Section offset to 0x6ffffffb.
                    stream.DirectWrite(0x604, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                case false:
                    // Dynamic Section offset to 0x6ffffffb.
                    stream.DirectWrite(0x608, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                }

                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // We're ET_DYN, have no symtab, and no DT_1_PIE isn't present, no SONAME
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.False);
            }
        }

        [Test]
        public void ElfCoreFile()
        {
            IList<SparseBlock> file = GetCoreElfBlocks(3, 0);
            using (SparseStream stream = new SparseStream(file))
            using (BinaryReader br = new BinaryReader(stream)) {
                UnixElfExecutable elf = UnixElfExecutableAccessor.GetFile(br);

                // We're ET_DYN, have no symtab, and no DT_1_PIE isn't present, no SONAME
                Assert.That(elf.IsDll, Is.False);
                Assert.That(elf.IsExe, Is.False);
                Assert.That(elf.IsCore, Is.True);
            }
        }
    }
}
