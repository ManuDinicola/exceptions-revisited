```

BenchmarkDotNet v0.16.0-preview.2, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
Intel Xeon Processor 2.10GHz, 1 CPU, 4 logical and 4 physical cores
Memory: 15.71 GB Total, 12.47 GB Available
.NET SDK 11.0.100
  [Host]   : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  ShortRun : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method             | Categories | Mean        | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Exceptions_Failure | Failure    | 2,031.02 ns | 559.955 ns | 30.693 ns | 1.000 |    0.00 | 0.0019 |     360 B |        1.00 |
| Result_Failure     | Failure    |    42.42 ns |  80.402 ns |  4.407 ns | 0.021 |    0.00 | 0.0007 |      96 B |        0.27 |
| Union_Failure      | Failure    |    18.91 ns |   3.246 ns |  0.178 ns | 0.009 |    0.00 | 0.0001 |      24 B |        0.07 |
| UnionAttr_Failure  | Failure    |    18.04 ns |   1.468 ns |  0.080 ns | 0.009 |    0.00 | 0.0001 |      24 B |        0.07 |
|                    |            |             |            |           |       |         |        |           |             |
| Exceptions_Success | Success    |   419.32 ns |  58.526 ns |  3.208 ns |  1.00 |    0.00 |      - |      72 B |        1.00 |
| Result_Success     | Success    |   465.71 ns | 865.925 ns | 47.464 ns |  1.11 |    0.10 |      - |      72 B |        1.00 |
| Union_Success      | Success    |   431.30 ns | 238.790 ns | 13.089 ns |  1.03 |    0.03 |      - |      72 B |        1.00 |
| UnionAttr_Success  | Success    |   406.99 ns | 137.369 ns |  7.530 ns |  0.97 |    0.02 | 0.0005 |      72 B |        1.00 |
