namespace RJCP.IO.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class NtDll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_NAME_INFORMATION
        { // OBJECT_INFORMATION_CLASS.ObjectNameInformation
            public UNICODE_STRING Name;
        }
    }
}
