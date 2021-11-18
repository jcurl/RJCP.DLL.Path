// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.WindowsPathInstantiateBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.WindowsPathParentBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.WindowsPathAppendBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.WindowsPathRelativeBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.WindowsPathTrimBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.UnixPathInstantiateBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.UnixPathParentBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.UnixPathAppendBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.UnixPathRelativeBenchmark")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet needs non-static", Scope = "type", Target = "~T:RJCP.IO.UnixPathTrimBenchmark")]
