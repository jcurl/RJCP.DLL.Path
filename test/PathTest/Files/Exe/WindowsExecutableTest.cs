namespace RJCP.IO.Files.Exe
{
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using NUnit.Framework.Internal;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class WindowsExecutableTest
    {
        private static readonly string File_win32cui_x86_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a014c_exe.bin");
        private static readonly string File_win32gui_x86_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a014c_exe.bin");
        private static readonly string File_win32gui_x86_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a014c_dll.bin");
        private static readonly string File_win32nat_x86_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a014c_sys.bin");
        private static readonly string File_win32cui_x64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a8664_exe.bin");
        private static readonly string File_win32gui_x64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a8664_exe.bin");
        private static readonly string File_win32gui_x64_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a8664_dll.bin");
        private static readonly string File_win32nat_x64_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a8664_sys.bin");
        private static readonly string File_win32cui_arm_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a01c4_exe.bin");
        private static readonly string File_win32gui_arm_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a01c4_exe.bin");
        private static readonly string File_win32gui_arm_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a01c4_dll.bin");
        private static readonly string File_win32nat_arm_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a01c4_sys.bin");
        private static readonly string File_win32cui_a64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_aaa64_exe.bin");
        private static readonly string File_win32gui_a64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_aaa64_exe.bin");
        private static readonly string File_win32cui_a64_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_aaa64_dll.bin");
        private static readonly string File_win32nat_a64_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_aaa64_sys.bin");
        private static readonly string File_win32cui_i6432_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a0200_exe32.bin");
        private static readonly string File_win32gui_i6432_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a0200_exe32.bin");
        private static readonly string File_win32cui_i6432_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a0200_dll32.bin");
        private static readonly string File_win32nat_i6432_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a0200_sys32.bin");
        private static readonly string File_win32cui_i64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a0200_exe.bin");
        private static readonly string File_win32gui_i64_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a0200_exe.bin");
        private static readonly string File_win32gui_i64_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a0200_dll.bin");
        private static readonly string File_win32nat_i64_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a0200_sys.bin");
        private static readonly string File_win32cui_axp_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a0184_exe.bin");
        private static readonly string File_win32gui_axp_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32gui_a0184_exe.bin");
        private static readonly string File_win32cui_axp_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32cui_a0184_dll.bin");
        private static readonly string File_win32nat_axp_sys = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32nat_a0184_sys.bin");

        private static readonly string File_win32pos_x86_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32pos_a014c_exe.bin");
        private static readonly string File_win32pos_x86_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win32pos_a014c_dll.bin");

        private static readonly string File_win95cui_x86_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win95cui_a014c_exe.bin");
        private static readonly string File_win95gui_x86_exe = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win95gui_a014c_exe.bin");
        private static readonly string File_win95gui_x86_dll = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win95gui_a014c_dll.bin");
        private static readonly string File_win95nat_x86_vxd = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "win95vxd.bin");

        private static readonly string File_dos1 = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "dos_header.bin");
        private static readonly string File_dos2 = Path.Combine(Deploy.TestDirectory, "TestResources", "WinBinary", "dos_incomplete.bin");

        public class PEHeaderProperties
        {
            public PEHeaderProperties(string fileName, FileMachineType machineType, FileTargetOs targetOs, bool exe, bool dll, int archSize, WindowsSubsystem subsystem, int wordSize)
            {
                FileName = fileName;
                MachineType = machineType;
                TargetOs = targetOs;
                IsExe = exe;
                IsDll = dll;
                ArchitectureSize = archSize;
                SubSystem = subsystem;
                WordSize = wordSize;
            }

            public string FileName { get; }

            public FileMachineType MachineType { get; }

            public FileTargetOs TargetOs { get; }

            public bool IsExe { get; }

            public bool IsDll { get; }

            public int ArchitectureSize { get; }

            public WindowsSubsystem SubSystem { get; }

            public int WordSize { get; }

            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_x86_exe, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_i386_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_x86_exe, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_i386_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_x86_dll, FileMachineType.Intel386, FileTargetOs.Windows, false, true, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_i386_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_x86_sys, FileMachineType.Intel386, FileTargetOs.Windows, false, false, 32, WindowsSubsystem.WindowsNativeDriverSys, 32)
                        ).SetName("Windows_i386_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_x64_exe, FileMachineType.Amd64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsConsole, 64)
                        ).SetName("Windows_Amd64_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_x64_exe, FileMachineType.Amd64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsGui, 64)
                        ).SetName("Windows_Amd64_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_x64_dll, FileMachineType.Amd64, FileTargetOs.Windows, false, true, 64, WindowsSubsystem.WindowsGui, 64)
                        ).SetName("Windows_Amd64_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_x64_sys, FileMachineType.Amd64, FileTargetOs.Windows, false, false, 64, WindowsSubsystem.WindowsNativeDriverSys, 64)
                        ).SetName("Windows_Amd64_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_arm_exe, FileMachineType.Arm, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_ArmNT_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_arm_exe, FileMachineType.Arm, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_ArmNT_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_arm_dll, FileMachineType.Arm, FileTargetOs.Windows, false, true, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_ArmNT_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_arm_sys, FileMachineType.Arm, FileTargetOs.Windows, false, false, 32, WindowsSubsystem.WindowsNativeDriverSys, 32)
                        ).SetName("Windows_ArmNT_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_a64_exe, FileMachineType.Arm64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsConsole, 64)
                        ).SetName("Windows_Arm64_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_a64_exe, FileMachineType.Arm64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsGui, 64)
                        ).SetName("Windows_Arm64_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_a64_dll, FileMachineType.Arm64, FileTargetOs.Windows, false, true, 64, WindowsSubsystem.WindowsConsole, 64)
                        ).SetName("Windows_Arm64_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_a64_sys, FileMachineType.Arm64, FileTargetOs.Windows, false, false, 64, WindowsSubsystem.WindowsNativeDriverSys, 64)
                        ).SetName("Windows_Arm64_Native_Sys");

                    // Interesting this is a 64-bit architecture with a wordsize of 32-bit
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_i6432_exe, FileMachineType.Itanium64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_IA6432_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_i6432_exe, FileMachineType.Itanium64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_IA6432_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_i6432_dll, FileMachineType.Itanium64, FileTargetOs.Windows, false, true, 64, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_IA6432_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_i6432_sys, FileMachineType.Itanium64, FileTargetOs.Windows, false, false, 64, WindowsSubsystem.WindowsNativeDriverSys, 32)
                        ).SetName("Windows_IA6432_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_i64_exe, FileMachineType.Itanium64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsConsole, 64)
                        ).SetName("Windows_IA64_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_i64_exe, FileMachineType.Itanium64, FileTargetOs.Windows, true, false, 64, WindowsSubsystem.WindowsGui, 64)
                        ).SetName("Windows_IA64_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_i64_dll, FileMachineType.Itanium64, FileTargetOs.Windows, false, true, 64, WindowsSubsystem.WindowsGui, 64)
                        ).SetName("Windows_IA64_GUI_Dll");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32nat_i64_sys, FileMachineType.Itanium64, FileTargetOs.Windows, false, false, 64, WindowsSubsystem.WindowsNativeDriverSys, 64)
                        ).SetName("Windows_IA64_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_axp_exe, FileMachineType.Alpha, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_Alpha_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32gui_axp_exe, FileMachineType.Alpha, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Windows_Alpha_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32cui_axp_dll, FileMachineType.Alpha, FileTargetOs.Windows, false, true, 32, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Windows_Alpha_GUI_Dll");
                    yield return new TestCaseData(   // This driver is also a DLL.
                        new PEHeaderProperties(File_win32nat_axp_sys, FileMachineType.Alpha, FileTargetOs.Windows, false, true, 32, WindowsSubsystem.WindowsNativeDriverSys, 32)
                        ).SetName("Windows_Alpha_Native_Sys");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win95cui_x86_exe, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsConsole, 32)
                        ).SetName("Win95_i386_Console_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win95gui_x86_exe, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Win95_i386_GUI_Exe");
                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win95gui_x86_dll, FileMachineType.Intel386, FileTargetOs.Windows, false, true, 32, WindowsSubsystem.WindowsGui, 32)
                        ).SetName("Win95_i386_GUI_DLL");

                    yield return new TestCaseData(
                        new PEHeaderProperties(File_win32pos_x86_exe, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.ServicesForUnix, 32)
                        ).SetName("Windows_i386_Posix_Exe");
                    yield return new TestCaseData(   // libm.so is an EXE and not a DLL.
                        new PEHeaderProperties(File_win32pos_x86_dll, FileMachineType.Intel386, FileTargetOs.Windows, true, false, 32, WindowsSubsystem.ServicesForUnix, 64)
                        ).SetName("Windows_i386_Posix_Dll");
                }
            }
        }

        public class InvalidFiles
        {
            public InvalidFiles(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; }

            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(new InvalidFiles(File_dos1)).SetName("DOS_Header");
                    yield return new TestCaseData(new InvalidFiles(File_dos2)).SetName("DOS_Incomplete");

                    // The Win95 VXD has a LE header.
                    yield return new TestCaseData(new InvalidFiles(File_win95nat_x86_vxd)).SetName("Win95_i386_Native_VXD");
                }
            }
        }

        [TestCaseSource(typeof(PEHeaderProperties), nameof(PEHeaderProperties.TestCases))]
        public void Windows(PEHeaderProperties test)
        {
            FileExecutable exe = FileExecutable.GetFile(test.FileName);
            Assert.That(exe, Is.Not.Null);
            Assert.That(exe, Is.TypeOf<WindowsExecutable>());
            Assert.That(exe.MachineType, Is.EqualTo(test.MachineType));
            Assert.That(exe.TargetOs, Is.EqualTo(test.TargetOs));
            Assert.That(exe.IsExe, Is.EqualTo(test.IsExe));
            Assert.That(exe.IsDll, Is.EqualTo(test.IsDll));
            Assert.That(exe.IsLittleEndian, Is.True);
            Assert.That(exe.ArchitectureSize, Is.EqualTo(test.ArchitectureSize));

            WindowsExecutable winExe = (WindowsExecutable)exe;
            Assert.That(winExe.Subsystem, Is.EqualTo(test.SubSystem));
            Assert.That(winExe.WordSize, Is.EqualTo(test.WordSize));
        }

        [TestCaseSource(typeof(InvalidFiles), nameof(InvalidFiles.TestCases))]
        public void WindowsInvalid(InvalidFiles test)
        {
            FileExecutable exe = FileExecutable.GetFile(test.FileName);
            Assert.That(exe, Is.Null.Or.Not.TypeOf<WindowsExecutable>());
        }
    }
}
