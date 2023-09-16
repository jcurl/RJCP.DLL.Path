namespace RJCP.IO.FileSystem
{
    using System;

    /// <summary>
    /// Get file information for all operating systems.
    /// </summary>
    /// <remarks>
    /// This class is generic and can only compare the name of the file, not the what the file actually points to. File
    /// names are not normalised prior.
    /// </remarks>
    internal sealed class GenericNodeInfo : NodeInfo<GenericNodeInfo>
    {
        private readonly StringComparison m_CaseSensitive;
        private readonly int m_HashCode;

        public GenericNodeInfo(string path, bool caseSensitive)
        {
            if (!System.IO.Path.IsPathRooted(path)) {
                Path = IO.Path.ToPath(Environment.CurrentDirectory).Append(path).ToString();
            } else {
                Path = path;
            }
            if (caseSensitive) {
                m_CaseSensitive = StringComparison.InvariantCulture;
                m_HashCode = Path.ToUpper().GetHashCode();
            } else {
                m_CaseSensitive = StringComparison.InvariantCultureIgnoreCase;
                m_HashCode = Path.GetHashCode();
            }
        }

        public override NodeInfoType Type
        {
            get { return NodeInfoType.Other; }
        }

        public override string LinkTarget { get; }

        public override string Path { get; }

        protected override bool Equals(GenericNodeInfo other)
        {
            return Path.Equals(other.Path, m_CaseSensitive);
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }
    }
}
