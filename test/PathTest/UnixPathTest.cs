namespace RJCP.IO
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class UnixPathTest
    {
        [Flags]
        public enum UnixPathFeature
        {
            None = 0,
            IsPinned = 1,
        }

        [TestCase(null, UnixPathFeature.None)]
        [TestCase("", UnixPathFeature.None)]
        [TestCase("X", UnixPathFeature.None)]
        [TestCase("/", UnixPathFeature.IsPinned)]     // Pinned, because '\' is the beginning of the filesystem
        [TestCase("foo", UnixPathFeature.None)]
        [TestCase("/foo", UnixPathFeature.IsPinned)]
        [TestCase("foo/", UnixPathFeature.None)]
        [TestCase("/foo/", UnixPathFeature.IsPinned)]
        [TestCase("/foo/bar", UnixPathFeature.IsPinned)]
        [TestCase("/foo/bar/", UnixPathFeature.IsPinned)]
        [TestCase("鳥/目/山水.txt", UnixPathFeature.None)]
        public void ParsePath(string path, UnixPathFeature features)
        {
            UnixPath p = new UnixPath(path);
            Console.WriteLine($"{p}");

            string expectedPath = path ?? string.Empty;
            Assert.Multiple(() => {
                Assert.That(p.ToString(), Is.EqualTo(expectedPath));
                Assert.That(p.RootVolume, Is.EqualTo(string.Empty));
                Assert.That(p.IsPinned, Is.EqualTo(features.HasFlag(UnixPathFeature.IsPinned)));
            });
        }

        [TestCase("/foo/..", "/")]
        [TestCase("/foo/bar/..", "/foo")]
        [TestCase("/..", null)]
        [TestCase("/foo/../..", null)]
        [TestCase(".", "")]
        [TestCase("./", "")]
        [TestCase("/.", "/")]
        [TestCase("/./", "/")]
        [TestCase("..", "..")]
        [TestCase("../../.", "../..")]
        [TestCase("./../..", "../..")]
        [TestCase("../foo", "../foo")]
        [TestCase("../foo/..", "..")]
        [TestCase("../foo/bar/..", "../foo")]
        [TestCase("../foo/bar/../../..", "../..")]
        [TestCase("./bar", "bar")]
        [TestCase("/./bar", "/bar")]
        [TestCase("foo//bar", "foo/bar")]
        [TestCase("//foo", "/foo")]
        public void NormalizedPath(string path, string expectedPath)
        {
            if (expectedPath == null) {
                Assert.That(() => {
                    _ = new UnixPath(path);
                }, Throws.TypeOf<ArgumentException>());
            } else {
                UnixPath p = new UnixPath(path);
                Assert.That(p.ToString(), Is.EqualTo(expectedPath));
            }
        }

        [TestCase("/", "/")]
        [TestCase("/foo", "/foo")]
        [TestCase("/foo/", "/foo")]
        public void TrimPath(string path, string expectedPath)
        {
            UnixPath p = new UnixPath(path);
            Assert.That(p.Trim().ToString(), Is.EqualTo(expectedPath));
        }

        [TestCase("foo", "")]
        [TestCase("foo/", "")]
        [TestCase("foo/bar", "foo")]
        [TestCase("foo/bar/", "foo")]
        [TestCase("/", "/")]
        [TestCase(@"/foo", @"/")]
        [TestCase(@"/foo/", @"/")]
        [TestCase(@"/foo/bar", @"/foo")]
        [TestCase(@"/foo/bar/", @"/foo")]
        [TestCase("", "..")]
        [TestCase("../foo", "..")]
        [TestCase("../..", "../../..")]
        public void GetParent(string path, string expectedNewPath)
        {
            UnixPath p = new UnixPath(path);
            Assert.That(p.GetParent().ToString(), Is.EqualTo(expectedNewPath));
        }

        [TestCase("A/B/C", "A/B/C/D/E", @"../..")]              // Relative paths
        [TestCase("A/B/C", "A/B/C/D", "..")]
        [TestCase("A/B/C", "A/B/C", "")]
        [TestCase("A/B/C", "A/B", "C")]
        [TestCase("A/B/C", "A", "B/C")]
        [TestCase("A/B/C", "", "A/B/C")]
        [TestCase("A/B/C", "A/B/X", "../C")]
        [TestCase("A/B/C", "A/B/X/Y", "../../C")]
        [TestCase("A/B/C/D", "A/B/X/Y", "../../C/D")]
        [TestCase("/A/B/C", "/A/B/C/D/E", "../..")]              // Pinned paths
        [TestCase("/A/B/C", "/A/B/C/D", "..")]
        [TestCase("/A/B/C", "/A/B/C", "")]
        [TestCase("/A/B/C", "/A/B", "C")]
        [TestCase("/A/B/C", "/A", "B/C")]
        [TestCase("/A/B/C", "/", "A/B/C")]
        [TestCase("/A/B/C", "/A/B/X", "../C")]
        [TestCase("/A/B/C", "/A/B/X/Y", "../../C")]
        [TestCase("/A/B/C/D", "/A/B/X/Y", "../../C/D")]
        [TestCase("A/B/C", "A/b/c", "../../B/C")]
        public void GetRelative(string path, string basePath, string expected)
        {
            UnixPath p = new UnixPath(path);
            UnixPath b = new UnixPath(basePath);
            Path r = p.GetRelative(b);
            Assert.That(r.ToString(), Is.EqualTo(expected));

            // Appending the relative path to the base path should result in the original path. There are exceptions:
            // - When the path is unpinned, the basePath is pinned, the path is returned. Appending the base path with
            //   the unpinned path cannot result in the original unpinned path.
            Path o = b.Append(r);
            Assert.That(o.ToString(), Is.EqualTo(p.Trim().ToString()));
        }

        [TestCase("", "", "")]
        [TestCase("", "/", "/")]
        [TestCase("", "A", "A")]
        [TestCase("", "A/", "A/")]
        [TestCase("bar", "", "bar")]
        [TestCase("bar", "/", "/")]
        [TestCase("bar", "A", "bar/A")]
        [TestCase("bar", "A/", "bar/A/")]
        [TestCase("bar/", "", "bar/")]
        [TestCase("bar/", "/", "/")]
        [TestCase("bar/", "A", "bar/A")]
        [TestCase("bar/", "A/", "bar/A/")]
        [TestCase("/", "", "/")]
        [TestCase("/", "/", "/")]
        [TestCase("/", "A", "/A")]
        [TestCase("/", "A/", "/A/")]
        [TestCase("/bar", "", "/bar")]
        [TestCase("/bar", "/", "/")]
        [TestCase("/bar", "A", "/bar/A")]
        [TestCase("/bar", "A/", "/bar/A/")]
        [TestCase("/bar/", "", "/bar/")]
        [TestCase("/bar/", "/", "/")]
        [TestCase("/bar/", "A", "/bar/A")]
        [TestCase("/bar/", "A/", "/bar/A/")]
        [TestCase(".", "..", "..")]
        [TestCase("..", "..", "../..")]
        [TestCase("../..", "..", "../../..")]
        [TestCase("..", "../..", "../../..")]
        [TestCase("../..", "../..", "../../../..")]
        [TestCase("../foo", "..", "..")]
        [TestCase("../foo/bar", "..", "../foo")]
        [TestCase("../foo/bar", "../..", "..")]
        [TestCase("../foo/bar", "../../..", "../..")]
        [TestCase("..", "../foo", "../../foo")]
        [TestCase("..", "../foo/bar", "../../foo/bar")]
        [TestCase("../..", "../foo/bar", "../../../foo/bar")]
        [TestCase("/", "..", null)]
        public void Append(string path, string extend, string expected)
        {
            UnixPath p = new UnixPath(path);

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
