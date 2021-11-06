# Benchmark Results

These are the results of running the tool `PathBenchmark` between .NET 4.8 and
.NET Core 3.1.

## Overview and Measurements

The tests are run with the following machine. Results are specific to the OS,
.NET versions, and possibly the phase of the moon. They should be used for
relative comparisons only.

```ini
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

### Instantiate

.NET 4.8

|          Method |     Mean |    Error |  StdDev |
|---------------- |---------:|---------:|--------:|
|        DosDrive | 314.2 ns |  4.53 ns | 4.24 ns |
|        UncShare | 427.8 ns |  5.56 ns | 5.20 ns |
| UncSharePartial | 131.0 ns |  2.03 ns | 1.90 ns |
|    RelativePath | 767.7 ns | 10.09 ns | 9.44 ns |

.NET Core 3.1

|          Method |     Mean |   Error |  StdDev |   Median |
|---------------- |---------:|--------:|--------:|---------:|
|        DosDrive | 194.7 ns | 2.67 ns | 2.50 ns | 196.7 ns |
|        UncShare | 368.6 ns | 5.39 ns | 5.04 ns | 370.5 ns |
| UncSharePartial | 123.1 ns | 1.67 ns | 1.56 ns | 123.9 ns |
|    RelativePath | 534.4 ns | 7.06 ns | 5.51 ns | 537.0 ns |

### Append

.NET 4.8

|       Method |       Mean |      Error |     StdDev |
|------------- |-----------:|-----------:|-----------:|
|     DosDrive | 918.075 ns | 13.2100 ns | 12.3566 ns |
|  DosDriveAbs |   6.021 ns |  0.1474 ns |  0.1307 ns |
|     UncShare | 767.402 ns | 13.5482 ns | 12.6730 ns |
| RelativePath | 926.019 ns |  9.2753 ns |  8.6761 ns |

.NET Core 3.1

|       Method |       Mean |      Error |     StdDev |
|------------- |-----------:|-----------:|-----------:|
|     DosDrive | 634.318 ns |  8.5699 ns |  8.0163 ns |
|  DosDriveAbs |   6.027 ns |  0.1171 ns |  0.1038 ns |
|     UncShare | 516.222 ns |  6.8153 ns |  6.3750 ns |
| RelativePath | 615.045 ns | 11.6510 ns | 10.8983 ns |

### GetParent

.NET 4.8

|         Method |       Mean |     Error |    StdDev |     Median |
|--------------- |-----------:|----------:|----------:|-----------:|
|      DosParent |  22.452 ns | 0.2803 ns | 0.2622 ns |  22.549 ns |
|      UncParent |  18.394 ns | 0.1674 ns | 0.1398 ns |  18.430 ns |
|  DosParentRoot |  15.948 ns | 0.2503 ns | 0.2342 ns |  16.073 ns |
|  UncParentRoot |   5.175 ns | 0.0576 ns | 0.0510 ns |   5.154 ns |
| RelativeParent | 119.892 ns | 1.9036 ns | 1.7806 ns | 121.305 ns |

.NET Core 3.1

|         Method |      Mean |     Error |    StdDev |
|--------------- |----------:|----------:|----------:|
|      DosParent | 21.416 ns | 0.2691 ns | 0.2517 ns |
|      UncParent | 16.868 ns | 0.1892 ns | 0.1580 ns |
|  DosParentRoot | 16.231 ns | 0.2946 ns | 0.2755 ns |
|  UncParentRoot |  4.704 ns | 0.1220 ns | 0.1141 ns |
| RelativeParent | 91.394 ns | 1.2487 ns | 1.1680 ns |

### GetRelative

.NET 4.8

|      Method |     Mean |   Error |  StdDev |
|------------ |---------:|--------:|--------:|
| DosRelative | 313.5 ns | 4.27 ns | 3.78 ns |

.NET Core 3.1

|      Method |     Mean |   Error |  StdDev |
|------------ |---------:|--------:|--------:|
| DosRelative | 236.8 ns | 2.03 ns | 1.80 ns |

## Summary

While the software is identical, the compilation and execution is generally 20-30% faster on .NET Core.
