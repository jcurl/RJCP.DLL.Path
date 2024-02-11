namespace RJCP.IO
{
    using System;
    using Internal;
    using RJCP.Core.Environment;

    /// <summary>
    /// A representation of a file system path that is operating system agnostic.
    /// </summary>
    public abstract class Path
    {
        /// <summary>
        /// Gets the root volume.
        /// </summary>
        /// <value>The root volume.</value>
        /// <remarks>
        /// This is the root volume for the path, or the drive letter, depending on the Operating System.
        /// </remarks>
        public string RootVolume { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the path is pinned.
        /// </summary>
        /// <value>Returns <see langword="true"/> if this instance is pinned; otherwise, <see langword="false"/>.</value>
        /// <remarks>A pinned path is one that is relative to the root of the file system.</remarks>
        public bool IsPinned { get; protected set; }

        /// <summary>
        /// Converts a string to a <see cref="Path"/> object, based on the current Operating System.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <returns>A <see cref="Path"/> object that can be manipulated.</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// The platform is not supported. To create a Path object, instantiate the object directly. See
        /// <see cref="WindowsPath"/>.
        /// </exception>
        /// <remarks>
        /// Converts the string path to a path based on the current operating system. The resultant path is
        /// automatically normalized.
        /// </remarks>
        public static Path ToPath(string path)
        {
            if (Platform.IsWinNT()) return new WindowsPath(path);
            if (Platform.IsUnix()) return new UnixPath(path);
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Creates the path. To be implemented by the classes deriving from this abstract class.
        /// </summary>
        /// <param name="path">The path that is to be parsed.</param>
        /// <returns>
        /// A parsed path, given as a <see cref="Path"/> object, specific for the Operating System this class
        /// represents.
        /// </returns>
        protected abstract Path CreatePath(string path);

        /// <summary>
        /// Appends the specified path to the current path.
        /// </summary>
        /// <param name="path">The path element to append.</param>
        /// <returns>
        /// A parsed path, given as a <see cref="Path"/> object, specific for the Operating System this class
        /// represents.
        /// </returns>
        public Path Append(string path)
        {
            return Append(CreatePath(path));
        }

        /// <summary>
        /// Appends the specified path to the current path.
        /// </summary>
        /// <param name="path">The path object to append to this object.</param>
        /// <returns>The appended <see cref="Path"/>.</returns>
        public abstract Path Append(Path path);

        /// <summary>
        /// Gets the relative path given a base path.
        /// </summary>
        /// <param name="basePath">The base path, which will be used to get the relative path.</param>
        /// <returns>A new <see cref="Path"/> that is relative to the <paramref name="basePath"/>.</returns>
        public Path GetRelative(string basePath)
        {
            return GetRelative(CreatePath(basePath));
        }

        /// <summary>
        /// Gets the relative path given a base path.
        /// </summary>
        /// <param name="basePath">The base path, which will be used to get the relative path.</param>
        /// <returns>A new <see cref="Path"/> that is relative to the <paramref name="basePath"/>.</returns>
        public abstract Path GetRelative(Path basePath);

        /// <summary>
        /// Gets the parent of the current path.
        /// </summary>
        /// <returns>The new parent of this path, or the root volume.</returns>
        public abstract Path GetParent();

        /// <summary>
        /// Trims the trailing folder character, if one exists.
        /// </summary>
        /// <returns>A new <see cref="Path"/> object with the trailing path trimmed.</returns>
        /// <remarks>
        /// If the path ends with a directory separator character, it is removed, unless it would make the path from
        /// being an absolute path, to a relative path, in which case it is not removed.
        /// </remarks>
        public abstract Path Trim();

        /// <summary>
        /// Determines whether this instance is trimmed.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if this path is trimmed; otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool IsTrimmed();

        /// <summary>
        /// Gets the path stack.
        /// </summary>
        /// <value>The path stack representing each folder and leaf node in the path.</value>
        internal PathStack PathStack { get; } = new PathStack();
    }
}
