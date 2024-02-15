namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathParentBenchmark
    {
        private static readonly WindowsPath DosRoot = new(@"C:\");
        private static readonly WindowsPath DosSubDir = new(@"C:\path");
        private static readonly WindowsPath UncRoot = new(@"\\srv\sh");
        private static readonly WindowsPath UncSubDir = new(@"\\srv\sh\path");
        private static readonly WindowsPath Relative = new(@".\foo\bar");

        [Benchmark]
        public Path DosParent() => DosSubDir.GetParent();

        [Benchmark]
        public Path UncParent() => UncSubDir.GetParent();

        [Benchmark]
        public Path DosParentRoot() => DosRoot.GetParent();

        [Benchmark]
        public Path UncParentRoot() => UncRoot.GetParent();

        [Benchmark]
        public Path RelativeParent() => Relative.GetParent();
    }
}
