namespace RJCP.IO
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class WindowsPathTest
    {
        [Flags]
        public enum WinPathFeature
        {
            None = 0,
            IsPinned = 1,
            IsDos = 2,
            IsUnc = 4
        }

        [TestCase(null, "", "", WinPathFeature.None)]
        [TestCase("", "", "", WinPathFeature.None)]
        [TestCase("X", "", "X", WinPathFeature.None)]
        [TestCase("/", "", @"\", WinPathFeature.IsPinned)]     // Pinned, because '\' is the beginning of the filesystem
        [TestCase(@"\", "", @"\", WinPathFeature.IsPinned)]    // Pinned, because '\' is the beginning of the filesystem
        [TestCase("foo", "", "foo", WinPathFeature.None)]
        [TestCase(@"\foo", "", @"\foo", WinPathFeature.IsPinned)]
        [TestCase("/foo", "", @"\foo", WinPathFeature.IsPinned)]
        [TestCase(@"foo\", "", @"foo\", WinPathFeature.None)]
        [TestCase(@"\foo\", "", @"\foo\", WinPathFeature.IsPinned)]
        [TestCase(@"/foo\", "", @"\foo\", WinPathFeature.IsPinned)]
        [TestCase("foo/", "", @"foo\", WinPathFeature.None)]
        [TestCase(@"\foo/", "", @"\foo\", WinPathFeature.IsPinned)]
        [TestCase("/foo/", "", @"\foo\", WinPathFeature.IsPinned)]
        [TestCase(@"\foo\bar", "", @"\foo\bar", WinPathFeature.IsPinned)]
        [TestCase(@"\foo\bar\", "", @"\foo\bar\", WinPathFeature.IsPinned)]
        [TestCase("/foo/bar", "", @"\foo\bar", WinPathFeature.IsPinned)]
        [TestCase("/foo/bar/", "", @"\foo\bar\", WinPathFeature.IsPinned)]
        [TestCase("C:", "C:", "C:", WinPathFeature.IsDos)]
        [TestCase(@"C:\", "C:", @"C:\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase("C:/", "C:", @"C:\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase(@"C:foo", "C:", @"C:foo", WinPathFeature.IsDos)]
        [TestCase(@"C:\foo", "C:", @"C:\foo", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase("C:/foo", "C:", @"C:\foo", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase(@"C:foo\", "C:", @"C:foo\", WinPathFeature.IsDos)]
        [TestCase(@"C:\foo\", "C:", @"C:\foo\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase(@"C:/foo\", "C:", @"C:\foo\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase("C:foo/", "C:", @"C:foo\", WinPathFeature.IsDos)]
        [TestCase(@"C:\foo/", "C:", @"C:\foo\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase("C:/foo/", "C:", @"C:\foo\", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase(@"\\server", @"\\server", @"\\server", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\", @"\\server\", @"\\server\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share", @"\\server\share", @"\\server\share", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share\", @"\\server\share", @"\\server\share\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\.\COM10", @"\\.\COM10", @"\\.\COM10", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server", @"\\server", @"\\server", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/", @"\\server\", @"\\server\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share", @"\\server\share", @"\\server\share", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share/", @"\\server\share", @"\\server\share\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//?/Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//./Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//?/Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}/", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\?\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//./Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}/", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}", @"\\.\Volume{36c57c01-86d6-4a35-b1c9-eb33bb731e38}\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//./COM10", @"\\.\COM10", @"\\.\COM10", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share\foo", @"\\server\share", @"\\server\share\foo", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share\foo\", @"\\server\share", @"\\server\share\foo\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share\foo\bar", @"\\server\share", @"\\server\share\foo\bar", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\server\share\foo\bar\", @"\\server\share", @"\\server\share\foo\bar\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share/foo", @"\\server\share", @"\\server\share\foo", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share/foo/", @"\\server\share", @"\\server\share\foo\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share/foo/bar", @"\\server\share", @"\\server\share\foo\bar", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase("//server/share/foo/bar/", @"\\server\share", @"\\server\share\foo\bar\", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"C:\Dokü\500€.txt", @"C:", @"C:\Dokü\500€.txt", WinPathFeature.IsDos | WinPathFeature.IsPinned)]
        [TestCase(@"\\192.168.1.1\share\file.txt", @"\\192.168.1.1\share", @"\\192.168.1.1\share\file.txt", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\::1\share\file.txt", @"\\::1\share", @"\\::1\share\file.txt", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\foo*\share", @"\\foo*\share", @"\\foo*\share", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\x_y\share", @"\\x_y\share", @"\\x_y\share", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        [TestCase(@"\\鳥\目\山水.txt", @"\\鳥\目", @"\\鳥\目\山水.txt", WinPathFeature.IsUnc | WinPathFeature.IsPinned)]
        public void ParsePath(string path, string rootVolume, string expectedPath, WinPathFeature features)
        {
            WindowsPath p = new WindowsPath(path);
            Console.WriteLine($"{p}");

            Assert.Multiple(() => {
                Assert.That(p.ToString(), Is.EqualTo(expectedPath));
                Assert.That(p.RootVolume, Is.EqualTo(rootVolume));
                Assert.That(p.IsPinned, Is.EqualTo(features.HasFlag(WinPathFeature.IsPinned)));
                Assert.That(p.IsDos, Is.EqualTo(features.HasFlag(WinPathFeature.IsDos)));
                Assert.That(p.IsUnc, Is.EqualTo(features.HasFlag(WinPathFeature.IsUnc)));
            });
        }

        [TestCase(@"ü:\")]
        [TestCase(@"€:\")]
        [TestCase(@"\\server\\share")]
        [TestCase(@"C:\foo\D:")]
        [TestCase(@"C::")]
        [TestCase(@"\\server\share\c:\root\dir")]
        public void InvalidPath(string path)
        {
            Assert.That(() => {
                _ = new WindowsPath(path);
            }, Throws.TypeOf<ArgumentException>());
        }

        [TestCase("C:.", "C:")]
        [TestCase(@"C:\..", null)]
        [TestCase("C:..", "C:..")]
        [TestCase(@"C:\foo\..", @"C:\")]
        [TestCase(@"C:\foo\bar\..", @"C:\foo")]
        [TestCase(@"\foo\..", @"\")]
        [TestCase(@"\..", null)]
        [TestCase(@"\foo\..\..", null)]
        [TestCase(".", "")]
        [TestCase(@".\", "")]
        [TestCase(@"\.", @"\")]
        [TestCase("..", "..")]
        [TestCase(@"..\..\.", @"..\..")]
        [TestCase(@".\..\..", @"..\..")]
        [TestCase(@"..\foo", @"..\foo")]
        [TestCase(@"..\foo\..", @"..")]
        [TestCase(@"..\foo\bar\..", @"..\foo")]
        [TestCase(@"..\foo\bar\..\..\..", @"..\..")]
        [TestCase(@"\\.\.", @"\\.\.")]
        [TestCase(@"\\..\.", @"\\..\.")]
        [TestCase(@"\\server\.", @"\\server\.")]
        [TestCase(@"\\server\..", @"\\server\..")]
        [TestCase(@"\\server\share\.", @"\\server\share\")]
        [TestCase(@"\\server\share\..", null)]
        [TestCase(@"\\server\share\foo\..", @"\\server\share")]
        [TestCase(@"\\server\share\foo\..\bar", @"\\server\share\bar")]
        [TestCase(@"\\server\share\foo\..\bar\..", @"\\server\share")]
        [TestCase(@"\\server\share\foo\bar\..\..", @"\\server\share")]
        [TestCase(@"\\server\share\foo\bar\..\..\", @"\\server\share\")]
        [TestCase(@"C:\.", @"C:\")]
        [TestCase(@"C:\.\", @"C:\")]
        [TestCase(@"C:\foo\.", @"C:\foo")]
        [TestCase(@"C:\foo\.\", @"C:\foo\")]
        [TestCase(@"C:\.\foo", @"C:\foo")]
        [TestCase(@"C:..\foo\.", @"C:..\foo")]
        [TestCase(@"C:..\foo\..\..", @"C:..\..")]
        [TestCase(@".\bar", "bar")]
        [TestCase(@"\.\bar", @"\bar")]
        [TestCase(@"foo\\bar", @"foo\bar")]
        [TestCase(@"C:\\foo", @"C:\foo")]
        [TestCase(@"\\server\share\\foo", @"\\server\share\foo")]
        public void NormalizedPath(string path, string expectedPath)
        {
            if (expectedPath == null) {
                Assert.That(() => {
                    _ = new WindowsPath(path);
                }, Throws.TypeOf<ArgumentException>());
            } else {
                WindowsPath p = new WindowsPath(path);
                Assert.That(p.ToString(), Is.EqualTo(expectedPath));
            }
        }

        [TestCase(@"\", @"\")]
        [TestCase("C:", "C:")]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\foo", @"C:\foo")]
        [TestCase(@"C:\foo\", @"C:\foo")]
        [TestCase(@"\foo", @"\foo")]
        [TestCase(@"\foo\", @"\foo")]
        [TestCase(@"\\server", @"\\server")]
        [TestCase(@"\\server\", @"\\server\")]
        [TestCase(@"\\server\share", @"\\server\share")]
        [TestCase(@"\\server\share\", @"\\server\share")]
        [TestCase(@"\\server\share\foo", @"\\server\share\foo")]
        [TestCase(@"\\server\share\foo\", @"\\server\share\foo")]
        public void TrimPath(string path, string expectedPath)
        {
            WindowsPath p = new WindowsPath(path);
            Assert.That(p.Trim().ToString(), Is.EqualTo(expectedPath));
        }

        [TestCase(@"C:", @"C:..")]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\foo", @"C:\")]
        [TestCase(@"C:\foo\", @"C:\")]
        [TestCase(@"C:\foo\bar", @"C:\foo")]
        [TestCase(@"C:\foo\bar\", @"C:\foo")]
        [TestCase("foo", "")]
        [TestCase(@"foo\", "")]
        [TestCase(@"foo\bar", "foo")]
        [TestCase(@"foo\bar\", "foo")]
        [TestCase(@"\", @"\")]
        [TestCase(@"\foo", @"\")]
        [TestCase(@"\foo\", @"\")]
        [TestCase(@"\foo\bar", @"\foo")]
        [TestCase(@"\foo\bar\", @"\foo")]
        [TestCase(@"\\server", @"\\server")]
        [TestCase(@"\\server\", @"\\server\")]
        [TestCase(@"\\server\share", @"\\server\share")]
        [TestCase(@"\\server\share\", @"\\server\share")]
        [TestCase(@"\\server\share\foo", @"\\server\share")]
        [TestCase(@"\\server\share\foo\", @"\\server\share")]
        [TestCase(@"\\server\share\foo\bar", @"\\server\share\foo")]
        [TestCase(@"\\server\share\foo\bar\", @"\\server\share\foo")]
        [TestCase("", "..")]
        [TestCase(@"..\foo", "..")]
        [TestCase(@"..\..", @"..\..\..")]
        public void GetParent(string path, string expectedNewPath)
        {
            WindowsPath p = new WindowsPath(path);
            Assert.That(p.GetParent().ToString(), Is.EqualTo(expectedNewPath));
        }

        [TestCase(@"C:\A\B\C", @"C:\A\B\C\D\E", @"..\..")]        // DOS, with/without trailing '\'
        [TestCase(@"C:\A\B\C", @"C:\A\B\C\D", "..")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\C", "")]
        [TestCase(@"C:\A\B\C", @"C:\A\B", "C")]
        [TestCase(@"C:\A\B\C", @"C:\A", @"B\C")]
        [TestCase(@"C:\A\B\C", @"C:\", @"A\B\C")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\X", @"..\C")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\X\Y", @"..\..\C")]
        [TestCase(@"C:\A\B\C\D", @"C:\A\B\X\Y", @"..\..\C\D")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C\D\E", @"..\..")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C\D", "..")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C", "")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B", "C")]
        [TestCase(@"C:\A\B\C\", @"C:\A", @"B\C")]
        [TestCase(@"C:\A\B\C\", @"C:\", @"A\B\C")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\X", @"..\C")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\X\Y", @"..\..\C")]
        [TestCase(@"C:\A\B\C\D\", @"C:\A\B\X\Y", @"..\..\C\D")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\C\D\E\", @"..\..")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\C\D\", "..")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\C\", "", @"C:\A\B\C\")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\", "C")]
        [TestCase(@"C:\A\B\C", @"C:\A\", @"B\C")]
        [TestCase(@"C:\A\B\C", @"C:\", @"A\B\C")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\X\", @"..\C")]
        [TestCase(@"C:\A\B\C", @"C:\A\B\X\Y\", @"..\..\C")]
        [TestCase(@"C:\A\B\C\D", @"C:\A\B\X\Y\", @"..\..\C\D")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C\D\E\", @"..\..")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C\D\", "..")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\C\", "", @"C:\A\B\C\")]  // Appending nothing results in the base path
        [TestCase(@"C:\A\B\C\", @"C:\A\B\", "C")]
        [TestCase(@"C:\A\B\C\", @"C:\A\", @"B\C")]
        [TestCase(@"C:\A\B\C\", @"C:\", @"A\B\C")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\X\", @"..\C")]
        [TestCase(@"C:\A\B\C\", @"C:\A\B\X\Y\", @"..\..\C")]
        [TestCase(@"C:\A\B\C\D\", @"C:\A\B\X\Y\", @"..\..\C\D")]
        [TestCase(@"A\B\C", @"A\B\C\D\E", @"..\..")]              // Relative paths
        [TestCase(@"A\B\C", @"A\B\C\D", "..")]
        [TestCase(@"A\B\C", @"A\B\C", "")]
        [TestCase(@"A\B\C", @"A\B", "C")]
        [TestCase(@"A\B\C", @"A", @"B\C")]
        [TestCase(@"A\B\C", @"", @"A\B\C")]
        [TestCase(@"A\B\C", @"A\B\X", @"..\C")]
        [TestCase(@"A\B\C", @"A\B\X\Y", @"..\..\C")]
        [TestCase(@"A\B\C\D", @"A\B\X\Y", @"..\..\C\D")]
        [TestCase(@"\A\B\C", @"\A\B\C\D\E", @"..\..")]              // Pinned paths
        [TestCase(@"\A\B\C", @"\A\B\C\D", "..")]
        [TestCase(@"\A\B\C", @"\A\B\C", "")]
        [TestCase(@"\A\B\C", @"\A\B", "C")]
        [TestCase(@"\A\B\C", @"\A", @"B\C")]
        [TestCase(@"\A\B\C", @"\", @"A\B\C")]
        [TestCase(@"\A\B\C", @"\A\B\X", @"..\C")]
        [TestCase(@"\A\B\C", @"\A\B\X\Y", @"..\..\C")]
        [TestCase(@"\A\B\C\D", @"\A\B\X\Y", @"..\..\C\D")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\a\b\c\d\e", @"..\..")]  // UNC paths
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\a\b\c\d", "..")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\a\b\c", "")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\a\b", "c")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\a", @"b\c")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh\", @"a\b\c")]
        [TestCase(@"\\srv\sh\a\b\c", @"\\srv\sh", @"a\b\c")]
        [TestCase(@"D:A\B\C", @"D:A\B\C\D\E", @"..\..")]              // DOS Relative paths
        [TestCase(@"D:A\B\C", @"D:A\B\C\D", "..")]
        [TestCase(@"D:A\B\C", @"D:A\B\C", "")]
        [TestCase(@"D:A\B\C", @"D:A\B", "C")]
        [TestCase(@"D:A\B\C", @"D:A", @"B\C")]
        [TestCase(@"D:A\B\C", @"D:", @"A\B\C")]
        [TestCase(@"D:A\B\C", @"D:A\B\X", @"..\C")]
        [TestCase(@"D:A\B\C", @"D:A\B\X\Y", @"..\..\C")]
        [TestCase(@"D:A\B\C\D", @"D:A\B\X\Y", @"..\..\C\D")]
        [TestCase(@"X:\A\B\C", @"Y:\A\B\C", @"X:\A\B\C")]            // Different roots or pinned
        [TestCase(@"\A\B\C", @"X:\A\B\C", @"\A\B\C", @"X:\A\B\C")]     //   X:\A\B\C + \A\B\C = X:\A\B\C (not \A\B\C)
        [TestCase(@"X:\A\B\C", @"\A\B\C", @"X:\A\B\C")]
        [TestCase(@"A\B\C", @"Y:\A\B\C", @"A\B\C", @"Y:\A\B\C\A\B\C")] //   Y:\A\B\C + A\B\C = Y:\A\B\C\A\B\C (not A\B\C)
        [TestCase(@"Y:\A\B\C", @"A\B\C", @"Y:\A\B\C")]
        [TestCase(@"A\B\C", @"\A\B\C", @"A\B\C", @"\A\B\C\A\B\C")]     //   \A\B\C + A\B\C = \A\B\C\A\B\C (not A\B\C)
        [TestCase(@"\A\B\C", @"A\B\C", @"\A\B\C")]
        [TestCase(@"A\B\C", @"\\srv\sh\A\B\C", @"A\B\C", @"\\srv\sh\A\B\C\A\B\C")] // \\srv\sh\A\B\C + A\B\C = \\srv\sh\A\B\C\A\B\C (not A\B\C)
        [TestCase(@"\\srv\sh\A\B\C", @"A\B\C", @"\\srv\sh\A\B\C")]
        [TestCase(@"\\SRV\SH\A\B\C", @"\\srv\sh\A\B\C", "", @"\\srv\sh\A\B\C")]  // Ignore case
        [TestCase(@"A\B\C", @"A\b\c", "", @"A\b\c")]
        public void GetRelative(string path, string basePath, string expected, string append = null)
        {
            WindowsPath p = new WindowsPath(path);
            WindowsPath b = new WindowsPath(basePath);
            Path r = p.GetRelative(b);
            Assert.That(r.ToString(), Is.EqualTo(expected));

            // Appending the relative path to the base path should result in the original path. There are exceptions:
            // - When the path is unpinned, the basePath is pinned, the path is returned. Appending the base path with
            //   the unpinned path cannot result in the original unpinned path.
            Path o = b.Append(r);
            if (append == null) {
                Assert.That(o.ToString(), Is.EqualTo(p.Trim().ToString()));
            } else {
                Assert.That(o.ToString(), Is.EqualTo(append));
            }
        }

        [TestCase("", "", "")]
        [TestCase("", @"\", @"\")]
        [TestCase("", "A", "A")]
        [TestCase("", @"A\", @"A\")]
        [TestCase("", @"X:", @"X:")]
        [TestCase("", @"X:foo", @"X:foo")]
        [TestCase("", @"X:\foo", @"X:\foo")]
        [TestCase("", @"\\srv\sh", @"\\srv\sh")]
        [TestCase("", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase("bar", "", "bar")]
        [TestCase("bar", @"\", @"\")]
        [TestCase("bar", "A", @"bar\A")]
        [TestCase("bar", @"A\", @"bar\A\")]
        [TestCase("bar", @"X:", "X:bar")]
        [TestCase("bar", @"X:foo", @"X:bar\foo")]
        [TestCase("bar", @"X:foo\", @"X:bar\foo\")]
        [TestCase("bar", @"X:\foo", @"X:\foo")]
        [TestCase("bar", @"\\srv\sh", @"\\srv\sh")]
        [TestCase("bar", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"bar\", "", @"bar\")]
        [TestCase(@"bar\", @"\", @"\")]
        [TestCase(@"bar\", "A", @"bar\A")]
        [TestCase(@"bar\", @"A\", @"bar\A\")]
        [TestCase(@"bar\", @"X:", @"X:bar\")]
        [TestCase(@"bar\", @"X:foo", @"X:bar\foo")]
        [TestCase(@"bar\", @"X:foo\", @"X:bar\foo\")]
        [TestCase(@"bar\", @"X:\foo", @"X:\foo")]
        [TestCase(@"bar\", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"bar\", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:", "", @"C:")]
        [TestCase(@"C:", @"\", @"C:\")]
        [TestCase(@"C:", "A", "C:A")]
        [TestCase(@"C:", @"A\", @"C:A\")]
        [TestCase(@"C:", @"X:", null)]
        [TestCase(@"C:", @"X:foo", null)]
        [TestCase(@"C:", @"X:\foo", @"X:\foo")]
        [TestCase(@"C:", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"C:", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:Z", "", @"C:Z")]
        [TestCase(@"C:Z", @"\", @"C:\")]
        [TestCase(@"C:Z", "A", @"C:Z\A")]
        [TestCase(@"C:Z", @"A\", @"C:Z\A\")]
        [TestCase(@"C:Z", @"X:", null)]
        [TestCase(@"C:Z", @"X:foo", null)]
        [TestCase(@"C:Z", @"X:\foo", @"X:\foo")]
        [TestCase(@"C:Z", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"C:Z", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:\", "", @"C:\")]
        [TestCase(@"C:\", @"\", @"C:\")]
        [TestCase(@"C:\", "A", @"C:\A")]
        [TestCase(@"C:\", @"A\", @"C:\A\")]
        [TestCase(@"C:\", @"X:", null)]
        [TestCase(@"C:\", @"X:foo", null)]
        [TestCase(@"C:\", @"X:\foo", @"X:\foo")]
        [TestCase(@"C:\", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"C:\", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:\A", "", @"C:\A")]
        [TestCase(@"C:\A", @"\", @"C:\")]
        [TestCase(@"C:\A", @"B\C", @"C:\A\B\C")]
        [TestCase(@"C:\A", @"B\C\", @"C:\A\B\C\")]
        [TestCase(@"C:\A", @"C:B\C", @"C:\A\B\C")]
        [TestCase(@"C:\A", @"C:B\C\", @"C:\A\B\C\")]
        [TestCase(@"C:\A", @"C:\B\C", @"C:\B\C")]
        [TestCase(@"C:\A", @"C:\B\C\", @"C:\B\C\")]
        [TestCase(@"C:\A", @"X:", null)]
        [TestCase(@"C:\A", @"X:foo", null)]
        [TestCase(@"C:\A", @"X:\foo", @"X:\foo")]
        [TestCase(@"C:\A", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"C:\A", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:\A\", "", @"C:\A\")]
        [TestCase(@"C:\A\", @"\", @"C:\")]
        [TestCase(@"C:\A\", @"B\C", @"C:\A\B\C")]
        [TestCase(@"C:\A\", @"B\C\", @"C:\A\B\C\")]
        [TestCase(@"C:\A\", @"C:B\C", @"C:\A\B\C")]
        [TestCase(@"C:\A\", @"C:B\C\", @"C:\A\B\C\")]
        [TestCase(@"C:\A\", @"C:\B\C", @"C:\B\C")]
        [TestCase(@"C:\A\", @"C:\B\C\", @"C:\B\C\")]
        [TestCase(@"C:\A\", @"X:", null)]
        [TestCase(@"C:\A\", @"X:foo", null)]
        [TestCase(@"C:\A\", @"X:\foo", @"X:\foo")]
        [TestCase(@"C:\A\", @"\\srv\sh", @"\\srv\sh")]
        [TestCase(@"C:\A\", @"\\srv\sh\foo", @"\\srv\sh\foo")]
        [TestCase(@"C:\A", @"..", @"C:\")]
        [TestCase(@"C:\A", @"..\", @"C:\")]
        [TestCase(@"C:\A", @"..\..", null)]
        [TestCase(@"C:\A\", @"..", @"C:\")]
        [TestCase(@"C:\A\", @"..\", @"C:\")]
        [TestCase(@"C:\A\", @"..\..", null)]
        [TestCase(@"\\srv\sh", "", @"\\srv\sh")]
        [TestCase(@"\\srv\sh", @"\", @"\\srv\sh\")]
        [TestCase(@"\\srv\sh", @"B\C", @"\\srv\sh\B\C")]
        [TestCase(@"\\srv\sh", @"B\C\", @"\\srv\sh\B\C\")]
        [TestCase(@"\\srv\sh", @"\\srv\sh\B\C", @"\\srv\sh\B\C")]
        [TestCase(@"\\srv\sh", @"\\srv\sh\B\C\", @"\\srv\sh\B\C\")]
        [TestCase(@"\\srv\sh", @"X:", null)]
        [TestCase(@"\\srv\sh", @"X:foo", null)]
        [TestCase(@"\\srv\sh", @"X:\foo", @"X:\foo")]
        [TestCase(@"\\srv\sh", @"\\srv\sh2", @"\\srv\sh2")]
        [TestCase(@"\\srv\sh", @"\\srv\sh2\foo", @"\\srv\sh2\foo")]
        [TestCase(@"\\srv\sh\", "", @"\\srv\sh\")]
        [TestCase(@"\\srv\sh\", @"\", @"\\srv\sh\")]
        [TestCase(@"\\srv\sh\", @"B\C", @"\\srv\sh\B\C")]
        [TestCase(@"\\srv\sh\", @"B\C\", @"\\srv\sh\B\C\")]
        [TestCase(@"\\srv\sh\", @"X:", null)]
        [TestCase(@"\\srv\sh\", @"X:foo", null)]
        [TestCase(@"\\srv\sh\", @"X:\foo", @"X:\foo")]
        [TestCase(".", "..", "..")]
        [TestCase("..", "..", @"..\..")]
        [TestCase(@"..\..", "..", @"..\..\..")]
        [TestCase("..", @"..\..", @"..\..\..")]
        [TestCase(@"..\..", @"..\..", @"..\..\..\..")]
        [TestCase(@"..\foo", "..", "..")]
        [TestCase(@"..\foo\bar", "..", @"..\foo")]
        [TestCase(@"..\foo\bar", @"..\..", "..")]
        [TestCase(@"..\foo\bar", @"..\..\..", @"..\..")]
        [TestCase("..", @"..\foo", @"..\..\foo")]
        [TestCase("..", @"..\foo\bar", @"..\..\foo\bar")]
        [TestCase(@"..\..", @"..\foo\bar", @"..\..\..\foo\bar")]
        [TestCase(@"\", "..", null)]
        public void Append(string path, string extend, string expected)
        {
            WindowsPath p = new WindowsPath(path);

            if (expected != null) {
                Assert.That(p.Append(extend).ToString(), Is.EqualTo(expected));
            } else {
                Assert.That(() => {
                    Path r = p.Append(extend);
                    Console.WriteLine($"Got: {r}");
                }, Throws.TypeOf<ArgumentException>());
            }
        }
    }
}
