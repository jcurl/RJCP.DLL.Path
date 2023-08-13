namespace RJCP.IO
{
    /// <summary>
    /// The type of <see cref="FileSystemNodeInfo"/>.
    /// </summary>
    public enum NodeInfoType
    {
        /// <summary>
        /// No file system information available.
        /// </summary>
        None,

        /// <summary>
        /// Using Windows Extended information.
        /// </summary>
        WindowsExtended,

        /// <summary>
        /// Using Windows Default information.
        /// </summary>
        WindowsFileInfo,

        /// <summary>
        /// Using Mono.Unix or Mono runtime.
        /// </summary>
        MonoUnix,

        /// <summary>
        /// Using default information.
        /// </summary>
        Other
    }
}
