namespace RJCP.IO.Native.Win32
{
    internal partial class Kernel32
    {
        internal static class FSCTL
        {
            // #define CTL_CODE( DeviceType, Function, Method, Access ) (                 \
            //     ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method) \
            // )

            // #define FILE_DEVICE_FILE_SYSTEM         0x00000009

            // #define METHOD_BUFFERED                 0

            // #define FILE_ANY_ACCESS                 0

            // #define FSCTL_GET_REPARSE_POINT         CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 42, METHOD_BUFFERED, FILE_ANY_ACCESS) // REPARSE_DATA_BUFFER
            public const int GET_REPARSE_POINT = 0x000900A8;
        }
    }
}
