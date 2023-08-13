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
        private readonly string m_Path;
        private readonly StringComparison m_CaseSensitive;
        private readonly int m_HashCode;

        public GenericNodeInfo(string path, bool caseSensitive)
        {
            m_Path = path;
            m_CaseSensitive = caseSensitive ?
                StringComparison.InvariantCulture :
                StringComparison.InvariantCultureIgnoreCase;
            m_HashCode = path.GetHashCode();
        }

        public override NodeInfoType Type
        {
            get { return NodeInfoType.Other; }
        }

        public override string LinkTarget { get; }

        protected override bool Equals(GenericNodeInfo other)
        {
            return m_Path.Equals(other.m_Path, m_CaseSensitive);
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }
    }
}
