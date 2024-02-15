namespace RJCP.FileInfoCheck
{
    using System;
    using System.IO;
    using System.Xml;
    using RJCP.Core.Xml;
    using RJCP.IO.Files.Exe;

    public static class Program
    {
        private const string RootNode = "FileExecutableTest";
        private const string UnixNode = "UnixElfExecutable";
        private const string WinNode = "WinExecutable";

        private const string FileAttr = "file";
        private const string MachineAttr = "machine";
        private const string TargetOsAttr = "targetos";
        private const string IsLittleEndianAttr = "isle";
        private const string IsExeAttr = "isexe";
        private const string IsDllAttr = "isdll";
        private const string ArchSizeAttr = "architecturesize";

        private const string IsCoreAttr = "iscore";
        private const string IsPIEAttr = "ispie";

        private const string WordSizeAttr = "wordsize";
        private const string SubSystemAttr = "subsystem";
        private const string OSVerAttr = "osversion";
        private const string ImgVerAttr = "imgversion";
        private const string SubSysVerAttr = "subsysversion";

        static int Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("FileInfoCheck <file.xml>");
                return 1;
            }

            bool success = Execute(args[0]);
            return success ? 0 : 1;
        }

        static bool Execute(string fileName)
        {
            ThrowHelper.ThrowIfNull(fileName);
            IO.Path baseDir = IO.Path.ToPath(fileName);
            if (!baseDir.IsPinned) baseDir = IO.Path.ToPath(Environment.CurrentDirectory).Append(baseDir);

            using (FileStream file = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return Execute(file, baseDir.GetParent());
            }
        }

        static bool Execute(Stream stream, IO.Path baseDir)
        {
            int pass = 0;
            int fail = 0;

            XmlTreeReader xmlTreeReader = new() {
                Nodes = {
                    new XmlTreeNode(RootNode) {
                        Nodes = {
                            new XmlTreeNode(UnixNode) {
                                ProcessElement = (n, e) => {
                                    FileExecutable exe;
                                    try {
                                        exe = FileExecutable.GetFile(baseDir.Append(e.Reader[FileAttr]));
                                    } catch (FileNotFoundException) {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - file not found)", ConsoleColor.Red);
                                        fail++;
                                        return;
                                    } catch (DirectoryNotFoundException) {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - dir not found)", ConsoleColor.Red);
                                        fail++;
                                        return;
                                    }
                                    if (exe is UnixElfExecutable elfExe) {
                                        bool equals =
                                            Compare(e.Reader, MachineAttr, elfExe.MachineType.ToString()) &&
                                            Compare(e.Reader, TargetOsAttr, elfExe.TargetOs.ToString()) &&
                                            Compare(e.Reader, IsLittleEndianAttr, elfExe.IsLittleEndian.ToString()) &&
                                            Compare(e.Reader, IsExeAttr, elfExe.IsExe.ToString()) &&
                                            Compare(e.Reader, IsDllAttr, elfExe.IsDll.ToString()) &&
                                            Compare(e.Reader, ArchSizeAttr, elfExe.ArchitectureSize.ToString()) &&
                                            Compare(e.Reader, IsCoreAttr, elfExe.IsCore.ToString()) &&
                                            Compare(e.Reader, IsPIEAttr, elfExe.IsPositionIndependent.ToString());

                                        if (equals) {
                                            PrintResult($"File: {e.Reader[FileAttr]}", "(PASSED)", ConsoleColor.Green);
                                            pass++;
                                        } else {
                                            PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED)", ConsoleColor.Red);
                                            PrintCompare("Machine:", elfExe.MachineType.ToString(), e.Reader[MachineAttr]);
                                            PrintCompare("TargetOs:", elfExe.TargetOs.ToString(), e.Reader[TargetOsAttr]);
                                            PrintCompare("Is LE:", elfExe.IsLittleEndian.ToString(), e.Reader[IsLittleEndianAttr]);
                                            PrintCompare("Is Exe:", elfExe.IsExe.ToString(), e.Reader[IsExeAttr]);
                                            PrintCompare("Is DLL:", elfExe.IsDll.ToString(), e.Reader[IsDllAttr]);
                                            PrintCompare("Is Core:", elfExe.IsCore.ToString(), e.Reader[IsCoreAttr]);
                                            PrintCompare("Is PIE:", elfExe.IsPositionIndependent.ToString(), e.Reader[IsPIEAttr]);
                                            PrintCompare("Arch Size:", elfExe.ArchitectureSize.ToString(), e.Reader[ArchSizeAttr]);
                                            fail++;
                                        }
                                    } else {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - is not an ELF exe/dll file)", ConsoleColor.Red);
                                        fail++;
                                    }
                                }
                            },
                            new XmlTreeNode(WinNode) {
                                ProcessElement = (n, e) => {
                                    FileExecutable exe;
                                    try {
                                        exe = FileExecutable.GetFile(baseDir.Append(e.Reader[FileAttr]));
                                    } catch (FileNotFoundException) {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - file not found)", ConsoleColor.Red);
                                        fail++;
                                        return;
                                    } catch (DirectoryNotFoundException) {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - dir not found)", ConsoleColor.Red);
                                        fail++;
                                        return;
                                    }
                                    if (exe is WindowsExecutable winExe) {
                                        bool equals =
                                            Compare(e.Reader, MachineAttr, winExe.MachineType.ToString()) &&
                                            Compare(e.Reader, TargetOsAttr, winExe.TargetOs.ToString()) &&
                                            Compare(e.Reader, IsLittleEndianAttr, winExe.IsLittleEndian.ToString()) &&
                                            Compare(e.Reader, IsExeAttr, winExe.IsExe.ToString()) &&
                                            Compare(e.Reader, IsDllAttr, winExe.IsDll.ToString()) &&
                                            Compare(e.Reader, ArchSizeAttr, winExe.ArchitectureSize.ToString()) &&
                                            Compare(e.Reader, WordSizeAttr, winExe.WordSize.ToString()) &&
                                            Compare(e.Reader, SubSystemAttr, winExe.Subsystem.ToString()) &&
                                            Compare(e.Reader, OSVerAttr, winExe.OSVersion.ToString()) &&
                                            Compare(e.Reader, ImgVerAttr, winExe.ImageVersion.ToString()) &&
                                            Compare(e.Reader, SubSysVerAttr, winExe.SubsystemVersion.ToString());

                                        if (equals) {
                                            PrintResult($"File: {e.Reader[FileAttr]}", "(PASSED)", ConsoleColor.Green);
                                            pass++;
                                        } else {
                                            PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED)", ConsoleColor.Red);
                                            PrintCompare("Machine:", winExe.MachineType.ToString(), e.Reader[MachineAttr]);
                                            PrintCompare("TargetOs:", winExe.TargetOs.ToString(), e.Reader[TargetOsAttr]);
                                            PrintCompare("Is LE:", winExe.IsLittleEndian.ToString(), e.Reader[IsLittleEndianAttr]);
                                            PrintCompare("Is Exe:", winExe.IsExe.ToString(), e.Reader[IsExeAttr]);
                                            PrintCompare("Is DLL:", winExe.IsDll.ToString(), e.Reader[IsDllAttr]);
                                            PrintCompare("Arch Size:", winExe.ArchitectureSize.ToString(), e.Reader[ArchSizeAttr]);
                                            PrintCompare("Word Size:", winExe.WordSize.ToString(), e.Reader[WordSizeAttr]);
                                            PrintCompare("OS Ver:", winExe.OSVersion.ToString(), e.Reader[OSVerAttr]);
                                            PrintCompare("Img Ver:", winExe.ImageVersion.ToString(), e.Reader[ImgVerAttr]);
                                            PrintCompare("Subsys Ver:", winExe.SubsystemVersion.ToString(), e.Reader[SubSysVerAttr]);
                                            fail++;
                                        }
                                    } else {
                                        PrintResult($"File: {e.Reader[FileAttr]}", "(FAILED - s not a Windows PE EXE/DLL file)", ConsoleColor.Red);
                                        fail++;
                                    }
                                }
                            }
                        }
                    }
                }
            };

            xmlTreeReader.Read(stream);
            return fail == 0 && pass > 1;
        }

        static bool Compare(XmlReader reader, string attribute, string expected)
        {
            string value = reader[attribute];
            if (value is null) return false;

            return value.Equals(expected, StringComparison.InvariantCultureIgnoreCase);
        }

        static void PrintCompare(string attr, string actual, string expected)
        {
            if (expected is null) return;

            actual = actual?.Trim();
            expected = expected?.Trim();
            if (!expected.Equals(actual, StringComparison.InvariantCultureIgnoreCase)) {
                Console.WriteLine($"   -> {attr,-12} {expected,-12} {actual,-12}");
            } else {
                Console.WriteLine($"      {attr,-12} {expected,-12} {actual,-12}");
            }
        }

        static void PrintResult(string message, string result, ConsoleColor color)
        {
            ThrowHelper.ThrowIfNull(message);
            ThrowHelper.ThrowIfNull(result);
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.Write($"{message} ");
            Console.ForegroundColor = color;
            Console.Write(result);
            Console.ForegroundColor = oldColor;
            Console.WriteLine(string.Empty);
        }
    }
}
