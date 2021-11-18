namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathParentBenchmark
    {
        private static readonly Path ParentRootDir = new UnixPath("/");
        private static readonly Path RootDir = new UnixPath("/path");
        private static readonly Path Relative = new UnixPath("./foo/bar");

        [Benchmark]
        public Path Parent() => RootDir.GetParent();

        [Benchmark]
        public Path ParentRoot() => ParentRootDir.GetParent();

        [Benchmark]
        public Path RelativeParent() => Relative.GetParent();
    }
}
