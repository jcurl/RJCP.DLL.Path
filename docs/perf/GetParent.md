# Performance Benchmark for constructing a path

There is the test case `WindowsPathInstantiateBenchmark` test `RelativePath`
that tests the construction of a `WindowsPath` object with multiple elements.

## Implementations

### Implementation 1: IndexOfAny

The main loop, searching for directory separators, and then building the path
stack:

```csharp
int ps = pathStart;
while (ps < trimmedPath.Length) {
    int s = trimmedPath.IndexOfAny(dirChars, ps);
    if (s == -1) {
        m_PathStack.Add(trimmedPath.Substring(ps));
        break;
    } else if (s > ps) {
        m_PathStack.Add(trimmedPath.Substring(ps, s - ps));
    }
    ps = s + 1;
}

if (ps == trimmedPath.Length) {
    m_PathStack.Add(string.Empty);
}
```

### Implementation 2: For-loop

Instead of using IndexOfAny from the framework, perform a small, tight loop that
does the same thing:

```csharp
int ps = pathStart;
for (int i = ps; i < trimmedPath.Length; i++) {
    char c = trimmedPath[i];
    if (IsDirSepChar(c)) {
        if (i > ps) m_PathStack.Add(trimmedPath.Substring(ps, i - ps));
        ps = i + 1;
    }
}
if (ps < trimmedPath.Length) {
    m_PathStack.Add(trimmedPath.Substring(ps));
} else {
    m_PathStack.Add(string.Empty);
}
```

## Results

Results are obtained using the BenchmarkDotNet, and documented here which one is
faster.

```text
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
Intel Core i7-6700T CPU 2.80GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4340.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4340.0), X64 RyuJIT

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
Intel Core i7-6700T CPU 2.80GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.402
  [Host]     : .NET Core 3.1.20 (CoreCLR 4.700.21.47003, CoreFX 4.700.21.47101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.20 (CoreCLR 4.700.21.47003, CoreFX 4.700.21.47101), X64 RyuJIT
```

| Framework     | Implementation |     Mean |    Error |  StdDev |
|---------------|----------------|---------:|---------:|--------:|
| .NET 4.8      | IndexOfAny     | 865.5 ns | 10.57 ns | 9.89 ns |
| .NET 4.8      | For-loop       | 780.6 ns |  7.89 ns | 7.38 ns |
| .NET Core 3.1 | IndexOfAny     | 611.2 ns |  7.90 ns | 7.39 ns |
| .NET Core 3.1 | For-loop       | 551.4 ns |  7.37 ns | 6.53 ns |

So we see that in both cases, using the for-loop is about 10% faster (and that
.NET Core makes a much bigger difference over .NET Framework).

.NET 4.8 = 84.9 / 865.5 = 9.8% (for loop is faster)
.NET Core = 59.8 / 611.2 = 9.8% (for loop is faster)
