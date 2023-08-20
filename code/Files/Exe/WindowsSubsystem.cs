namespace RJCP.IO.Files.Exe
{
    /// <summary>
    /// The detected Windows Subsystem.
    /// </summary>
    public enum WindowsSubsystem
    {
        /// <summary>
        /// Runs using the Windows Console subsystem.
        /// </summary>
        WindowsConsole,

        /// <summary>
        /// Runs using the Windows GUI subsystem.
        /// </summary>
        WindowsGui,

        /// <summary>
        /// A Native Windows driver (.SYS).
        /// </summary>
        WindowsNativeDriverSys,

        /// <summary>
        /// Windows Posix Subsystem.
        /// </summary>
        ServicesForUnix,
    }
}
