namespace RJCP.IO.Files.Exe
{
    using System;
    using System.IO;
    using Win32;

    /// <summary>
    /// A Windows <see cref="FileExecutable"/>.
    /// </summary>
    public sealed class WindowsExecutable : FileExecutable
    {
        /// <summary>
        /// Gets information from the Windows Portable Executable file.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> to the file to open and read.</param>
        /// <returns>
        /// A <see cref="WindowsExecutable"/> object with read information. Returns <see langword="null"/> if the file
        /// cannot be interpreted.
        /// </returns>
        internal static WindowsExecutable GetFile(BinaryReader br)
        {
            try {
                PEHeader.IMAGE_DOS_HEADER dosHeader = br.ReadStruct<PEHeader.IMAGE_DOS_HEADER>();
                if (dosHeader.e_magic != 0x5a4d) return null;
                if (dosHeader.e_lfanew >= br.BaseStream.Length) return null;

                br.BaseStream.Position = dosHeader.e_lfanew;
                uint ntHeaderSignature = br.ReadUInt32();
                if (ntHeaderSignature != 0x00004550) return null;          // PE\0\0

                PEHeader.IMAGE_FILE_HEADER fileHeader = br.ReadStruct<PEHeader.IMAGE_FILE_HEADER>();
                if (fileHeader.SizeOfOptionalHeader == 0) return null;
                if ((fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.EXECUTABLE_IMAGE) == 0) return null;

                WindowsExecutable winExe = new();

                // Some implementations use the bit-size in the File Characteristics for the PE32 or PE32+ decision.
                // That is wrong, it must be based on the magic number.
                PEHeader.IMAGE_SUBSYSTEM subsystem;
                ushort imgOptionalMagic = br.PeekUInt16();
                switch (imgOptionalMagic) {
                case PEHeader.IMAGE_NT_OPTIONAL_HDR32_MAGIC:
                    PEHeader.IMAGE_OPTIONAL_HEADER32 hdr32 = br.ReadStruct<PEHeader.IMAGE_OPTIONAL_HEADER32>();
                    subsystem = hdr32.Subsystem;
                    winExe.m_OSVersion = new Version(hdr32.MajorOperatingSystemVersion, hdr32.MinorOperatingSystemVersion);
                    winExe.m_ImageVersion = new Version(hdr32.MajorImageVersion, hdr32.MinorImageVersion);
                    winExe.m_SubsystemVersion = new Version(hdr32.MajorSubsystemVersion, hdr32.MinorSubsystemVersion);
                    winExe.m_ArchitectureSize = 32;
                    break;
                case PEHeader.IMAGE_NT_OPTIONAL_HDR64_MAGIC:
                    PEHeader.IMAGE_OPTIONAL_HEADER64 hdr64 = br.ReadStruct<PEHeader.IMAGE_OPTIONAL_HEADER64>();
                    subsystem = hdr64.Subsystem;
                    winExe.m_OSVersion = new Version(hdr64.MajorOperatingSystemVersion, hdr64.MinorOperatingSystemVersion);
                    winExe.m_ImageVersion = new Version(hdr64.MajorImageVersion, hdr64.MinorImageVersion);
                    winExe.m_SubsystemVersion = new Version(hdr64.MajorSubsystemVersion, hdr64.MinorSubsystemVersion);
                    winExe.m_ArchitectureSize = 64;
                    break;
                default:
                    return null;
                }

                // The following might be found on a WinNT machine that is executable.
                switch (subsystem) {
                case PEHeader.IMAGE_SUBSYSTEM.WINDOWS_CUI:
                    winExe.m_Subsystem = WindowsSubsystem.WindowsConsole;
                    winExe.m_IsExe = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) == 0;
                    winExe.m_IsDll = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) != 0;
                    break;
                case PEHeader.IMAGE_SUBSYSTEM.WINDOWS_GUI:
                    winExe.m_Subsystem = WindowsSubsystem.WindowsGui;
                    winExe.m_IsExe = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) == 0;
                    winExe.m_IsDll = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) != 0;
                    break;
                case PEHeader.IMAGE_SUBSYSTEM.NATIVE:
                    winExe.m_Subsystem = WindowsSubsystem.WindowsNativeDriverSys;
                    winExe.m_IsExe = false;
                    winExe.m_IsDll = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) != 0;
                    break;
                case PEHeader.IMAGE_SUBSYSTEM.POSIX_CUI:
                    winExe.m_Subsystem = WindowsSubsystem.ServicesForUnix;
                    winExe.m_IsExe = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) == 0;
                    winExe.m_IsDll = (fileHeader.Characteristics & PEHeader.IMAGE_FILE_Characteristics.DLL) != 0;
                    break;
                default:
                    // Unknown subsystem, so we abort.
                    return null;
                }

                winExe.m_MachineType = GetFileMachineType(fileHeader.Machine);
                winExe.m_WordSize = (fileHeader.Characteristics &
                    PEHeader.IMAGE_FILE_Characteristics.MACHINE_32BIT_MACHINE) == 0 ? 64 : 32;
                return winExe;
            } catch (EndOfStreamException) {
                return null;
            }
        }

        private static FileMachineType GetFileMachineType(PEHeader.IMAGE_FILE_MachineType machine)
        {
            switch (machine) {
            case PEHeader.IMAGE_FILE_MachineType.ARMNT: return FileMachineType.Arm;
            case PEHeader.IMAGE_FILE_MachineType.ARM64: return FileMachineType.Arm64;
            case PEHeader.IMAGE_FILE_MachineType.I386: return FileMachineType.Intel386;
            case PEHeader.IMAGE_FILE_MachineType.IA64: return FileMachineType.Itanium64;
            case PEHeader.IMAGE_FILE_MachineType.AMD64: return FileMachineType.Amd64;
            case PEHeader.IMAGE_FILE_MachineType.ALPHA: return FileMachineType.Alpha;
            default: return FileMachineType.Unknown;
            }
        }

        internal WindowsExecutable() { }

        private FileMachineType m_MachineType;

        /// <summary>
        /// Gets the type of the machine the executable targets.
        /// </summary>
        /// <value>The type of the machine the executable targets.</value>
        public override FileMachineType MachineType { get { return m_MachineType; } }

        /// <summary>
        /// Gets the OS the executable targets.
        /// </summary>
        /// <value>The OS the executable targets.</value>
        public override FileTargetOs TargetOs { get { return FileTargetOs.Windows; } }

        /// <summary>
        /// Gets a value indicating whether this instance is little endian.
        /// </summary>
        /// <value><see langword="true"/> if this instance is little endian; otherwise, <see langword="false"/>.</value>
        public override bool IsLittleEndian { get { return true; } }

        private bool m_IsExe;

        /// <summary>
        /// Gets a value indicating whether this instance is an executable.
        /// </summary>
        /// <value><see langword="true"/> if this instance is executable; otherwise, <see langword="false"/>.</value>
        public override bool IsExe { get { return m_IsExe; } }

        private bool m_IsDll;

        /// <summary>
        /// Gets a value indicating whether this instance is DLL.
        /// </summary>
        /// <value><see langword="true"/> if this instance is DLL; otherwise, <see langword="false"/>.</value>
        public override bool IsDll { get { return m_IsDll; } }

        private int m_ArchitectureSize;

        /// <summary>
        /// Gets the architecture address size for the executable binary.
        /// </summary>
        /// <value>The architecture address size of the executable binary.</value>
        public override int ArchitectureSize { get { return m_ArchitectureSize; } }

        private int m_WordSize;

        /// <summary>
        /// Gets the word size for the executable binary.
        /// </summary>
        /// <value>The word size of the executable binary.</value>
        public int WordSize { get { return m_WordSize; } }

        private WindowsSubsystem m_Subsystem;

        /// <summary>
        /// Gets the Windows subsystem the binary should run on.
        /// </summary>
        /// <value>The Windows subsystem.</value>
        public WindowsSubsystem Subsystem { get { return m_Subsystem; } }

        private Version m_OSVersion;

        /// <summary>
        /// Gets the OS Version for the PE file.
        /// </summary>
        /// <value>The OS version.</value>
        public Version OSVersion { get { return m_OSVersion; } }

        private Version m_ImageVersion;

        /// <summary>
        /// Gets the image version for the PE file.
        /// </summary>
        /// <value>The image version.</value>
        public Version ImageVersion { get { return m_ImageVersion; } }

        private Version m_SubsystemVersion;

        /// <summary>
        /// Gets the subsystem version for the PE file.
        /// </summary>
        /// <value>The subsystem version.</value>
        public Version SubsystemVersion { get { return m_SubsystemVersion; } }
    }
}
