namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class WindowsPathRelativeBenchmark
    {
        private static readonly Path DosUser = new WindowsPath(@"C:\users\home\repository\project");

        private static readonly Path DosHome = new WindowsPath(@"C:\users\home");

        [Benchmark]
        public Path DosRelative() => DosUser.GetRelative(DosHome);
    }
}