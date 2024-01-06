namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathParentBenchmark
    {
        private static readonly UnixPath ParentRootDir = new UnixPath("/");
        private static readonly UnixPath RootDir = new UnixPath("/path");
        private static readonly UnixPath Relative = new UnixPath("./foo/bar");

        [Benchmark]
        public Path Parent() => RootDir.GetParent();

        [Benchmark]
        public Path ParentRoot() => ParentRootDir.GetParent();

        [Benchmark]
        public Path RelativeParent() => Relative.GetParent();
    }
}
