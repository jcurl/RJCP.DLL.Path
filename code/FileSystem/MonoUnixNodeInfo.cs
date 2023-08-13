namespace RJCP.IO.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using Mono.Unix;

    /// <summary>
    /// Get file information for Windows Operating Systems.
    /// </summary>
    /// <remarks>
    /// This class depends on glibc v6 to work (the <c>readlink</c> function). If we ever support .NET 6.0 or later, we
    /// can use methods provided there.
    /// </remarks>
    internal sealed class MonoUnixNodeInfo : NodeInfo<MonoUnixNodeInfo>
    {
        private readonly int m_HashCode;

        public MonoUnixNodeInfo(string path, bool resolveLinks)
        {
            // Used to check for cyclic dependencies (a link depends on a link, depends on a link, ...)
            HashSet<string> resolved = new HashSet<string> {
                path
            };

            UnixFileSystemInfo info;
            bool resolving = resolveLinks;
            string cPath = path;
            int redirects = 40;
            do {
                info = UnixFileSystemInfo.GetFileSystemEntry(cPath);
                if (!info.Exists)
                    throw new FileNotFoundException($"File {path} can't be resolved");

                if (info.IsSymbolicLink) {
                    cPath = GetLinkTarget(cPath);
                    LinkTarget = cPath;
                    if (resolving) {
                        if (resolved.Contains(cPath))
                            throw new FileNotFoundException($"File cyclic dependency at {cPath}");
                        resolved.Add(cPath);
                        redirects--;
                        // If we ever get to too many redirects, we present the user with the last entry. Note, this
                        // makes it very hard to detect large cyclic graphs.
                        if (redirects == 0) resolving = false;
                    }
                }
            } while (resolving && info.IsSymbolicLink);

            Device = info.Device;
            DeviceType = info.DeviceType;
            Inode = info.Inode;

            if (DeviceType != 0) {
                // We don't care what the device/inode is for a device type file, because it doesn't matter where it is,
                // it's still the same device.
                m_HashCode = unchecked((int)DeviceType);
            } else {
                m_HashCode = unchecked(
                    ((int)Device << 16) ^
                    (int)Inode
                );
            }
        }

        private static string GetLinkTarget(string path)
        {
            unsafe {
                byte* buffer = stackalloc byte[32768];
                int len = Native.Unix.GLibc6.readlink(path, buffer, 32768);
                if (len <= 0 || len >= 32768)
                    return string.Empty;
                return Marshal.PtrToStringAnsi((IntPtr)buffer, len);
            }
        }

        public override NodeInfoType Type
        {
            get { return NodeInfoType.MonoUnix; }
        }

        public override string LinkTarget { get; }

        public long Device { get; }

        public long DeviceType { get; }

        public long Inode { get; }

        protected override bool Equals(MonoUnixNodeInfo other)
        {
            if (DeviceType != other.DeviceType) return false;
            if (DeviceType != 0) return true;
            return Device == other.Device && Inode == other.Inode;
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }
    }
}
