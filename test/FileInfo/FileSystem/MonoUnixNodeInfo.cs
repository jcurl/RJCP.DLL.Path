namespace RJCP.FileInfo.FileSystem
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using RJCP.IO;

    /// <summary>
    /// MonoUnixNodeInfo that is using reflection to access internal details, useful for integration testing.
    /// </summary>
    /// <remarks>
    /// The test program should print the internal details that can be used to validate the behaviour of the software
    /// with integration tests. It should not have them exposed to the user, as the primary intent of
    /// <see cref="FileSystemNodeInfo"/> is for comparison of files.
    /// </remarks>
    internal class MonoUnixNodeInfo
    {
        private readonly object m_Instance;
        private readonly Type m_Type;

        public MonoUnixNodeInfo(FileSystemNodeInfo nodeInfo)
        {
            if (nodeInfo == null) throw new ArgumentNullException(nameof(nodeInfo));
            Assembly assembly = Assembly.GetAssembly(typeof(FileSystemNodeInfo));
            if (assembly == null) return;

            m_Type = assembly.GetType("RJCP.IO.FileSystem.MonoUnixNodeInfo");
            if (m_Type == null) return;

            FieldInfo nodeInfoField = typeof(FileSystemNodeInfo).GetField("m_NodeInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (nodeInfoField == null) return;

            m_Instance = nodeInfoField.GetValue(nodeInfo);
        }

        private T GetProperty<T>(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (m_Instance == null) throw new InvalidOperationException("Object not defined");

            return (T)m_Type.InvokeMember(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
                null, m_Instance, null, CultureInfo.InvariantCulture);
        }

        public long Device
        {
            get { return GetProperty<long>(nameof(Device)); }
        }

        public long DeviceType
        {
            get { return GetProperty<long>(nameof(DeviceType)); }
        }

        public long Inode
        {
            get { return GetProperty<long>(nameof(Inode)); }
        }
    }
}
