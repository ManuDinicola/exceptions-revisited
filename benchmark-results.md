```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9974)
AMD Ryzen 7 5700X 3.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host] : .NET 11.0.0 (11.0.0-rc.2.26504.105, 11.0.26.50505), X64 RyuJIT x86-64-v3

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method             | Mean        | Error      | StdDev    | Gen0   | Allocated |
|------------------- |------------:|-----------:|----------:|-------:|----------:|
| Exceptions_Success |    87.92 ns |  64.117 ns |  3.514 ns | 0.0043 |      72 B |
| Result_Success     |    96.21 ns |  16.610 ns |  0.910 ns | 0.0062 |     104 B |
| Union_Success      |    93.00 ns |  19.770 ns |  1.084 ns | 0.0057 |      96 B |
| Exceptions_Failure | 1,530.12 ns | 293.398 ns | 16.082 ns | 0.0229 |     384 B |
| Result_Failure     |    30.10 ns |  21.503 ns |  1.179 ns | 0.0057 |      96 B |
| Union_Failure      |    18.45 ns |   5.695 ns |  0.312 ns | 0.0014 |      24 B |
