namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathAppendBenchmark
    {
        private static readonly Path RelativePath1 = new UnixPath("users/home");
        private static readonly Path RelativePath2 = new UnixPath("documents/repos/project");

        private static readonly Path PinnedPath = new UnixPath(@"/users/home");

        [Benchmark]
        public Path RootPath() => PinnedPath.Append(RelativePath2);

        [Benchmark]
        public Path RelativePath() => RelativePath1.Append(RelativePath2);
    }
}
