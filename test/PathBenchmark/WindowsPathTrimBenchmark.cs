namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathTrimBenchmark
    {
        private static readonly WindowsPath UntrimmedPath = new(@"C:\users\home\repository\project\");
        private static readonly WindowsPath TrimmedPath = new(@"C:\users\home\repository\project");

        [Benchmark]
        public Path Trimmed() => UntrimmedPath.Trim();

        [Benchmark]
        public Path NoTrim() => TrimmedPath.Trim();
    }
}
