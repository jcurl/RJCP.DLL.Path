namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathParentBenchmark
    {
        private static readonly Path DosRoot = new WindowsPath(@"C:\");
        private static readonly Path DosSubDir = new WindowsPath(@"C:\path");
        private static readonly Path UncRoot = new WindowsPath(@"\\srv\sh");
        private static readonly Path UncSubDir = new WindowsPath(@"\\srv\sh\path");
        private static readonly Path Relative = new WindowsPath(@".\foo\bar");

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