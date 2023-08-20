namespace RJCP.IO.Files.Exe.Win32
{
    using System;

    internal static partial class PEHeader
    {
        /// <summary>
        /// The following values are defined for the DllCharacteristics field of the optional header.
        /// </summary>
        [Flags]
        public enum IMAGE_DLLCHARACTERISTICS : ushort
        {
            /// <summary>
            /// Reserved. Borland Linker.
            /// </summary>
            IMAGE_LIBRARY_PROCESS_INIT = 0x0001,

            /// <summary>
            /// Reserved. Borland Linker.
            /// </summary>
            IMAGE_LIBRARY_PROCESS_TERM = 0x0002,

            /// <summary>
            /// Reserved. Borland Linker.
            /// </summary>
            IMAGE_LIBRARY_THREAD_INIT = 0x0004,

            /// <summary>
            /// Reserved. Borland Linker.
            /// </summary>
            IMAGE_LIBRARY_THREAD_TERM = 0x0008,

            /// <summary>
            /// Image can handle a high entropy 64-bit virtual address space.
            /// </summary>
            HIGH_ENTROPY_VA = 0x0020,

            /// <summary>
            /// DLL can be relocated at load time.
            /// </summary>
            DYNAMIC_BASE = 0x0040,

            /// <summary>
            /// Code Integrity checks are enforced.
            /// </summary>
            FORCE_INTEGRITY = 0x0080,

            /// <summary>
            /// Image is NX compatible.
            /// </summary>
            NX_COMPAT = 0x0100,

            /// <summary>
            /// Isolation aware, but do not isolate the image.
            /// </summary>
            NO_ISOLATION = 0x0200,

            /// <summary>
            /// Does not use structured exception (SE) handling. No SE handler may be called in this image.
            /// </summary>
            NO_SEH = 0x0400,

            /// <summary>
            /// Do not bind the image.
            /// </summary>
            NO_BIND = 0x0800,

            /// <summary>
            /// Image must execute in an AppContainer.
            /// </summary>
            APPCONTAINER = 0x1000,

            /// <summary>
            /// A WDM driver
            /// </summary>
            WDM_DRIVER = 0x2000,

            /// <summary>
            /// Image supports Control Flow Guard.
            /// </summary>
            GUARD_CF = 0x4000,

            /// <summary>
            /// Terminal Server aware.
            /// </summary>
            TERMINAL_SERVER_AWARE = 0x8000
        }
    }
}
