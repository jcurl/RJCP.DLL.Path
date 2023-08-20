namespace RJCP.IO.Files.Exe
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class FileExecutableTest
    {
        [Test]
        public void NullPathString()
        {
            Assert.That(() => {
                _ = FileExecutable.GetFile((string)null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void NullPath()
        {
            Assert.That(() => {
                _ = FileExecutable.GetFile((Path)null);

            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void EmptyPathString([Values("", " ")] string pathStr)
        {
            Assert.That(() => {
                _ = FileExecutable.GetFile(pathStr);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void EmptyPath([Values("", " ")] string pathStr)
        {
            Path path = Path.ToPath(pathStr);
            Assert.That(() => {
                _ = FileExecutable.GetFile(path);
            }, Throws.TypeOf<ArgumentException>());
        }
    }
}
