﻿namespace RJCP.IO
{
    using BenchmarkDotNet.Attributes;

    public class UnixPathAppendBenchmark
    {
        private static readonly UnixPath RelativePath1 = new("users/home");
        private static readonly UnixPath RelativePath2 = new("documents/repos/project");

        private static readonly UnixPath PinnedPath = new(@"/users/home");

        [Benchmark]
        public Path RootPath() => PinnedPath.Append(RelativePath2);

        [Benchmark]
        public Path RelativePath() => RelativePath1.Append(RelativePath2);
    }
}
