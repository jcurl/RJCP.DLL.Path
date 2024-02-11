namespace RJCP.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using FileSystem;
    using RJCP.Core.Environment;

    /// <summary>
    /// Obtain detailed file information.
    /// </summary>
    /// <remarks>
    /// This class obtains further information from the file system for the Operating System that can be used to help
    /// identify the file precisely. Under windows, reparse points (softlinks and directory junctions) are considered
    /// different files. You'll need to get the reparse point and iterate to find the result file and compare that.
    /// </remarks>
    public sealed class FileSystemNodeInfo : IEquatable<FileSystemNodeInfo>
    {
        private readonly INodeInfo m_NodeInfo;

        /// <summary>
        /// Get detailed file information with the <see cref="FileSystemNodeInfo"/> class.
        /// </summary>
        /// <param name="path">The path to the file to get information for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="path"/> is not found.</exception>
        /// <exception cref="PlatformNotSupportedException">Only Windows and Linux are supported.</exception>
        /// <remarks>
        /// This constructor will automatically resolve link targets if available. The <see cref="LinkTarget"/> is
        /// the name of the file after resolution.
        /// </remarks>
        public FileSystemNodeInfo(Path path) : this(path?.ToString(), true) { }

        /// <summary>
        /// Get detailed file information with the <see cref="FileSystemNodeInfo"/> class.
        /// </summary>
        /// <param name="path">The path to the file to get information for.</param>
        /// <param name="resolveLink">
        /// Set to <see langword="true"/> to resolve the link to the target. <see langword="false"/> to treat as its own
        /// file.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="path"/> is not found.</exception>
        /// <exception cref="PlatformNotSupportedException">Only Windows and Linux are supported.</exception>
        /// <remarks>
        /// The <see cref="LinkTarget"/> is the name of the final file if <paramref name="resolveLink"/> is <see langword="true"/>,
        /// else it is the link pointed to this path if it's a symbolic link (which may be another symbolic link).
        /// </remarks>
        public FileSystemNodeInfo(Path path, bool resolveLink) : this(path?.ToString(), resolveLink) { }

        /// <summary>
        /// Get detailed file information with the <see cref="FileSystemNodeInfo"/> class.
        /// </summary>
        /// <param name="path">The path to the file to get information for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="path"/> is not found.</exception>
        /// <exception cref="PlatformNotSupportedException">Only Windows and Linux are supported.</exception>
        /// <remarks>
        /// This constructor will automatically resolve link targets if available. The <see cref="LinkTarget"/> is
        /// the name of the file after resolution.
        /// </remarks>
        public FileSystemNodeInfo(string path) : this(path, true) { }

        /// <summary>
        /// Get detailed file information with the <see cref="FileSystemNodeInfo"/> class.
        /// </summary>
        /// <param name="path">The path to the file to get information for.</param>
        /// <param name="resolveLink">
        /// Set to <see langword="true"/> to resolve the link to the target. <see langword="false"/> to treat as its own
        /// file.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="path"/> is not found.</exception>
        /// <exception cref="PlatformNotSupportedException">Only Windows and Linux are supported.</exception>
        /// <remarks>
        /// The <see cref="LinkTarget"/> is the name of the final file if <paramref name="resolveLink"/> is <see langword="true"/>,
        /// else it is the link pointed to this path if it's a symbolic link (which may be another symbolic link).
        /// </remarks>
        public FileSystemNodeInfo(string path, bool resolveLink)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Empty path", nameof(path));

            if (!File.Exists(path) && !Directory.Exists(path))
                throw new FileNotFoundException($"Path '{path}' not found");

            if (Platform.IsWinNT()) {
                m_NodeInfo = new Win32NodeInfo(path, resolveLink);
            } else if (Platform.IsUnix()) {
                m_NodeInfo = new MonoUnixNodeInfo(path, resolveLink);
            } else {
                throw new PlatformNotSupportedException();
            }

            if (m_NodeInfo.Type != NodeInfoType.None) {
                LinkTarget = m_NodeInfo.LinkTarget;
            } else {
                m_NodeInfo = new GenericNodeInfo(path, false);
            }
            Path = m_NodeInfo.Path ?? path;
        }

        /// <summary>
        /// The fully qualified path for the file being queried.
        /// </summary>
        /// <value>The fully qualified path for the file being queried.</value>
        public string Path { get; }

        /// <summary>
        /// If this is a reparse point, get the path of the target.
        /// </summary>
        /// <value>The link target path.</value>
        /// <remarks>
        /// If this value is <see langword="null"/> or empty, then this is not a reparse point, or it is not known.
        /// Otherwise, this is the full path to the file (it may not be the canonical path, but is the one resolved
        /// through parsing links). On Linux, if a part of the path is a symbolic link, then this is not resolved,
        /// only the final path.
        /// </remarks>
        public string LinkTarget { get; }

        /// <summary>
        /// Gets the type of the information obtained.
        /// </summary>
        /// <value>The type of information obtained.</value>
        /// <remarks>This can help with a description of how reliable the comparison between objects can be.</remarks>
        public NodeInfoType Type
        {
            get { return m_NodeInfo.Type; }
        }

        /// <summary>
        /// Gets the extended information for the file. This is Operating System specific.
        /// </summary>
        /// <value>
        /// The extended file information that is available for the file. The value returned may be
        /// <see langword="null"/> if there is no further information.
        /// </value>
        public IFileSystemExtended Extended { get { return m_NodeInfo.Extended; } }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (!(obj is FileSystemNodeInfo fileInfo)) return false;
            return Equals(fileInfo);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(FileSystemNodeInfo other)
        {
            if (other == null) return false;

            // The NodeInfoType.None should never be set.
            CheckType();
            other.CheckType();

            if (Type != other.Type) return false;
            return m_NodeInfo.Equals(other.m_NodeInfo);
        }

        [Conditional("DEBUG")]
        private void CheckType()
        {
            if (Type == NodeInfoType.None)
                throw new InvalidOperationException("Internal error, NodeInfoType may not be None");
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return m_NodeInfo.GetHashCode();
        }

        /// <summary>
        /// Implements the equality operator.
        /// </summary>
        /// <param name="info1">The first object being compared.</param>
        /// <param name="info2">The second object being compared.</param>
        /// <returns>The result of the operator, if the two objects are equal.</returns>
        public static bool operator ==(FileSystemNodeInfo info1, FileSystemNodeInfo info2)
        {
            if (((object)info1) == null || ((object)info2) == null)
                return object.Equals(info1, info2);

            return info1.Equals(info2);
        }

        /// <summary>
        /// Implements the inequality operator.
        /// </summary>
        /// <param name="info1">The first object being compared.</param>
        /// <param name="info2">The second object being compared.</param>
        /// <returns>The result of the operator, if the two objects are not equal.</returns>
        public static bool operator !=(FileSystemNodeInfo info1, FileSystemNodeInfo info2)
        {
            if (((object)info1) == null || ((object)info2) == null)
                return !Equals(info1, info2);

            return !info1.Equals(info2);
        }
    }
}
