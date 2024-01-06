namespace RJCP.IO.FileSystem
{
    /// <summary>
    /// Extended information from the Windows File System Driver.
    /// </summary>
    /// <remarks>
    /// The parameters that make up <see cref="FileSystemNodeInfo.GetHashCode()"/> for testing equality of two files
    /// are:
    /// <list type="bullet">
    /// <item><see cref="VolumeSerialNumber"/>;</item>
    /// <item><see cref="FileIdHigh"/>; and</item>
    /// <item><see cref="FileIdLow"/>.</item>
    /// </list>
    /// </remarks>
    public class Win32Extended : IFileSystemExtended
    {
        internal Win32Extended() { }

        /// <summary>
        /// Gets the volume serial number where the file resides.
        /// </summary>
        /// <value>The volume serial number where the file resides.</value>
        /// <remarks>
        /// Windows Vista and later provides extended information.
        /// </remarks>
        public long VolumeSerialNumber { get; internal set; }

        /// <summary>
        /// Gets the low 64-bit identifier for the file.
        /// </summary>
        /// <value>The low 64-bit identifier file the file.</value>
        public long FileIdLow { get; internal set; }

        /// <summary>
        /// Gets the high 64-bit identifier for the file.
        /// </summary>
        /// <value>The high 64-bit identifier file the file.</value>
        public long FileIdHigh { get; internal set; }
    }
}
