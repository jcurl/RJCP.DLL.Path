namespace RJCP.IO.Files.Exe.ElfGen
{
    public class StringSectionEntry
    {
        internal StringSectionEntry(int offset, string value)
        {
            Offset = offset;
            Value = value;
        }

        public int Offset { get; }

        public string Value { get; }
    }
}
