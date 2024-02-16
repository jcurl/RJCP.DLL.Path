namespace RJCP.IO.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using Mono.Unix;
    using Native.Unix;

    /// <summary>
    /// Get file information for Windows Operating Systems.
    /// </summary>
    /// <remarks>
    /// This class depends on glibc v6 to work (the <c>readlink</c> function). Testing with .NET 7 on Ubuntu 22.04 shows that
    /// <c>File.ResolveLinkTarget(path, false)</c> will only return correct results if the <c>path</c> has a file portion.
    /// </remarks>
    [SupportedOSPlatform("linux")]
    internal sealed class MonoUnixNodeInfo : NodeInfo<MonoUnixNodeInfo, MonoUnixExtended>
    {
        private readonly int m_HashCode;

        public MonoUnixNodeInfo(string path, bool resolveLinks)
        {
            UnixFileSystemInfo info;
            info = UnixFileSystemInfo.GetFileSystemEntry(path);
            if (info is null || !info.Exists)
                throw new FileNotFoundException($"File {path} not found", path);
            Path = info.FullName;

            if (info.IsSymbolicLink) {
                LinkTarget = GetLinkTarget(path);
                if (resolveLinks) {
                    if (LinkTarget is null)
                        throw new FileNotFoundException($"Link {path} can't be resolved", path);

                    info = UnixFileSystemInfo.GetFileSystemEntry(LinkTarget);
                    if (info is null || !info.Exists || !TargetCanBeResolved(LinkTarget))
                        throw new FileNotFoundException($"Resolved File {LinkTarget} not found", LinkTarget);
                }
            }

            ExtendedInfo.Device = info.Device;
            ExtendedInfo.DeviceType = info.DeviceType;
            ExtendedInfo.Inode = info.Inode;
            ExtendedInfo.Mode = unchecked((int)info.Protection);
            ExtendedInfo.UserId = info.OwnerUserId;
            ExtendedInfo.GroupId = info.OwnerGroupId;

            if (ExtendedInfo.DeviceType != 0) {
                // We don't care what the device/inode is for a device type file, because it doesn't matter where it is,
                // it's still the same device. We set the upper bit if it's a device, else we clear it, to avoid a
                // common conditions.
                m_HashCode = unchecked((int)ExtendedInfo.DeviceType | (int)0x80000000);
            } else {
                m_HashCode = unchecked(
                    (((int)ExtendedInfo.Device << 16) ^
                    (int)ExtendedInfo.Inode) & 0x7FFFFFFF
                );
            }
        }

        private static string GetLinkTarget(string path)
        {
            // Use the filesystem to get the path, not File.GetFullPath.
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
                if (len is <= 0 or >= 32768)
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

        protected override MonoUnixExtended ExtendedInfo { get; } = new MonoUnixExtended();

        protected override bool Equals(MonoUnixNodeInfo other)
        {
            if (ExtendedInfo.DeviceType != other.ExtendedInfo.DeviceType) return false;
            if (ExtendedInfo.DeviceType != 0) return true;
            return ExtendedInfo.Device == other.ExtendedInfo.Device && ExtendedInfo.Inode == other.ExtendedInfo.Inode;
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }
    }
}
