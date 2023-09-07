namespace RJCP.IO.Files.Exe.ElfGen
{
    using System;
    using RJCP.Core;

    public class ElfPHdr
    {
        private readonly ElfHdr m_Hdr;

        internal ElfPHdr(ElfHdr hdr)
        {
            if (hdr == null) throw new ArgumentNullException(nameof(hdr));
            m_Hdr = hdr;
            Reset();
        }

        private void Reset()
        {
            m_HdrType = 0;
            Flags = 0;
            Offset = 0;
            VirtualAddress = 0;
            PhysicalAddress = 0;
            FileSize = 0;
            MemSegmentSize = 0;
            Align = 0x100;
        }

        private int m_HdrType;

        public int HdrType
        {
            get { return m_HdrType; }
            set
            {
                Reset();
                m_HdrType = value;
            }
        }

        public int Flags { get; set; }

        public ulong Offset { get; set; }

        public ulong VirtualAddress { get; set; }

        public ulong PhysicalAddress { get; set; }

        public ulong FileSize { get; set; }

        public ulong MemSegmentSize { get; set; }

        public ulong Align { get; set; } = 0x1000;

        public ulong GetIndexOffset(int index)
        {
            switch (m_Hdr.WordSize) {
            case 32: return m_Hdr.PrgHdrOffset + (ulong)(32 * index);
            case 64: return m_Hdr.PrgHdrOffset + (ulong)(56 * index);
            default: return 0;
            }
        }

        public byte[] GenerateProgramHeader()
        {
            switch (m_Hdr.WordSize) {
            case 32: return GenerateProgramHeader32();
            case 64: return GenerateProgramHeader64();
            default: return null;
            }
        }

        private byte[] GenerateProgramHeader32()
        {
            byte[] buffer = new byte[32];
            BitOperations.Copy32Shift(HdrType, buffer, 0, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Offset & 0xFFFFFFFF), buffer, 4, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(VirtualAddress & 0xFFFFFFFF), buffer, 8, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(PhysicalAddress & 0xFFFFFFFF), buffer, 12, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(FileSize & 0xFFFFFFFF), buffer, 16, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(MemSegmentSize & 0xFFFFFFFF), buffer, 20, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Flags, buffer, 24, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Align & 0xFFFFFFFF), buffer, 28, m_Hdr.IsLittleEndian);
            return buffer;
        }

        private byte[] GenerateProgramHeader64()
        {
            byte[] buffer = new byte[56];
            BitOperations.Copy32Shift(HdrType, buffer, 0, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Flags, buffer, 4, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Offset), buffer, 8, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)VirtualAddress), buffer, 16, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)PhysicalAddress), buffer, 24, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)FileSize), buffer, 32, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)MemSegmentSize), buffer, 40, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Align), buffer, 48, m_Hdr.IsLittleEndian);
            return buffer;
        }

        public int DynHeaderLength
        {
            get
            {
                switch (m_Hdr.WordSize) {
                case 32: return 8;
                case 64: return 16;
                default: return 0;
                }
            }
        }

        public byte[] GetDynHeader(long tag, ulong val)
        {
            byte[] buffer;
            switch (m_Hdr.WordSize) {
            case 32:
                buffer = new byte[8];
                BitOperations.Copy32Shift(tag, buffer, 0, m_Hdr.IsLittleEndian);
                BitOperations.Copy32Shift((int)val, buffer, 4, m_Hdr.IsLittleEndian);
                return buffer;
            case 64:
                buffer = new byte[16];
                BitOperations.Copy64Shift(tag, buffer, 0, m_Hdr.IsLittleEndian);
                BitOperations.Copy64Shift(unchecked((long)val), buffer, 8, m_Hdr.IsLittleEndian);
                return buffer;
            default: return null;
            }
        }
    }
}
