namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathRelativeBenchmark
    {
        private static readonly UnixPath User = new UnixPath("/users/home/repository/project");
        private static readonly UnixPath Home = new UnixPath("/users/home");

        [Benchmark]
        public Path PinnedRelative() => User.GetRelative(Home);
    }
}
