namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathTrimBenchmark
    {
        private static readonly Path UntrimmedPath = new UnixPath("/users/home/repository/project/");
        private static readonly Path TrimmedPath = new UnixPath("/users/home/repository/project");

        [Benchmark]
        public Path Trimmed() => UntrimmedPath.Trim();

        [Benchmark]
        public Path NoTrim() => TrimmedPath.Trim();
    }
}
