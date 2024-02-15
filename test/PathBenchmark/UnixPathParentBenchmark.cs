namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathParentBenchmark
    {
        private static readonly UnixPath ParentRootDir = new("/");
        private static readonly UnixPath RootDir = new("/path");
        private static readonly UnixPath Relative = new("./foo/bar");

        [Benchmark]
        public Path Parent() => RootDir.GetParent();

        [Benchmark]
        public Path ParentRoot() => ParentRootDir.GetParent();

        [Benchmark]
        public Path RelativeParent() => Relative.GetParent();
    }
}
