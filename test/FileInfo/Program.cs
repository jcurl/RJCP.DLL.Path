namespace RJCP.FileInfo
{
    using System;
    using RJCP.IO;
    using RJCP.IO.Files.Exe;
    using RJCP.IO.FileSystem;

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
                FileSystemNodeInfo info;
                try {
                    info = new FileSystemNodeInfo(arg, false);
                } catch (System.IO.FileNotFoundException ex) {
                    Console.WriteLine($"Couldn't find file: {arg} ({ex.Message})");
                    return 1;
                } catch (System.IO.DirectoryNotFoundException ex) {
                    Console.WriteLine($"Couldn't find directory: {arg} ({ex.Message})");
                    return 1;
                }

                Console.WriteLine($"Path: {info.Path}");
                Console.WriteLine($" Info Type: {info.Type}");
                Console.WriteLine($" LinkTarget: {info.LinkTarget}");

                switch (info.Type) {
                case NodeInfoType.WindowsExtended:
                case NodeInfoType.WindowsFileInfo:
                    Win32Extended winInfo = (Win32Extended)info.Extended;
                    Console.WriteLine($" VolumeSerialNumber: 0x{winInfo.VolumeSerialNumber:x016}");
                    Console.WriteLine($" FileIdHigh:         0x{winInfo.FileIdHigh:x016}");
                    Console.WriteLine($" FileIdLow:          0x{winInfo.FileIdLow:x016}");
                    break;
                case NodeInfoType.MonoUnix:
                    MonoUnixExtended monoInfo = (MonoUnixExtended)info.Extended;
                    Console.WriteLine($" DeviceType: 0x{monoInfo.DeviceType:x016}");
                    Console.WriteLine($" Device:     0x{monoInfo.Device:x016}");
                    Console.WriteLine($" I-Node:     0x{monoInfo.Inode:x016}");
                    Console.WriteLine($" Mode:       0x{monoInfo.Mode:x08}");
                    Console.WriteLine($" UserID:     {monoInfo.UserId}");
                    Console.WriteLine($" GroupID:    {monoInfo.GroupId}");
                    break;
                }

                FileSystemNodeInfo resolved = null;
                try {
                    resolved = new FileSystemNodeInfo(arg, true);
                } catch (System.IO.FileNotFoundException ex) {
                    identical = false;
                    Console.WriteLine($"Couldn't resolve file: {arg} ({ex.Message})");
                } catch (System.IO.DirectoryNotFoundException ex) {
                    identical = false;
                    Console.WriteLine($"Couldn't resolve dir: {arg} ({ex.Message})");
                }

                if (resolved is not null) {
                    if (first is null) {
                        first = resolved;
                    } else {
                        if (first != resolved) identical = false;
                    }
                }

                // Check the file contents:
                FileExecutable fileExecutable = null;
                try {
                    fileExecutable = FileExecutable.GetFile(arg);
                } catch (System.IO.IOException) { /* Pass through */
                } catch (UnauthorizedAccessException) { /* Pass through */
                } catch (ArgumentException) { /* Pass through */
                }

                if (fileExecutable is not null) {
                    Console.WriteLine($" Executable Machine Type: {fileExecutable.MachineType}");
                    Console.WriteLine($" Executable Target OS:    {fileExecutable.TargetOs}");
                    Console.WriteLine($" Executable Architecture: {fileExecutable.ArchitectureSize}");
                    Console.WriteLine($" Executable Is EXE:       {fileExecutable.IsExe}");
                    Console.WriteLine($" Executable Is DLL:       {fileExecutable.IsDll}");
                    Console.WriteLine($" Executable Is LE:        {fileExecutable.IsLittleEndian}");

                    if (fileExecutable is WindowsExecutable winExe) {
                        Console.WriteLine($" Executable Subsystem:    {winExe.Subsystem}");
                        Console.WriteLine($" Executable Word Size:    {winExe.WordSize}");
                        Console.WriteLine($" Executable OS Version:   {winExe.OSVersion}");
                        Console.WriteLine($" Executable Image Vers:   {winExe.ImageVersion}");
                        Console.WriteLine($" Executable SubSys Vers:  {winExe.SubsystemVersion}");
                    }

                    if (fileExecutable is UnixElfExecutable elfExe) {
                        Console.WriteLine($" Executable Is Core:      {elfExe.IsCore}");
                        Console.WriteLine($" Executable Is PIE:       {elfExe.IsPositionIndependent}");
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
