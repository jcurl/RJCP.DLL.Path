namespace RJCP.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;
    using RJCP.Core.Environment;

    [TestFixture]
    public class FileSystemNodeInfoTest
    {
        [Test]
        public void NullPathString()
        {
            Assert.That(() => {
                _ = new FileSystemNodeInfo((string)null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void NullPath()
        {
            Assert.That(() => {
                _ = new FileSystemNodeInfo((Path)null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetFileFull()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                FileSystemNodeInfo info = new(System.IO.Path.Combine(scratch.Path, "test.txt"));
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetFilePathFull()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                Path path = Path.ToPath(scratch.Path).Append("test.txt");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetFileRelative()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                FileSystemNodeInfo info = new("test.txt");
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetFilePathRelative()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                Path path = Path.ToPath("test.txt");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test specific methods")]
        public void GetFileEquality()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                FileSystemNodeInfo info = new(System.IO.Path.Combine(scratch.Path, "test.txt"));
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                using (Stream fs = new FileStream("test2.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                FileSystemNodeInfo info2 = new(System.IO.Path.Combine(scratch.Path, "test2.txt"));
                Assert.That(System.IO.Path.GetFileName(info2.Path), Is.EqualTo("test2.txt"));

                Assert.That(info.Equals(info2), Is.False);
                Assert.That(info2.Equals(info), Is.False);
                Assert.That(info == info2, Is.False);
                Assert.That(info2 == info, Is.False);
                Assert.That(info != info2, Is.True);
                Assert.That(info2 != info, Is.True);

                FileSystemNodeInfo info3 = new(System.IO.Path.Combine(scratch.Path, "test.txt"));
                Assert.That(System.IO.Path.GetFileName(info3.Path), Is.EqualTo("test.txt"));

                Assert.That(info3.Equals(info2), Is.False);
                Assert.That(info2.Equals(info3), Is.False);
                Assert.That(info3 == info2, Is.False);
                Assert.That(info2 == info3, Is.False);
                Assert.That(info3 != info2, Is.True);
                Assert.That(info2 != info3, Is.True);

                Assert.That(info3.Equals(info), Is.True);
                Assert.That(info.Equals(info3), Is.True);
                Assert.That(info3 == info, Is.True);
                Assert.That(info == info3, Is.True);
                Assert.That(info3 != info, Is.False);
                Assert.That(info != info3, Is.False);

                // Note, it is not correct to test GetHashCode for inequality, even if it is highly likely they're
                // different.
                Assert.That(info.GetHashCode(), Is.EqualTo(info3.GetHashCode()));
            }
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test specific methods")]
        public void GetFilePathEquality()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                using (Stream fs = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                Path path = Path.ToPath(scratch.Path).Append("test.txt");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("test.txt"));

                using (Stream fs = new FileStream("test2.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    /* Do nothing, the file is empty */
                }
                Path path2 = Path.ToPath(scratch.Path).Append("test2.txt");
                FileSystemNodeInfo info2 = new(path2);
                Assert.That(System.IO.Path.GetFileName(info2.Path), Is.EqualTo("test2.txt"));

                Assert.That(info.Equals(info2), Is.False);
                Assert.That(info2.Equals(info), Is.False);
                Assert.That(info == info2, Is.False);
                Assert.That(info2 == info, Is.False);
                Assert.That(info != info2, Is.True);
                Assert.That(info2 != info, Is.True);

                Path path3 = Path.ToPath(scratch.Path).Append("test.txt");
                FileSystemNodeInfo info3 = new(path3);
                Assert.That(System.IO.Path.GetFileName(info3.Path), Is.EqualTo("test.txt"));

                Assert.That(info3.Equals(info2), Is.False);
                Assert.That(info2.Equals(info3), Is.False);
                Assert.That(info3 == info2, Is.False);
                Assert.That(info2 == info3, Is.False);
                Assert.That(info3 != info2, Is.True);
                Assert.That(info2 != info3, Is.True);

                Assert.That(info3.Equals(info), Is.True);
                Assert.That(info.Equals(info3), Is.True);
                Assert.That(info3 == info, Is.True);
                Assert.That(info == info3, Is.True);
                Assert.That(info3 != info, Is.False);
                Assert.That(info != info3, Is.False);

                // Note, it is not correct to test GetHashCode for inequality, even if it is highly likely they're
                // different.
                Assert.That(info.GetHashCode(), Is.EqualTo(info3.GetHashCode()));
            }
        }

        [Test]
        public void GetDirectoryFull()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                FileSystemNodeInfo info = new(System.IO.Path.Combine(scratch.Path, "testdir"));
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetDirectoryPathFull()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                Path path = Path.ToPath(scratch.Path).Append("testdir");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetDirectoryRelative()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                FileSystemNodeInfo info = new("testdir");
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        public void GetDirectoryPathRelative()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                Path path = Path.ToPath("testdir");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                if (Platform.IsWinNT()) {
                    // On Windows Vista and later.
                    Assert.That(info.Type, Is.EqualTo(NodeInfoType.WindowsExtended));
                }
            }
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test specific methods")]
        public void GetDirectoryEquality()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                FileSystemNodeInfo info = new(System.IO.Path.Combine(scratch.Path, "testdir"));
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                Directory.CreateDirectory("testdir2");
                FileSystemNodeInfo info2 = new(System.IO.Path.Combine(scratch.Path, "testdir2"));
                Assert.That(System.IO.Path.GetFileName(info2.Path), Is.EqualTo("testdir2"));

                Assert.That(info.Equals(info2), Is.False);
                Assert.That(info2.Equals(info), Is.False);
                Assert.That(info == info2, Is.False);
                Assert.That(info2 == info, Is.False);
                Assert.That(info != info2, Is.True);
                Assert.That(info2 != info, Is.True);

                FileSystemNodeInfo info3 = new(System.IO.Path.Combine(scratch.Path, "testdir"));
                Assert.That(System.IO.Path.GetFileName(info3.Path), Is.EqualTo("testdir"));

                Assert.That(info3.Equals(info2), Is.False);
                Assert.That(info2.Equals(info3), Is.False);
                Assert.That(info3 == info2, Is.False);
                Assert.That(info2 == info3, Is.False);
                Assert.That(info3 != info2, Is.True);
                Assert.That(info2 != info3, Is.True);

                Assert.That(info3.Equals(info), Is.True);
                Assert.That(info.Equals(info3), Is.True);
                Assert.That(info3 == info, Is.True);
                Assert.That(info == info3, Is.True);
                Assert.That(info3 != info, Is.False);
                Assert.That(info != info3, Is.False);

                // Note, it is not correct to test GetHashCode for inequality, even if it is highly likely they're
                // different.
                Assert.That(info.GetHashCode(), Is.EqualTo(info3.GetHashCode()));
            }
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test specific methods")]
        public void GetDirectorPathEquality()
        {
            using (ScratchPad scratch = Deploy.ScratchPad()) {
                Directory.CreateDirectory("testdir");
                Path path = Path.ToPath(scratch.Path).Append("testdir");
                FileSystemNodeInfo info = new(path);
                Assert.That(System.IO.Path.GetFileName(info.Path), Is.EqualTo("testdir"));

                Directory.CreateDirectory("testdir2");
                Path path2 = Path.ToPath(scratch.Path).Append("testdir2");
                FileSystemNodeInfo info2 = new(path2);
                Assert.That(System.IO.Path.GetFileName(info2.Path), Is.EqualTo("testdir2"));

                Assert.That(info.Equals(info2), Is.False);
                Assert.That(info2.Equals(info), Is.False);
                Assert.That(info == info2, Is.False);
                Assert.That(info2 == info, Is.False);
                Assert.That(info != info2, Is.True);
                Assert.That(info2 != info, Is.True);

                Path path3 = Path.ToPath(scratch.Path).Append("testdir");
                FileSystemNodeInfo info3 = new(path3);
                Assert.That(System.IO.Path.GetFileName(info3.Path), Is.EqualTo("testdir"));

                Assert.That(info3.Equals(info2), Is.False);
                Assert.That(info2.Equals(info3), Is.False);
                Assert.That(info3 == info2, Is.False);
                Assert.That(info2 == info3, Is.False);
                Assert.That(info3 != info2, Is.True);
                Assert.That(info2 != info3, Is.True);

                Assert.That(info3.Equals(info), Is.True);
                Assert.That(info.Equals(info3), Is.True);
                Assert.That(info3 == info, Is.True);
                Assert.That(info == info3, Is.True);
                Assert.That(info3 != info, Is.False);
                Assert.That(info != info3, Is.False);

                // Note, it is not correct to test GetHashCode for inequality, even if it is highly likely they're
                // different.
                Assert.That(info.GetHashCode(), Is.EqualTo(info3.GetHashCode()));
            }
        }
    }
}
