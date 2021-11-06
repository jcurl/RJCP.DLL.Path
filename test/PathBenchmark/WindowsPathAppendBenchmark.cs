namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathAppendBenchmark
    {
        private static readonly Path RelativePath1 = new WindowsPath(@"users\home");
        private static readonly Path RelativePath2 = new WindowsPath(@"documents\repos\project");

        private static readonly Path DosPath = new WindowsPath(@"C:\users\home");

        private static readonly Path DosPath2 = new WindowsPath(@"X:\remote\share");

        private static readonly Path UncPath = new WindowsPath(@"\\server\share\home");

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