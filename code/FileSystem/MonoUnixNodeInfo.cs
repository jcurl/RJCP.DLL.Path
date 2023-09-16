namespace RJCP.IO.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Mono.Unix;
    using Native.Unix;

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
            UnixFileSystemInfo info;
            info = UnixFileSystemInfo.GetFileSystemEntry(path);
            if (info == null || !info.Exists)
                throw new FileNotFoundException($"File {path} not found", path);
            Path = info.FullName;

            if (info.IsSymbolicLink) {
                LinkTarget = GetLinkTarget(path);
                if (resolveLinks) {
                    if (LinkTarget == null)
                        throw new FileNotFoundException($"Link {path} can't be resolved", path);

                    info = UnixFileSystemInfo.GetFileSystemEntry(LinkTarget);
                    if (info == null || !info.Exists || !TargetCanBeResolved(LinkTarget))
                        throw new FileNotFoundException($"Resolved File {LinkTarget} not found", LinkTarget);
                }
            }

            Device = info.Device;
            DeviceType = info.DeviceType;
            Inode = info.Inode;

            if (DeviceType != 0) {
                // We don't care what the device/inode is for a device type file, because it doesn't matter where it is,
                // it's still the same device. We set the upper bit if it's a device, else we clear it, to avoid a
                // common conditions.
                m_HashCode = unchecked((int)DeviceType | (int)0x80000000);
            } else {
                m_HashCode = unchecked(
                    (((int)Device << 16) ^
                    (int)Inode) & 0x7FFFFFFF
                );
            }
        }

        private static string GetLinkTarget(string path)
        {
            using (GLibc6.SafeMallocHandle buffer = GLibc6.realpath(path, IntPtr.Zero)) {
                if (!buffer.IsInvalid)
                    return Marshal.PtrToStringAnsi(buffer.DangerousGetHandle());
            }

            // Getting the resolved link failed, so we open the symbolic link itself.
            return GetLinkTargetDirect(path);
        }

        private static string GetLinkTargetDirect(string path)
        {
            string link;
            unsafe {
                byte* buffer = stackalloc byte[32768];
                int len = GLibc6.readlink(path, buffer, 32768);
                if (len <= 0 || len >= 32768)
                    return null;

                link = Marshal.PtrToStringAnsi((IntPtr)buffer, len);
            }

            IO.Path fullLink = IO.Path.ToPath(link);
            if (fullLink.IsPinned) return link;

            IO.Path fullPath = IO.Path.ToPath(path);
            if (!fullPath.IsPinned)
                fullPath = IO.Path.ToPath(Environment.CurrentDirectory).Append(fullPath);
            fullPath = fullPath.GetParent().Append(fullLink);
            return fullPath.ToString();
        }

        private static bool TargetCanBeResolved(string path)
        {
            using (GLibc6.SafeMallocHandle buffer = GLibc6.realpath(path, IntPtr.Zero)) {
                return !buffer.IsInvalid;
            }
        }

        public override NodeInfoType Type { get { return NodeInfoType.MonoUnix; } }

        public override string LinkTarget { get; }

        public override string Path { get; }

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
