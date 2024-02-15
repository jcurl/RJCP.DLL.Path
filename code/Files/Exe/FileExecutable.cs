namespace RJCP.IO.Files.Exe
{
    using System;
    using System.IO;

    /// <summary>
    /// Get properties about a potentially executable file.
    /// </summary>
    public abstract class FileExecutable
    {
        /// <summary>
        /// Gets information about the executable file.
        /// </summary>
        /// <param name="path">The path to the executable file.</param>
        /// <returns>
        /// A reference to a <see cref="FileExecutable"/> (or derived), or <see langword="null"/> if the file is not an
        /// executable format.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is invalid (empty or whitespace).
        /// <para>- or -</para>
        /// .NET Framework and .NET Core versions older than 2.1: path is an empty string (""), contains only white
        /// space, or contains one or more invalid characters.
        /// <para>- or -</para>
        /// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The file specified by <paramref name="path"/> does not exist.
        /// </exception>
        /// <exception cref="IOException">
        /// The file is invalid, or cannot be properly read.
        /// <para>- or -</para>
        /// An I/O error specified by <paramref name="path"/> occurred.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">Access is not permitted by the operating system.</exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <remarks>
        /// If the file is not recognisable, then <see langword="null"/> is returned. Otherwise an object deriving from
        /// this class <see cref="FileExecutable"/> is returned. Note, that if the result is not <see langword="null"/>,
        /// future implementations may add new derived classes, so you should check all properties for determining the
        /// file type (e.g. if the file is not a PE Executable on Windows, one cannot assume that it is therefore an ELF
        /// file for Linux, other formats may be defined at a later time).
        /// </remarks>
        public static FileExecutable GetFile(string path)
        {
            ThrowHelper.ThrowIfNullOrWhiteSpace(path);

            using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new(fs)) {
                FileExecutable exe = WindowsExecutable.GetFile(br);
                if (exe is not null) return exe;

                fs.Position = 0;
                return UnixElfExecutable.GetFile(br);
            }
        }

        /// <summary>
        /// Gets information about the executable file.
        /// </summary>
        /// <param name="path">The path to the executable file.</param>
        /// <returns>
        /// A reference to a <see cref="FileExecutable"/> (or derived), or <see langword="null"/> if the file is not an
        /// executable format.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is invalid (empty or whitespace).</exception>
        /// <remarks>
        /// If the file is not recognisable, then <see langword="null"/> is returned. Otherwise an object deriving from
        /// this class <see cref="FileExecutable"/> is returned. Note, that if the result is not <see langword="null"/>,
        /// future implementations may add new derived classes, so you should check all properties for determining the
        /// file type (e.g. if the file is not a PE Executable on Windows, one cannot assume that it is therefore an ELF
        /// file for Linux, other formats may be defined at a later time).
        /// </remarks>
        public static FileExecutable GetFile(IO.Path path)
        {
            return GetFile(path?.ToString());
        }

        /// <summary>
        /// Gets the type of the machine the executable targets.
        /// </summary>
        /// <value>The type of the machine the executable targets.</value>
        public abstract FileMachineType MachineType { get; }

        /// <summary>
        /// Gets the OS the executable targets.
        /// </summary>
        /// <value>The OS the executable targets.</value>
        public abstract FileTargetOs TargetOs { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is little endian.
        /// </summary>
        /// <value><see langword="true"/> if this instance is little endian; otherwise, <see langword="false"/>.</value>
        public abstract bool IsLittleEndian { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is an executable.
        /// </summary>
        /// <value><see langword="true"/> if this instance is executable; otherwise, <see langword="false"/>.</value>
        public abstract bool IsExe { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is DLL.
        /// </summary>
        /// <value><see langword="true"/> if this instance is DLL; otherwise, <see langword="false"/>.</value>
        public abstract bool IsDll { get; }

        /// <summary>
        /// Gets the architecture address size for the executable binary.
        /// </summary>
        /// <value>The architecture address size of the executable binary.</value>
        public abstract int ArchitectureSize { get; }
    }
}
