namespace RJCP.IO.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
#if NETFRAMEWORK
    using System.Runtime.ConstrainedExecution;
#endif

    [SupportedOSPlatform("linux")]
    internal static partial class GLibc6
    {
        public class SafeMallocHandle : SafeHandle
        {
            public SafeMallocHandle() : base(IntPtr.Zero, true) { }

            public SafeMallocHandle(IntPtr buffer) : base(IntPtr.Zero, true)
            {
                handle = buffer;
            }

            public override bool IsInvalid
            {
                get { return (handle.Equals(IntPtr.Zero)); }
            }

#if NETFRAMEWORK
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
            protected override bool ReleaseHandle()
            {
                // Here, we must obey all rules for constrained execution regions.
                if (!handle.Equals(IntPtr.Zero))
                    free(handle);
                return true;
            }
        }
    }
}
