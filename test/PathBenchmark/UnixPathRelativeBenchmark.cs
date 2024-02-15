namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathRelativeBenchmark
    {
        private static readonly UnixPath User = new("/users/home/repository/project");
        private static readonly UnixPath Home = new("/users/home");

        [Benchmark]
        public Path PinnedRelative() => User.GetRelative(Home);
    }
}
