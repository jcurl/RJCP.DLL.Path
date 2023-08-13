namespace RJCP.IO.Native.Win32
{
    using System;

    internal partial class Kernel32
    {
        /// <summary>
        /// File attributes, flags, and security settings that are passed to the CreateFile method.
        /// </summary>
        [Flags]
        public enum FileAttributeFlags : uint
        {
            /// <summary>
            /// A file that is read-only. Applications can read the file, but cannot write to it or delete it. This
            /// attribute is not honored on directories. For more information, see You cannot view or change the
            /// Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, in Windows Vista or
            /// in Windows 7
            /// </summary>
            FILE_ATTRIBUTE_READONLY = 0x01,

            /// <summary>
            /// The file or directory is hidden. It is not included in an ordinary directory listing.
            /// </summary>
            FILE_ATTRIBUTE_HIDDEN = 0x02,

            /// <summary>
            /// A file or directory that the operating system uses a part of, or uses exclusively.
            /// </summary>
            FILE_ATTRIBUTE_SYSTEM = 0x04,

            /// <summary>
            /// The handle that identifies a directory.
            /// </summary>
            FILE_ATTRIBUTE_DIRECTORY = 0x10,

            /// <summary>
            /// A file or directory that is an archive file or directory. Applications typically use this attribute to
            /// mark files for backup or removal.
            /// </summary>
            FILE_ATTRIBUTE_ARCHIVE = 0x20,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            FILE_ATTRIBUTE_DEVICE = 0x40,

            /// <summary>
            /// A file that does not have other attributes set. This attribute is valid only when used alone.
            /// </summary>
            FILE_ATTRIBUTE_NORMAL = 0x80,

            /// <summary>
            /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if
            /// sufficient cache memory is available, because typically, an application deletes a temporary file after
            /// the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the
            /// data is written after the handle is closed.
            /// </summary>
            FILE_ATTRIBUTE_TEMPORARY = 0x100,

            /// <summary>
            /// A file that is a sparse file.
            /// </summary>
            FILE_ATTRIBUTE_SPARSE_FILE = 0x200,

            /// <summary>
            /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
            /// </summary>
            FILE_ATTRIBUTE_REPARSE_POINT = 0x400,

            /// <summary>
            /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a
            /// directory, compression is the default for newly created files and subdirectories.
            /// </summary>
            FILE_ATTRIBUTE_COMPRESSED = 0x800,

            /// <summary>
            /// The data of a file is not available immediately. This attribute indicates that the file data is
            /// physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical
            /// storage management software. Applications should not arbitrarily change this attribute.
            /// </summary>
            FILE_ATTRIBUTE_OFFLINE = 0x1000,

            /// <summary>
            /// The file or directory is not to be indexed by the content indexing service.
            /// </summary>
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000,

            /// <summary>
            /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a
            /// directory, encryption is the default for newly created files and subdirectories.
            /// </summary>
            FILE_ATTRIBUTE_ENCRYPTED = 0x4000,

            /// <summary>
            /// The directory or user data stream is configured with integrity (only supported on ReFS volumes). It is
            /// not included in an ordinary directory listing. The integrity setting persists with the file if it's
            /// renamed. If a file is copied the destination file will have integrity set if either the source file or
            /// destination directory have integrity set. Windows Server 2008 R2, Windows 7, Windows Server 2008,
            /// Windows Vista, Windows Server 2003 and Windows XP: This flag is not supported until Windows Server 2012.
            /// </summary>
            FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x8000,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            FILE_ATTRIBUTE_VIRTUAL = 0x10000,

            /// <summary>
            /// The user data stream not to be read by the background data integrity scanner (AKA scrubber). When set on
            /// a directory it only provides inheritance. This flag is only supported on Storage Spaces and ReFS
            /// volumes. It is not included in an ordinary directory listing. Windows Server 2008 R2, Windows 7, Windows
            /// Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This flag is not supported until Windows
            /// 8 and Windows Server 2012.
            /// </summary>
            FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x20000,

            /// <summary>
            /// A file or directory with extended attributes. IMPORTANT: This constant is for internal use only.
            /// </summary>
            FILE_ATTRIBUTE_EA = 0x40000,

            /// <summary>
            /// This attribute indicates user intent that the file or directory should be kept fully present locally
            /// even when not being actively accessed. This attribute is for use with hierarchical storage management
            /// software.
            /// </summary>
            FILE_ATTRIBUTE_PINNED = 0x00080000,

            /// <summary>
            /// This attribute indicates that the file or directory should not be kept fully present locally except when
            /// being actively accessed. This attribute is for use with hierarchical storage management software.
            /// </summary>
            FILE_ATTRIBUTE_UNPINNED = 0x00100000,

            /// <summary>
            /// This attribute only appears in directory enumeration classes (FILE_DIRECTORY_INFORMATION,
            /// FILE_BOTH_DIR_INFORMATION, etc.). When this attribute is set, it means that the file or directory has no
            /// physical representation on the local system; the item is virtual. Opening the item will be more
            /// expensive than normal, e.g. it will cause at least some of it to be fetched from a remote store.
            /// </summary>
            FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x00040000,

            /// <summary>
            /// When this attribute is set, it means that the file or directory is not fully present locally. For a file
            /// that means that not all of its data is on local storage (e.g. it may be sparse with some data still in
            /// remote storage). For a directory it means that some of the directory contents are being virtualized from
            /// another location. Reading the file / enumerating the directory will be more expensive than normal, e.g.
            /// it will cause at least some of the file/directory content to be fetched from a remote store. Only
            /// kernel-mode callers can set this bit. File system mini filters below the 180000 – 189999 altitude range
            /// (FSFilter HSM Load Order Group) must not issue targeted cached reads or writes to files that have this
            /// attribute set.This could lead to cache pollution and potential file corruption.For more information, see
            /// Handling placeholders.
            /// </summary>
            FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x00400000
        }
    }
}
