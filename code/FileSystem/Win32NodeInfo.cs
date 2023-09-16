namespace RJCP.IO.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using Native.Win32;
    using RJCP.Core;

    /// <summary>
    /// Get file information for Windows Operating Systems.
    /// </summary>
    internal sealed class Win32NodeInfo : NodeInfo<Win32NodeInfo>
    {
        private readonly int m_HashCode;

        // Don't use the extended API (e.g. on Windows XP) because it is not defined.
        private static class Api
        {
            private static bool s_GetFileInformationByHandleEx = true;
            private static bool s_GetFinalPathNameByHandle = true;

            public static bool GetFileInformationByHandleEx { get { return s_GetFileInformationByHandleEx; } }

            public static bool GetFinalPathNameByHandle { get { return s_GetFinalPathNameByHandle; } }

            public static void DisableGetFinalPathNameByHandle()
            {
                s_GetFinalPathNameByHandle = false;
            }

            public static void DisableGetFileInformationByHandleEx()
            {
                s_GetFileInformationByHandleEx = false;
            }
        }

        public Win32NodeInfo(string path, bool resolveLinks)
        {
            SafeFileHandle file = GetFileHandle(path, false).Value;
            if (GetTarget(file).TryGet(out string finalPath))
                Path = finalPath;

            try {
                // Get the link target of the path, without resolving.
                bool result = Kernel32.GetFileInformationByHandle(file, out Kernel32.BY_HANDLE_FILE_INFORMATION fileInfoByHandle);
                if (result) {
                    if ((fileInfoByHandle.FileAttributes & Kernel32.FileAttributeFlags.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
                        LinkTarget = GetLinkTarget(path, file).Value;
                        if (resolveLinks) {
                            if (LinkTarget == null)
                                throw new FileNotFoundException($"Link {path} can't be resolved", path);

                            file.Close();
                            file = GetFileHandle(path, true).Value;
                            result = Kernel32.GetFileInformationByHandle(file, out fileInfoByHandle);
                            if (!result)
                                throw new FileNotFoundException($"Resolved File {LinkTarget} not found", LinkTarget);
                        }
                    }
                } else {
                    throw new FileNotFoundException($"File {path} not found", path);
                }

                if (Api.GetFileInformationByHandleEx) {
                    try {
                        int idInfoSize = Marshal.SizeOf(typeof(Kernel32.FILE_ID_INFO));
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
                        Api.DisableGetFileInformationByHandleEx();
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

        private static readonly SafeFileHandle InvalidFile = new SafeFileHandle(IntPtr.Zero, false);

        private static Result<SafeFileHandle> GetFileHandle(string path, bool resolveLinks)
        {
            SafeFileHandle file;
            if (File.Exists(path)) {
                Kernel32.CreateFileFlags createFlags = resolveLinks ?
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL :
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL | Kernel32.CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT;

                file = Kernel32.CreateFile(path, 0,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING, createFlags, IntPtr.Zero);
            } else if (Directory.Exists(path)) {
                // See https://learn.microsoft.com/en-us/windows/win32/fileio/obtaining-a-handle-to-a-directory

                Kernel32.CreateFileFlags createFlags = resolveLinks ?
                    Kernel32.CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS :
                    Kernel32.CreateFileFlags.FILE_FLAG_BACKUP_SEMANTICS | Kernel32.CreateFileFlags.FILE_FLAG_OPEN_REPARSE_POINT;

                file = Kernel32.CreateFile(path, 0,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING, createFlags, IntPtr.Zero);
            } else {
                return Result.FromException<SafeFileHandle>(new FileNotFoundException($"Link {path} not found", path));
            }

            if (file.IsInvalid) {
                Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                return Result.FromException<SafeFileHandle>(new FileNotFoundException($"Link {path} not found", path, ex));
            }
            return file;
        }

        private static Result<string> GetTarget(SafeFileHandle targetHandle)
        {
            // Note, this is always called at least once, even on Windows XP, until we determine the API doesn't exist.
            if (Api.GetFinalPathNameByHandle) {
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

                try {
                    unsafe {
                        char* buffer = stackalloc char[32768];

                        // Windows Vista and later. On Windows XP, we don't support symlinks (fsutil doesn't, and most utils
                        // don't properly support them either).
                        int len = Kernel32.GetFinalPathNameByHandle(targetHandle, buffer, 32768, 0);
                        if (len < 0) {
                            return Result.FromException<string>(Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                        } else if (len > 32768) {
                            return Result.FromException<string>(new FileNotFoundException("Handle cannot be resolved"));
                        }

                        string target = new string(buffer, 0, len);
                        if (target.StartsWith(@"\\?\UNC\")) {
#if NETSTANDARD
                            string uncPath = target[8..];
#else
                            string uncPath = target.Substring(8);
#endif
                            return string.Format(@"\\{0}", uncPath);
                        } else if (target.StartsWith(@"\\?\"))
#if NETSTANDARD
                            return target[4..];
#else
                            return target.Substring(4);
#endif
                        return target;
                    }
                } catch (EntryPointNotFoundException) {
                    Api.DisableGetFinalPathNameByHandle();
                }
            }

            // Finally, to get it working on Windows XP, we have to revert to querying the objects directly.
            string ntTargetFileName = NtQueryInformationFile(targetHandle);
            if (ntTargetFileName == null)
                return Result.FromException<string>(new FileNotFoundException("Handle NtQueryInformationFile failed"));

            string ntObjectName = NtQueryObjectNameInformation(targetHandle);
            if (ntObjectName == null)
                return Result.FromException<string>(new FileNotFoundException("Handle NtQueryObject for ObjectNameInformation failed"));

            // Calculate the device name
            string deviceName = null;
            if (ntObjectName.EndsWith(ntTargetFileName)) {
                deviceName = ntObjectName.Substring(0, ntObjectName.Length - ntTargetFileName.Length);
            }

            // Try to map the device name to a DOS drive letter.
            string dosPath = QueryDosDeviceSubstitute(deviceName, ntTargetFileName);
            if (dosPath != null) return dosPath;
            return $"\\{ntTargetFileName}";
        }

        private static string NtQueryInformationFile(SafeFileHandle targetHandle)
        {
            IntPtr ipFileName = IntPtr.Zero;
            try {
                ipFileName = Marshal.AllocHGlobal(32768);
                int ntstatus = NtDll.NtQueryInformationFile(targetHandle, out NtDll.IO_STATUS_BLOCK iosb,
                    ipFileName, 32768, NtDll.FILE_INFORMATION_CLASS.FileNameInformation);
                if (ntstatus != 0) return null;

                NtDll.FILE_NAME_INFORMATION objFileName =
                    (NtDll.FILE_NAME_INFORMATION)Marshal.PtrToStructure(ipFileName, typeof(NtDll.FILE_NAME_INFORMATION));
                if (objFileName.FileNameLength <= 0) return null;

                // The file name is placed after the length, which is a ULONG (32-bit on Windows).
                return Marshal.PtrToStringUni(ipFileName + 4, (int)objFileName.FileNameLength / 2);
            } finally {
                if (!ipFileName.Equals(IntPtr.Zero))
                    Marshal.FreeHGlobal(ipFileName);
            }
        }

        private static string NtQueryObjectNameInformation(SafeFileHandle targetHandle)
        {
            IntPtr ipObjName = IntPtr.Zero;
            try {
                int nLength = 260;
                ipObjName = Marshal.AllocHGlobal(nLength);
                int ntstatus = NtDll.NtQueryObject(targetHandle, NtDll.OBJECT_INFORMATION_CLASS.ObjectNameInformation,
                    ipObjName, nLength, out nLength);
                if (ntstatus != 0) {
                    if (ntstatus != unchecked((int)0xc0000004)) return null;
                    if (nLength <= 0 || nLength > 65536) return null;

                    Marshal.FreeHGlobal(ipObjName);
                    ipObjName = Marshal.AllocHGlobal(nLength);
                    ntstatus = NtDll.NtQueryObject(targetHandle, NtDll.OBJECT_INFORMATION_CLASS.ObjectNameInformation,
                        ipObjName, nLength, out nLength);
                    if (ntstatus != 0) return null;
                }

                NtDll.OBJECT_NAME_INFORMATION objName =
                    (NtDll.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjName, typeof(NtDll.OBJECT_NAME_INFORMATION));
                if (objName.Name.Length <= 0) return null;
                if (objName.Name.Buffer.Equals(IntPtr.Zero)) return null;
                return Marshal.PtrToStringUni(objName.Name.Buffer, objName.Name.Length / 2);
            } finally {
                if (!ipObjName.Equals(IntPtr.Zero))
                    Marshal.FreeHGlobal(ipObjName);
            }
        }

        private static string QueryDosDeviceSubstitute(string device, string path)
        {
            unsafe {
                byte* target = stackalloc byte[1024];
                foreach (string drivePath in Environment.GetLogicalDrives()) {
                    string drv = drivePath.Substring(0, 2);
                    int result = Kernel32.QueryDosDevice(drv, target, 1024);
                    if (result > 0 && result < 1024) {
                        string dosDevice = Marshal.PtrToStringAnsi(new IntPtr(target));
                        if (dosDevice.Equals(device)) {
                            return $"{drv}{path}";
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the link to the target.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <returns>The string with the link to the target.</returns>
        /// <exception cref="FileNotFoundException">Link <paramref name="path"/> can't be resolved</exception>
        /// <remarks>
        /// It opens the file so that the end target is resolved, and gets the name of the file. This function assumes
        /// that <paramref name="path"/> is already determined to be a reparse point.
        /// </remarks>
        private static Result<string> GetLinkTarget(string path)
        {
            return GetLinkTarget(path, InvalidFile);
        }

        /// <summary>
        /// Gets the link to the target.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <param name="unresolvedLink">
        /// The already opened handle to the reparse point, so it doesn't need to be opened twice.
        /// </param>
        /// <returns>The string with the link to the target.</returns>
        /// <remarks>
        /// It opens the file so that the end target is resolved, and gets the name of the file. This function assumes
        /// that <paramref name="path"/> is already determined to be a reparse point. If opening the path fails, the
        /// <paramref name="unresolvedLink"/> is then used to get the reparse point information which contains the link.
        /// Therefore, it is important that the <paramref name="unresolvedLink"/> was already opened with
        /// <see cref="GetFileHandle(string, bool)"/> without resolving links.
        /// <para>The following exceptions are returned as an error:</para>
        /// <para>
        /// <see cref="FileNotFoundException"/> - Link <paramref name="path"/> can't be resolved. See the inner
        /// exception for more details.
        /// </para>
        /// </remarks>
        private static Result<string> GetLinkTarget(string path, SafeFileHandle unresolvedLink)
        {
            if (!GetFileHandle(path, true).TryGet(out SafeFileHandle file)) {
                // Getting the resolved link failed, so we open the symbolic link itself. If there is a problem, an
                // exception is raised. So we know the file handle is valid.
                if (unresolvedLink.IsInvalid) {
                    return GetLinkTargetResolve(path, false);
                } else {
                    return GetLinkTargetResolve(path, unresolvedLink, false);
                }
            }

            try {
                Result<string> targetResult = GetTarget(file);
                if (!targetResult.TryGet(out string target))
                    return targetResult;
                if (target != null) return target;
            } finally {
                file.Close();
            }

            // On Windows XP we should resolve the paths manually. Normally, we won't get recursion here (even if we do
            // handle it) because `file` would be invalid (the OS couldn't resolve). So no recursion would be raised by
            // this method, but instead by `GetFileHandle`.
            if (unresolvedLink.IsInvalid) {
                return GetLinkTargetResolve(path, true);
            } else {
                return GetLinkTargetResolve(path, unresolvedLink, true);
            }
        }

        /// <summary>
        /// Gets the reparse link information with recursion.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <param name="mustResolve">Return an error if recursion fails that the final target cannot be found.</param>
        /// <returns>The last link the reparse point redirects to.</returns>
        /// <remarks>
        /// Uses specific I/O control codes to the file system to get the reparse points. This is using the Microsoft
        /// specific reparse points. As such, vendor specific reparse points are not parsed as they are unknown.
        /// <para>The following exceptions are returned as an error:</para>
        /// <para>
        /// <see cref="FileNotFoundException"/> - Link <paramref name="path"/> can't be resolved. See the inner
        /// exception for more details.
        /// </para>
        /// </remarks>
        private static Result<string> GetLinkTargetResolve(string path, bool mustResolve)
        {
            var result = GetFileHandle(path, false);
            if (!result.TryGet(out SafeFileHandle file))
                return Result.FromException<string>(result.Error);

            using (file) {
                return GetLinkTargetResolve(path, file, mustResolve);
            }
        }

        /// <summary>
        /// Gets the reparse link information with recursion.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <param name="file">
        /// The already opened handle to the reparse point from <paramref name="path"/>, so it doesn't need to be opened
        /// twice.
        /// </param>
        /// <param name="mustResolve">Return an error if recursion fails that the final target cannot be found.</param>
        /// <returns>The last link the reparse point redirects to.</returns>
        /// <exception cref="ArgumentException">The parameter <paramref name="file"/> is not valid.</exception>
        /// <remarks>
        /// Uses specific I/O control codes to the file system to get the reparse points. This is using the Microsoft
        /// specific reparse points. As such, vendor specific reparse points are not parsed as they are unknown.
        /// <para>The following exceptions are returned as an error:</para>
        /// <para>
        /// <see cref="FileNotFoundException"/> - Link <paramref name="path"/> can't be resolved. See the inner
        /// exception for more details.
        /// </para>
        /// </remarks>
        private static Result<string> GetLinkTargetResolve(string path, SafeFileHandle file, bool mustResolve)
        {
            if (file.IsInvalid)
                throw new ArgumentException("Invalid file handle");

            List<Kernel32.BY_HANDLE_FILE_INFORMATION> recursion = new List<Kernel32.BY_HANDLE_FILE_INFORMATION>();
            bool result = Kernel32.GetFileInformationByHandle(file, out Kernel32.BY_HANDLE_FILE_INFORMATION fileInfoByHandle);
            if (!result)
                return Result.FromException<string>(new FileNotFoundException($"File {path} not found", path));
            recursion.Add(fileInfoByHandle);

            string prevTarget = path;
            Result<string> targetResult = GetLinkTargetDirect(prevTarget, file);
            if (!targetResult.TryGet(out string target))
                return Result.FromException<string>(targetResult.Error);
            if (target == null) return prevTarget;

            string unresolvedLink = target;
            do {
                if (!GetFileHandle(target, false).TryGet(out SafeFileHandle next))
                    return target;
                try {
                    result = Kernel32.GetFileInformationByHandle(next, out fileInfoByHandle);
                    if (result) {
                        // Check to see we have no recursion
                        foreach (Kernel32.BY_HANDLE_FILE_INFORMATION info in recursion) {
                            if (info.FileIndexLow == fileInfoByHandle.FileIndexLow &&
                                info.FileIndexHigh == fileInfoByHandle.FileIndexHigh &&
                                info.VolumeSerialNumber == fileInfoByHandle.VolumeSerialNumber) {
                                // We have recursion.
                                if (mustResolve)
                                    return Result.FromException<string>(new FileNotFoundException($"Link {path} can't be resolved from recursion", target));
                                return unresolvedLink;
                            }
                        }
                        if ((fileInfoByHandle.FileAttributes & Kernel32.FileAttributeFlags.FILE_ATTRIBUTE_REPARSE_POINT) == 0) {
                            return target;
                        }
                        recursion.Add(fileInfoByHandle);
                    } else {
                        return prevTarget;
                    }

                    prevTarget = target;
                    targetResult = GetLinkTargetDirect(prevTarget, next);
                    if (!targetResult.TryGet(out target))
                        return Result.FromException<string>(targetResult.Error);
                    if (target == null) {
                        return prevTarget;
                    }
                } finally {
                    next.Close();
                }
            } while (true);
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

        /// <summary>
        /// Gets the reparse link information without recursion.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <returns>
        /// The link the reparse point redirects to. If the result is <see langword="null"/>, then the reparse tag is
        /// unknown.
        /// </returns>
        /// <remarks>
        /// Uses specific I/O control codes to the file system to get the reparse points. This is using the Microsoft
        /// specific reparse points. As such, vendor specific reparse points are not parsed as they are unknown.
        /// <para>The following exceptions are returned as an error:</para>
        /// <para>
        /// <see cref="FileNotFoundException"/> - Link <paramref name="path"/> can't be resolved. See the inner
        /// exception for more details.
        /// </para>
        /// </remarks>
        private static Result<string> GetLinkTargetDirect(string path)
        {
            var result = GetFileHandle(path, false);
            if (!result.TryGet(out SafeFileHandle file))
                return Result.FromException<string>(result.Error);

            using (file) {
                return GetLinkTargetDirect(path, file);
            }
        }

        /// <summary>
        /// Gets the reparse link information without recursion.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        /// <param name="file">
        /// The already opened handle to the reparse point from <paramref name="path"/>, so it doesn't need to be opened
        /// twice.
        /// </param>
        /// <returns>
        /// The link the reparse point redirects to. If the result is <see langword="null"/>, then the reparse tag is
        /// unknown.
        /// </returns>
        /// <exception cref="ArgumentException">The parameter <paramref name="file"/> is not valid.</exception>
        /// <remarks>
        /// Uses specific I/O control codes to the file system to get the reparse points. This is using the Microsoft
        /// specific reparse points. As such, vendor specific reparse points are not parsed as they are unknown.
        /// <para>The following exceptions are returned as an error:</para>
        /// <para>
        /// <see cref="FileNotFoundException"/> - Link <paramref name="path"/> can't be resolved. See the inner
        /// exception for more details.
        /// </para>
        /// </remarks>
        private static Result<string> GetLinkTargetDirect(string path, SafeFileHandle file)
        {
            if (file.IsInvalid)
                throw new ArgumentException("Invalid file handle");
            CheckReparseStructs();

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
                    return Result.FromException<string>(new FileNotFoundException($"Link {path} can't be resolved", path, ex));
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

                    if (symReparseDataBuffer.PrintNameLength != 0) {
                        return Encoding.Unicode.GetString(symReparseDataBuffer.PathBuffer,
                            symReparseDataBuffer.PrintNameOffset, symReparseDataBuffer.PrintNameLength);
                    }
                    string symTarget = Encoding.Unicode.GetString(symReparseDataBuffer.PathBuffer,
                        symReparseDataBuffer.SubstituteNameOffset, symReparseDataBuffer.SubstituteNameLength);
                    if (symTarget.StartsWith(@"\??\"))
#if NETSTANDARD
                        return symTarget[4..];
#else
                        return symTarget.Substring(4);
#endif
                    return symTarget;
                case Kernel32.IO_REPARSE_TAG_MOUNT_POINT:
                    Kernel32.REPARSE_DATA_BUFFER_Junction junReparseDataBuffer =
                        (Kernel32.REPARSE_DATA_BUFFER_Junction)Marshal.PtrToStructure(outBuffer, typeof(Kernel32.REPARSE_DATA_BUFFER_Junction));

                    // On Windows XP, the PrintNameLength is zero.
                    if (junReparseDataBuffer.PrintNameLength != 0) {
                        return Encoding.Unicode.GetString(junReparseDataBuffer.PathBuffer,
                            junReparseDataBuffer.PrintNameOffset, junReparseDataBuffer.PrintNameLength);
                    }
                    string junTarget = Encoding.Unicode.GetString(junReparseDataBuffer.PathBuffer,
                        junReparseDataBuffer.SubstituteNameOffset, junReparseDataBuffer.SubstituteNameLength);
                    if (junTarget.StartsWith(@"\??\"))
#if NETSTANDARD
                        return junTarget[4..];
#else
                        return junTarget.Substring(4);
#endif
                    return junTarget;
                default:
                    return null;
                }
            } finally {
                if (!outBuffer.Equals(IntPtr.Zero))
                    Marshal.FreeHGlobal(outBuffer);
            }
        }

        public override NodeInfoType Type { get; }

        public ulong VolumeSerialNumber { get; }

        public ulong FileIdLow { get; }

        public ulong FileIdHigh { get; }

        public override string LinkTarget { get; }

        public override string Path { get; }

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
