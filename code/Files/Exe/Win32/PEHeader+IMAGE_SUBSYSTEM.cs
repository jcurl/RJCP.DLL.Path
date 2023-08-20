namespace RJCP.IO.Files.Exe.Win32
{
    internal static partial class PEHeader
    {
        /// <summary>
        /// The Subsystem field of the optional header determine which Windows subsystem (if any) is required to run the
        /// image.
        /// </summary>
        public enum IMAGE_SUBSYSTEM : ushort
        {
            /// <summary>
            /// An unknown subsystem.
            /// </summary>
            UNKNOWN = 0,

            /// <summary>
            /// Device drivers and native Windows processes.
            /// </summary>
            NATIVE = 1,

            /// <summary>
            /// The Windows graphical user interface (GUI) subsystem.
            /// </summary>
            WINDOWS_GUI = 2,

            /// <summary>
            /// The Windows character subsystem.
            /// </summary>
            WINDOWS_CUI = 3,

            /// <summary>
            /// Old Windows CE subsystem.
            /// </summary>
            WINDOWS_CE_OLD = 4,

            /// <summary>
            /// The OS/2 character subsystem.
            /// </summary>
            OS2_CUI = 5,

            /// <summary>
            /// The Posix character subsystem.
            /// </summary>
            POSIX_CUI = 7,

            /// <summary>
            /// Native Win9x driver.
            /// </summary>
            NATIVE_WINDOWS = 8,

            /// <summary>
            /// Windows CE.
            /// </summary>
            WINDOWS_CE_GUI = 9,

            /// <summary>
            /// An Extensible Firmware Interface (EFI) application.
            /// </summary>
            EFI_APPLICATION = 10,

            /// <summary>
            /// An EFI driver with boot services.
            /// </summary>
            EFI_BOOT_SERVICE_DRIVER = 11,

            /// <summary>
            /// An EFI driver with run-time services.
            /// </summary>
            EFI_RUNTIME_DRIVER = 12,

            /// <summary>
            /// An EFI ROM image.
            /// </summary>
            EFI_ROM = 13,

            /// <summary>
            /// XBOX.
            /// </summary>
            XBOX = 14,

            /// <summary>
            /// Windows boot application.
            /// </summary>
            WINDOWS_BOOT_APPLICATION = 16
        }
    }
}
