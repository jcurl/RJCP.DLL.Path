namespace RJCP.IO.FileSystem
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using Native.Win32;

    /// <summary>
    /// Get file information for Windows Operating Systems.
    /// </summary>
    internal sealed class Win32NodeInfo : NodeInfo<Win32NodeInfo>
    {
        private readonly int m_HashCode;

        public Win32NodeInfo(string path, bool resolveLinks)
        {
#if NETSTANDARD
            int idInfoSize = Marshal.SizeOf<Kernel32.FILE_ID_INFO>();
#else
            int idInfoSize = Marshal.SizeOf(typeof(Kernel32.FILE_ID_INFO));
#endif

            SafeFileHandle file = GetFileHandle(path, false, true);
            try {
                // Get the link target of the path, without resolving.
                bool result = Kernel32.GetFileInformationByHandle(file, out Kernel32.BY_HANDLE_FILE_INFORMATION fileInfoByHandle);
                if (result) {
                    if ((fileInfoByHandle.FileAttributes & Kernel32.FileAttributeFlags.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
                        LinkTarget = GetLinkTarget(path);
                        if (resolveLinks) {
                            if (LinkTarget == null)
                                throw new FileNotFoundException($"Link {path} can't be resolved", path);

                            file.Close();
                            file = GetFileHandle(path, true, true);
                            result = Kernel32.GetFileInformationByHandle(file, out fileInfoByHandle);
                            if (!result)
                                throw new FileNotFoundException($"Resolved File {LinkTarget} not found", LinkTarget);
                        }
                    }
                } else {
                    throw new FileNotFoundException($"File {path} not found", path);
                }

                if (m_Win32ExtendedApi) {
                    try {
                        bool resultEx = Kernel32.GetFileInformationByHandleEx(file, Kernel32.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                            out Kernel32.FILE_ID_INFO fileInfoEx, idInfoSize);
                        if (resultEx) {
                            Type = NodeInfoType.WindowsExtended;
                            VolumeSerialNumber = fileInfoEx.VolumeSerialNumber;
                            FileIdHigh = fileInfoEx.FileIdHigh;
                            FileIdLow = fileInfoEx.FileIdLow;
                            m_HashCode = unchecked(
                                (int)fileInfoEx.VolumeSerialNumber ^
                                ((int)fileInfoEx.FileIdHigh << 16) ^
                                (int)fileInfoEx.FileIdLow
                            );
                            return;
                        }
                    } catch (EntryPointNotFoundException) {
                        ExtendedApiNotAvailable();
                    }
                }

                Type = NodeInfoType.WindowsFileInfo;
                VolumeSerialNumber = fileInfoByHandle.VolumeSerialNumber;
                FileIdHigh = fileInfoByHandle.FileIndexHigh;
                FileIdLow = fileInfoByHandle.FileIndexLow;
                m_HashCode = unchecked(
                    (int)fileInfoByHandle.VolumeSerialNumber ^
                    ((int)fileInfoByHandle.FileIndexHigh << 16) ^
                    (int)fileInfoByHandle.FileIndexLow
                );
            } finally {
                file.Close();
            }
        }

        private static SafeFileHandle GetFileHandle(string path, bool resolveLinks, bool throwOnError)
        {
            SafeFileHandle file = null;
            if (File.Exists(path)) {
                Kernel32.CreateFileFlags createFlags = resolveLinks ?
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL :
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL | Kernel32.CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT;

                file = Kernel32.CreateFile(path, 0,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING, createFlags, IntPtr.Zero);
            } else {
                // See https://learn.microsoft.com/en-us/windows/win32/fileio/obtaining-a-handle-to-a-directory

                Kernel32.CreateFileFlags createFlags = resolveLinks ?
                    Kernel32.CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS :
                    Kernel32.CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS | Kernel32.CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT;

                file = Kernel32.CreateFile(path, 0,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING, createFlags, IntPtr.Zero);
            }

            if (file.IsInvalid) {
                if (throwOnError) {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    throw new FileNotFoundException($"Link {path} can't be resolved", path, ex);
                }
            }
            return file;
        }

        private static string GetLinkTarget(string path)
        {
            SafeFileHandle file = GetFileHandle(path, true, false);
            if (file.IsInvalid) {
                // Getting the resolved link failed, so we open the symbolic link itself. If there is a problem, an
                // exception is raised. So we know the file handle is valid.
                return GetLinkTargetDirect(path);
            }

            try {
                // See https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew
                //
                // When using VOLUME_NAME_DOS, the string that is returned by this function uses the "\\?\" syntax. For
                // more information, see CreateFile.
                //
                // We don't implement this case: Some third-party drivers can create a drive letter or mount point
                // without using the Mount Manager. If the Mount Manager was not used to create the drive, then
                // VOLUME_NAME_DOS or VOLUME_NAME_GUID will not succeed; only VOLUME_NAME_NT will be available. To
                // determine the drive letter for the volume device path, use the QueryDosDevice function on every drive
                // letter until a matching device name is found.

                unsafe {
                    char* buffer = stackalloc char[32768];

                    // Windows Vista and later. On Windows XP, we don't support symlinks (fsutil doesn't, and most utils
                    // don't properly support them either).
                    int len = Kernel32.GetFinalPathNameByHandle(file, buffer, 32768, 0);
                    if (len < 0) {
                        Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                        throw new FileNotFoundException($"Link {path} can't be resolved", path, ex);
                    } else if (len > 32768) {
                        return null;
                    }

                    string target = new string(buffer, 0, len);
                    if (target.StartsWith(@"\\?\"))
#if NETSTANDARD
                        return target[4..];
#else
                        return target.Substring(4);
#endif
                    return target;
                }
            } finally {
                file.Close();
            }
        }

        /// <summary>
        /// Checks the reparse structs.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is an internal error.</exception>
        /// <remarks>
        /// This method ensures that the <see cref="Kernel32.REPARSE_DATA_BUFFER_Generic"/> is large enough to contain
        /// all data before copying to other structs. If not, then we have an internal programming error and should
        /// abort and fix the code.
        /// </remarks>
        [Conditional("DEBUG")]
        private static void CheckReparseStructs()
        {
            int s1 = Marshal.SizeOf(typeof(Kernel32.REPARSE_DATA_BUFFER_Generic));
            int s2 = Marshal.SizeOf(typeof(Kernel32.REPARSE_DATA_BUFFER_SymbolicLink));
            int s3 = Marshal.SizeOf(typeof(Kernel32.REPARSE_DATA_BUFFER_Junction));
            if (s1 < s2) throw new InvalidOperationException("Internal Error");
            if (s1 < s3) throw new InvalidOperationException("Internal Error");
        }

        private static string GetLinkTargetDirect(string path)
        {
            CheckReparseStructs();
            SafeFileHandle file = GetFileHandle(path, false, true);

            // For more information, see also:
            // - https://social.msdn.microsoft.com/Forums/en-US/aaa2d1b8-b302-4939-861e-5b011b8b8a50/how-to-get-the-target-of-a-symbolic-link?forum=vcgeneral
            // - https://www.codeproject.com/Articles/15633/Manipulating-NTFS-Junction-Points-in-NET
            IntPtr outBuffer = IntPtr.Zero;
            try {
                int outBufferSize = Marshal.SizeOf(typeof(Kernel32.REPARSE_DATA_BUFFER_Generic));
                outBuffer = Marshal.AllocHGlobal(outBufferSize);

                bool success = Kernel32.DeviceIoControl(file, Kernel32.FSCTL.GET_REPARSE_POINT,
                    IntPtr.Zero, 0, outBuffer, outBufferSize, out int _, IntPtr.Zero);
                if (!success) {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    throw new FileNotFoundException($"Link {path} can't be resolved", path, ex);
                }
                Kernel32.REPARSE_DATA_BUFFER_Generic genReparseDataBuffer =
                    (Kernel32.REPARSE_DATA_BUFFER_Generic)Marshal.PtrToStructure(outBuffer, typeof(Kernel32.REPARSE_DATA_BUFFER_Generic));

                switch (genReparseDataBuffer.ReparseTag) {
                case Kernel32.IO_REPARSE_TAG_SYMLINK:
                    Kernel32.REPARSE_DATA_BUFFER_SymbolicLink symReparseDataBuffer =
                        (Kernel32.REPARSE_DATA_BUFFER_SymbolicLink)Marshal.PtrToStructure(outBuffer, typeof(Kernel32.REPARSE_DATA_BUFFER_SymbolicLink));

                    if ((symReparseDataBuffer.Flags & Kernel32.SYMLINK_FLAG_RELATIVE) != 0) {
                        // This is a relative path. We need to update it based on the absolute initial path.
                        string targetName = Encoding.Unicode.GetString(symReparseDataBuffer.PathBuffer,
                            symReparseDataBuffer.SubstituteNameOffset, symReparseDataBuffer.SubstituteNameLength);

                        IO.Path fullPath = IO.Path.ToPath(path);
                        if (!fullPath.IsPinned)
                            fullPath = IO.Path.ToPath(Environment.CurrentDirectory).Append(fullPath);
                        fullPath = fullPath.GetParent().Append(targetName);
                        return fullPath.ToString();
                    }

                    return Encoding.Unicode.GetString(symReparseDataBuffer.PathBuffer,
                        symReparseDataBuffer.PrintNameOffset, symReparseDataBuffer.PrintNameLength);
                case Kernel32.IO_REPARSE_TAG_MOUNT_POINT:
                    Kernel32.REPARSE_DATA_BUFFER_Junction junReparseDataBuffer =
                        (Kernel32.REPARSE_DATA_BUFFER_Junction)Marshal.PtrToStructure(outBuffer, typeof(Kernel32.REPARSE_DATA_BUFFER_Junction));

                    return Encoding.Unicode.GetString(junReparseDataBuffer.PathBuffer,
                        junReparseDataBuffer.PrintNameOffset, junReparseDataBuffer.PrintNameLength);
                default:
                    return null;
                }
            } finally {
                if (!outBuffer.Equals(IntPtr.Zero))
                    Marshal.FreeHGlobal(outBuffer);
                file.Close();
            }
        }

        private static bool m_Win32ExtendedApi = true;

        private static void ExtendedApiNotAvailable()
        {
            // Don't use the extended API (e.g. on Windows XP) because it is not defined.
            m_Win32ExtendedApi = false;
        }

        public override NodeInfoType Type { get; }

        public ulong VolumeSerialNumber { get; }

        public ulong FileIdLow { get; }

        public ulong FileIdHigh { get; }

        public override string LinkTarget { get; }

        protected override bool Equals(Win32NodeInfo other)
        {
            return Type == other.Type &&
                VolumeSerialNumber == other.VolumeSerialNumber &&
                FileIdHigh == other.FileIdHigh &&
                FileIdLow == other.FileIdLow;
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }
    }
}
