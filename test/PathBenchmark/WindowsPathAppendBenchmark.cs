namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathAppendBenchmark
    {
        private static readonly WindowsPath RelativePath1 = new(@"users\home");
        private static readonly WindowsPath RelativePath2 = new(@"documents\repos\project");

        private static readonly WindowsPath DosPath = new(@"C:\users\home");

        private static readonly WindowsPath DosPath2 = new(@"X:\remote\share");

        private static readonly WindowsPath UncPath = new(@"\\server\share\home");

        [Benchmark]
        public Path DosDrive() => DosPath.Append(RelativePath2);

        [Benchmark]
        public Path DosDriveAbs() => DosPath.Append(DosPath2);

        [Benchmark]
        public Path UncShare() => UncPath.Append(RelativePath2);

        [Benchmark]
        public Path RelativePath() => RelativePath1.Append(RelativePath2);
    }
}
