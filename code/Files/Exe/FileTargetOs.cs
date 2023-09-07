namespace RJCP.IO.Files.Exe
{
    /// <summary>
    /// A list of target OS for executable files.
    /// </summary>
    public enum FileTargetOs
    {
        /// <summary>
        /// The target OS ABI is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The target OS ABI is Windows.
        /// </summary>
        Windows,

        /// <summary>
        /// The target OS ABI is System-V ABI.
        /// </summary>
        SysV,

        /// <summary>
        /// The target OS ABI is Linux.
        /// </summary>
        Linux,

        /// <summary>
        /// The target OS ABI is Solaris.
        /// </summary>
        Solaris,

        /// <summary>
        /// The target OS ABI is NetBSD.
        /// </summary>
        NetBSD,

        /// <summary>
        /// The target OS ABI is FreeBSD.
        /// </summary>
        FreeBSD,

        /// <summary>
        /// The target OS ABI is OpenBSD.
        /// </summary>
        OpenBSD,

        /// <summary>
        /// The target OS ABI is HP Unix.
        /// </summary>
        HPUX,

        /// <summary>
        /// The target OS ABI is Arm.
        /// </summary>
        Arm
    }
}
