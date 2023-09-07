namespace RJCP.IO.Files.Exe.ElfGen
{
    using RJCP.Core;

    public class ElfHdr
    {
        public ElfHdr()
        {
            // A more suitable design would be to use a collection of these objects, but for testing, we need more
            // control (and injection of errors), so the process is more manual.
            ProgramHeader = new ElfPHdr(this);
            SectionHeader = new ElfSHdr(this);
        }

        public bool IsLittleEndian { get; set; } = true;

        public int WordSize { get; set; } = 32;

        public byte Abi { get; set; }

        public byte AbiVersion { get; set; }

        public ushort ObjectType { get; set; } = 2;

        public ushort MachineArch { get; set; } = 3;

        public uint Flags { get; set; }

        public ulong EntryAddr { get; set; }

        public ulong PrgHdrOffset { get; set; }

        public short PrgHdrRecordSize { get; set; }

        public int PrgHdrRecords { get; set; }

        public ulong SectHdrOffset { get; set; }

        public short SectHdrRecordSize { get; set; }

        public int SectHdrRecords { get; set; }

        public int SectHdrStringIndex { get; set; }

        public ElfPHdr ProgramHeader { get; }

        public ElfSHdr SectionHeader { get; }

        public byte[] GenerateFileHeader()
        {
            switch (WordSize) {
            case 32: return GenerateFileHeader32();
            case 64: return GenerateFileHeader64();
            default: return null;
            }
        }

        private byte[] GenerateFileHeader32()
        {
            byte[] elfBuffer = new byte[52];
            elfBuffer[0] = 0x7F;
            elfBuffer[1] = (byte)'E';
            elfBuffer[2] = (byte)'L';
            elfBuffer[3] = (byte)'F';
            elfBuffer[4] = 1;               // 32-bit
            elfBuffer[5] = IsLittleEndian ? (byte)1 : (byte)2;
            elfBuffer[6] = 1;               // Version
            elfBuffer[7] = Abi;
            elfBuffer[8] = AbiVersion;

            short prgRecSize = (short)(PrgHdrRecordSize == 0 ? 32 : PrgHdrRecordSize);
            short sectRecSize = (short)(SectHdrRecordSize == 0 ? 40 : SectHdrRecordSize);
            int prgHdrOffset = PrgHdrOffset == 0 ? 52 : unchecked((int)(PrgHdrOffset & 0xFFFFFFFF));
            PrgHdrOffset = (ulong)prgHdrOffset; // Update that it can be referenced
            int secHdrOffset = SectHdrOffset == 0 ? prgHdrOffset + prgRecSize : unchecked((int)(SectHdrOffset & 0xFFFFFFFF));
            SectHdrOffset = (ulong)secHdrOffset; // Update that it can be referenced

            BitOperations.Copy16Shift(ObjectType, elfBuffer, 16, IsLittleEndian);
            BitOperations.Copy16Shift(MachineArch, elfBuffer, 18, IsLittleEndian);
            BitOperations.Copy32Shift(1, elfBuffer, 20, IsLittleEndian);
            BitOperations.Copy32Shift((uint)(EntryAddr & 0xFFFFFFFF), elfBuffer, 24, IsLittleEndian);
            BitOperations.Copy32Shift(prgHdrOffset, elfBuffer, 28, IsLittleEndian);
            BitOperations.Copy32Shift(secHdrOffset, elfBuffer, 32, IsLittleEndian);
            BitOperations.Copy32Shift(Flags, elfBuffer, 36, IsLittleEndian);
            BitOperations.Copy16Shift(52, elfBuffer, 40, IsLittleEndian);  // Size of the Program Header
            BitOperations.Copy16Shift(prgRecSize, elfBuffer, 42, IsLittleEndian);
            BitOperations.Copy16Shift(PrgHdrRecords, elfBuffer, 44, IsLittleEndian);
            BitOperations.Copy16Shift(sectRecSize, elfBuffer, 46, IsLittleEndian);
            BitOperations.Copy16Shift(SectHdrRecords, elfBuffer, 48, IsLittleEndian);
            BitOperations.Copy16Shift(SectHdrStringIndex, elfBuffer, 50, IsLittleEndian);
            return elfBuffer;
        }

        private byte[] GenerateFileHeader64()
        {
            byte[] elfBuffer = new byte[64];
            elfBuffer[0] = 0x7F;
            elfBuffer[1] = (byte)'E';
            elfBuffer[2] = (byte)'L';
            elfBuffer[3] = (byte)'F';
            elfBuffer[4] = 2;               // 64-bit
            elfBuffer[5] = IsLittleEndian ? (byte)1 : (byte)2;
            elfBuffer[6] = 1;               // Version
            elfBuffer[7] = Abi;
            elfBuffer[8] = AbiVersion;

            short prgRecSize = (short)(PrgHdrRecordSize == 0 ? 56 : PrgHdrRecordSize);
            short sectRecSize = (short)(SectHdrRecordSize == 0 ? 64 : SectHdrRecordSize);
            long prgHdrOffset = PrgHdrOffset == 0 ? 64 : unchecked((long)PrgHdrOffset);
            PrgHdrOffset = unchecked((ulong)prgHdrOffset); // Update that it can be referenced
            long secHdrOffset = SectHdrOffset == 0 ? prgHdrOffset + prgRecSize : unchecked((long)SectHdrOffset);
            SectHdrOffset = unchecked((ulong)secHdrOffset); // Update that it can be referenced

            BitOperations.Copy16Shift(ObjectType, elfBuffer, 16, IsLittleEndian);
            BitOperations.Copy16Shift(MachineArch, elfBuffer, 18, IsLittleEndian);
            BitOperations.Copy32Shift(1, elfBuffer, 20, IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)EntryAddr), elfBuffer, 24, IsLittleEndian);
            BitOperations.Copy64Shift(prgHdrOffset, elfBuffer, 32, IsLittleEndian);
            BitOperations.Copy64Shift(secHdrOffset, elfBuffer, 40, IsLittleEndian);
            BitOperations.Copy32Shift(Flags, elfBuffer, 48, IsLittleEndian);
            BitOperations.Copy16Shift(64, elfBuffer, 52, IsLittleEndian);  // Size of the Program Header
            BitOperations.Copy16Shift(prgRecSize, elfBuffer, 54, IsLittleEndian);
            BitOperations.Copy16Shift(PrgHdrRecords, elfBuffer, 56, IsLittleEndian);
            BitOperations.Copy16Shift(sectRecSize, elfBuffer, 58, IsLittleEndian);
            BitOperations.Copy16Shift(SectHdrRecords, elfBuffer, 60, IsLittleEndian);
            BitOperations.Copy16Shift(SectHdrStringIndex, elfBuffer, 62, IsLittleEndian);
            return elfBuffer;
        }
    }
}
