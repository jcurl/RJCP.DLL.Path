namespace RJCP.IO
{
    using System;

    internal static class OSInfo
    {
        public static bool IsWinNT()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        public static bool IsUnix()
        {
            int platform = (int)Environment.OSVersion.Platform;
            return ((platform == 4) || (platform == 6) || (platform == 128));
        }
    }
}
