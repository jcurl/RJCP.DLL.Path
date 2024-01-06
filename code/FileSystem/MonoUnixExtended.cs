namespace RJCP.IO.FileSystem
{
    /// <summary>
    /// Extended information obtained from Mono.
    /// </summary>
    /// <remarks>
    /// The parameters that make up <see cref="FileSystemNodeInfo.GetHashCode()"/> for testing equality of two files
    /// are:
    /// <list type="bullet">
    /// <item><see cref="Device"/>;</item>
    /// <item><see cref="DeviceType"/>; and</item>
    /// <item><see cref="Inode"/>.</item>
    /// </list>
    /// </remarks>
    public class MonoUnixExtended : IFileSystemExtended
    {
        internal MonoUnixExtended() { }

        /// <summary>
        /// Gets the identifier of device on which this file resides, equivalent to <c>st_dev</c>.
        /// </summary>
        /// <value>The identifier of device on which this file resides.</value>
        public long Device { get; internal set; }

        /// <summary>
        /// Gets the Device identifier (if special file), equivalent to <c>st_rdev</c>.
        /// </summary>
        /// <value>The device identifier (if special file).</value>
        /// <remarks>
        /// On Linux, this contains the major and minor number of the node, usually created with <c>mknod</c>.
        /// </remarks>
        public long DeviceType { get; internal set; }

        /// <summary>
        /// Gets the inode number on the file system.
        /// </summary>
        /// <value>The inode number on the file system.</value>
        public long Inode { get; internal set; }

        /// <summary>
        /// Gets the mode for the file, equivalent to <c>st_mod</c>.
        /// </summary>
        /// <value>The mode for the file.</value>
        /// <remarks>
        /// This bit field provides the following information:
        /// <list type="bullet">
        /// <item>S_ISREG - A regular file;</item>
        /// <item>S_ISDIR - A directory;</item>
        /// <item>S_ISCHR - A character device;</item>
        /// <item>S_ISBLK - A block device;</item>
        /// <item>S_ISFIFO - A named pipe;</item>
        /// <item>S_ISLNK - A symbolic link;</item>
        /// <item>S_ISSOCK - A socket.</item>
        /// </list>
        /// </remarks>
        public int Mode { get; internal set; }

        /// <summary>
        /// Gets the user identifier for the file.
        /// </summary>
        /// <value>The user identifier for the file.</value>
        public long UserId { get; internal set; }

        /// <summary>
        /// Gets the group identifier for the file.
        /// </summary>
        /// <value>The group identifier for the file.</value>
        public long GroupId { get; internal set; }
    }
}
