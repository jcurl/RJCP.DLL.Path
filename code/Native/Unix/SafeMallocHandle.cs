namespace RJCP.IO.Native.Unix
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

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

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            override protected bool ReleaseHandle()
            {
                // Here, we must obey all rules for constrained execution regions.
                if (!handle.Equals(IntPtr.Zero))
                    free(handle);
                return true;
            }
        }
    }
}
