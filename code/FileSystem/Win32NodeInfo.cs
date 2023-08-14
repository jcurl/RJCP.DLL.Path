namespace RJCP.IO.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
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

            SafeFileHandle file = GetFileHandle(path, false);
            try {
                // Get the link target of the path, without resolving.
                bool result = Kernel32.GetFileInformationByHandle(file, out Kernel32.BY_HANDLE_FILE_INFORMATION fileInfoByHandle);
                if (result) {
                    if ((fileInfoByHandle.FileAttributes & Kernel32.FileAttributeFlags.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
                        LinkTarget = GetLinkTarget(path);
                        if (resolveLinks) {
                            file.Close();
                            file = GetFileHandle(path, true);
                            result = Kernel32.GetFileInformationByHandle(file, out fileInfoByHandle);
                            if (!result) {
                                // TODO: We may want to raise an exception here.
                                Type = NodeInfoType.None;
                                return;
                            }
                        }
                    }
                } else {
                    // TODO: We may want to raise an exception here.
                    Type = NodeInfoType.None;
                    return;
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

        private static SafeFileHandle GetFileHandle(string path, bool resolveLinks)
        {
            SafeFileHandle file;
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
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                throw new InvalidOperationException();
            }
            return file;
        }

        private static string GetLinkTarget(string path)
        {
            SafeFileHandle file = GetFileHandle(path, true);

            // See https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew
            //
            // When using VOLUME_NAME_DOS, the string that is returned by this function uses the "\\?\" syntax. For more
            // information, see CreateFile.
            //
            // We don't implement this case: Some third-party drivers can create a drive letter or mount point without
            // using the Mount Manager. If the Mount Manager was not used to create the drive, then VOLUME_NAME_DOS or
            // VOLUME_NAME_GUID will not succeed; only VOLUME_NAME_NT will be available. To determine the drive letter
            // for the volume device path, use the QueryDosDevice function on every drive letter until a matching device
            // name is found.

            unsafe {
                char* buffer = stackalloc char[32768];
                int len = Kernel32.GetFinalPathNameByHandle(file, buffer, 32768, 0);
                if (len < 0 || len >= 32768)
                    return string.Empty;

                string target = new string(buffer, 0, len);
                if (target.StartsWith(@"\\?\"))
#if NETSTANDARD
                    return target[4..];
#else
                    return target.Substring(4);
#endif
                return target;
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
