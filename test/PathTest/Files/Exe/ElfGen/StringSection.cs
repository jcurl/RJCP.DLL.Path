namespace RJCP.IO.Files.Exe.ElfGen
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StringSection
    {
        internal StringSection() { }

        public List<string> Strings { get; } = new List<string>();

        public byte[] Section { get; private set; }

        public List<StringSectionEntry> GenerateSection()
        {
            List<byte[]> data = new List<byte[]>();
            List<StringSectionEntry> entries = new List<StringSectionEntry>();

            // Encode the strings and calculate the offsets in the buffer. At the end, `offset` is the total length
            // needed.
            int offset = 0;
            foreach (string value in Strings) {
                byte[] enc = Encoding.UTF8.GetBytes(value);
                StringSectionEntry entry = new StringSectionEntry(offset, value ?? string.Empty);
                data.Add(enc);
                entries.Add(entry);
                offset += enc.Length + 1;
            }

            Section = new byte[offset];
            offset = 0;
            foreach (byte[] enc in data) {
                if (enc.Length > 0) {
                    Buffer.BlockCopy(enc, 0, Section, offset, enc.Length);
                }
                Section[offset + enc.Length] = 0;
                offset += enc.Length + 1;
            }
            return entries;
        }
    }
}
