namespace RJCP.FileInfo
{
    using System;
    using FileSystem;
    using RJCP.IO;

    /// <summary>
    /// File Info Program
    /// </summary>
    /// <remarks>
    /// This program is used to test the file information as obtained by <see cref="FileSystemNodeInfo"/>. This can be
    /// used to test various integration test cases. A test case cannot reliably set up hard links, soft links, etc. but
    /// an integration test is likely better at this and can use the results of this program to check consistency.
    /// </remarks>
    public static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("FileInfo <path> [<path2> ...]");
                return 1;
            }

            FileSystemNodeInfo first = null;
            bool identical = true;
            foreach (string arg in args) {
                Path path = Path.ToPath(arg);
                FileSystemNodeInfo info;
                try {
                    info = new FileSystemNodeInfo(path, false);
                } catch (System.IO.FileNotFoundException ex) {
                    Console.WriteLine($"Couldn't find file: {path} ({ex.Message})");
                    return 1;
                } catch (System.IO.DirectoryNotFoundException ex) {
                    Console.WriteLine($"Couldn't find directory: {path} ({ex.Message})");
                    return 1;
                }

                Console.WriteLine($"Path: {info.Path}");
                Console.WriteLine($" Info Type: {info.Type}");
                Console.WriteLine($" LinkTarget: {info.LinkTarget}");

                switch (info.Type) {
                case NodeInfoType.WindowsExtended:
                case NodeInfoType.WindowsFileInfo:
                    Win32NodeInfo winInfo = new Win32NodeInfo(info);
                    Console.WriteLine($" VolumeSerialNumber: 0x{winInfo.VolumeSerialNumber:x016}");
                    Console.WriteLine($" FileIdHigh:         0x{winInfo.FileIdHigh:x016}");
                    Console.WriteLine($" FileIdLow:          0x{winInfo.FileIdLow:x016}");
                    break;
                case NodeInfoType.MonoUnix:
                    MonoUnixNodeInfo monoInfo = new MonoUnixNodeInfo(info);
                    Console.WriteLine($" DeviceType: 0x{monoInfo.DeviceType:x016}");
                    Console.WriteLine($" Device:     0x{monoInfo.Device:x016}");
                    Console.WriteLine($" I-Node:     0x{monoInfo.Inode:x016}");
                    break;
                }

                FileSystemNodeInfo resolved = null;
                try {
                    resolved = new FileSystemNodeInfo(path, true);
                } catch (System.IO.FileNotFoundException ex) {
                    identical = false;
                    Console.WriteLine($"Couldn't resolve file: {path} ({ex.Message})");
                } catch (System.IO.DirectoryNotFoundException ex) {
                    identical = false;
                    Console.WriteLine($"Couldn't resolve dir: {path} ({ex.Message})");
                }

                if (resolved != null) {
                    if (first == null) {
                        first = resolved;
                    } else {
                        if (first != resolved) identical = false;
                    }
                }
                Console.WriteLine("");
            }

            if (args.Length > 1) {
                if (identical) {
                    Console.WriteLine("Paths are identical");
                    return 0;
                }
                return 1;
            }
            return 0;
        }
    }
}
