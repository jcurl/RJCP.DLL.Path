namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathInstantiateBenchmark
    {
        [Benchmark]
        public Path DosDrive() => new WindowsPath(@"C:\path");

        [Benchmark]
        public Path UncShare() => new WindowsPath(@"\\share\server\path");

        [Benchmark]
        public Path UncSharePartial() => new WindowsPath(@"\\share");

        [Benchmark]
        public Path RelativePath() => new WindowsPath(@"this\is\a\path");
    }
}
