# Superseded: .NET 11 RC2, BenchmarkDotNet 0.15.8, in-process

> Kept for history. These numbers were measured **in-process** (BenchmarkDotNet 0.15.8 did not know
> `net11.0`), and the success-path methods returned `object`, so the `Allocated` column of the
> `Result`/union rows includes boxing of the returned struct (+24 B / +32 B). Do not quote them.

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
| Exceptions_Success |    85.43 ns |   1.048 ns |  0.057 ns | 0.0043 |      72 B |
| Result_Success     |    95.60 ns |  17.118 ns |  0.938 ns | 0.0062 |     104 B |
| Union_Success      |    86.98 ns |  21.330 ns |  1.169 ns | 0.0057 |      96 B |
| UnionAttr_Success  |    87.38 ns |   4.677 ns |  0.256 ns | 0.0057 |      96 B |
| Exceptions_Failure | 1,542.27 ns | 910.723 ns | 49.920 ns | 0.0229 |     384 B |
| Result_Failure     |    33.17 ns |   3.744 ns |  0.205 ns | 0.0057 |      96 B |
| Union_Failure      |    19.93 ns |  11.201 ns |  0.614 ns | 0.0014 |      24 B |
| UnionAttr_Failure  |    19.55 ns |   2.502 ns |  0.137 ns | 0.0014 |      24 B |
