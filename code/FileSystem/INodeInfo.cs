namespace RJCP.IO.FileSystem
{
    using System;

    internal interface INodeInfo : IEquatable<INodeInfo>
    {
        NodeInfoType Type { get; }

        string LinkTarget { get; }

        string Path { get; }
    }
}
