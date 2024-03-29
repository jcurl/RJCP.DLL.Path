﻿namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathTrimBenchmark
    {
        private static readonly UnixPath UntrimmedPath = new("/users/home/repository/project/");
        private static readonly UnixPath TrimmedPath = new("/users/home/repository/project");

        [Benchmark]
        public Path Trimmed() => UntrimmedPath.Trim();

        [Benchmark]
        public Path NoTrim() => TrimmedPath.Trim();
    }
}
