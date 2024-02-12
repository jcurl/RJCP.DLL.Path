namespace RJCP.IO.Files.Exe.ElfGen
{
    using System;
    using RJCP.Core;

    public class ElfSHdr
    {
        private readonly ElfHdr m_Hdr;

        internal ElfSHdr(ElfHdr hdr)
        {
            ThrowHelper.ThrowIfNull(hdr);
            m_Hdr = hdr;
            Reset();
        }

        private void Reset()
        {
            m_Name = 0;
            HdrType = 0;
            Flags = 0;
            VirtualAddress = 0;
            Offset = 0;
            Size = 0;
            Link = 0;
            Info = 0;
            Align = 0x100;
            EntrySize = 0;
        }

        private int m_Name;

        public int Name
        {
            get { return m_Name; }
            set
            {
                Reset();
                m_Name = value;
            }
        }

        public int HdrType { get; set; }

        public ulong Flags { get; set; }

        public ulong VirtualAddress { get; set; }

        public ulong Offset { get; set; }

        public ulong Size { get; set; }

        public uint Link { get; set; }

        public uint Info { get; set; }

        public ulong Align { get; set; }

        public ulong EntrySize { get; set; }

        public ulong GetIndexOffset(int index)
        {
            switch (m_Hdr.WordSize) {
            case 32: return m_Hdr.SectHdrOffset + (ulong)(40 * index);
            case 64: return m_Hdr.SectHdrOffset + (ulong)(64 * index);
            default: return 0;
            }
        }

        public byte[] GenerateSectionHeader()
        {
            switch (m_Hdr.WordSize) {
            case 32: return GenerateSectionHeader32();
            case 64: return GenerateSectionHeader64();
            default: return null;
            }
        }

        private byte[] GenerateSectionHeader32()
        {
            byte[] buffer = new byte[40];
            BitOperations.Copy32Shift(Name, buffer, 0, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(HdrType, buffer, 4, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Flags & 0xFFFFFFFF), buffer, 8, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(VirtualAddress & 0xFFFFFFFF), buffer, 12, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Offset & 0xFFFFFFFF), buffer, 16, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Size & 0xFFFFFFFF), buffer, 20, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Link, buffer, 24, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Info, buffer, 28, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(Align & 0xFFFFFFFF), buffer, 32, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift((uint)(EntrySize & 0xFFFFFFFF), buffer, 36, m_Hdr.IsLittleEndian);
            return buffer;
        }

        private byte[] GenerateSectionHeader64()
        {
            byte[] buffer = new byte[64];
            BitOperations.Copy32Shift(Name, buffer, 0, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(HdrType, buffer, 4, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Flags), buffer, 8, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)VirtualAddress), buffer, 16, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Offset), buffer, 24, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Size), buffer, 32, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Link, buffer, 40, m_Hdr.IsLittleEndian);
            BitOperations.Copy32Shift(Info, buffer, 44, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)Align), buffer, 48, m_Hdr.IsLittleEndian);
            BitOperations.Copy64Shift(unchecked((long)EntrySize), buffer, 56, m_Hdr.IsLittleEndian);
            return buffer;
        }
    }
}
