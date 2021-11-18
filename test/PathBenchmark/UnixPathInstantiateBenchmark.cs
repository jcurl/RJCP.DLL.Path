namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathInstantiateBenchmark
    {
        [Benchmark]
        public Path FixedDrive() => new WindowsPath("/path");

        [Benchmark]
        public Path RelativePath() => new WindowsPath("this/is/a/path");
    }
}
