```

BenchmarkDotNet v0.16.0-preview.2, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
Intel Xeon Processor 2.10GHz, 1 CPU, 4 logical and 4 physical cores
Memory: 15.71 GB Total, 12.75 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4


```
| Method             | Categories | Mean        | Error     | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| Exceptions_Failure | Failure    | 1,860.32 ns | 51.379 ns | 148.240 ns | 1.000 |    0.00 |      - |     368 B |        1.00 |
| Result_Failure     | Failure    |    37.96 ns |  0.752 ns |   1.485 ns | 0.021 |    0.00 | 0.0007 |      96 B |        0.26 |
| Union_Failure      | Failure    |    17.88 ns |  0.351 ns |   0.821 ns | 0.010 |    0.00 | 0.0001 |      24 B |        0.07 |
| UnionAttr_Failure  | Failure    |    19.18 ns |  0.414 ns |   1.207 ns | 0.010 |    0.00 | 0.0001 |      24 B |        0.07 |
|                    |            |             |           |            |       |         |        |           |             |
| Exceptions_Success | Success    |   447.70 ns |  8.439 ns |  18.346 ns |  1.00 |    0.00 | 0.0005 |      72 B |        1.00 |
| Result_Success     | Success    |   412.38 ns |  8.003 ns |  12.222 ns |  0.92 |    0.05 | 0.0005 |      72 B |        1.00 |
| Union_Success      | Success    |   419.71 ns |  8.353 ns |  15.275 ns |  0.94 |    0.05 | 0.0005 |      72 B |        1.00 |
| UnionAttr_Success  | Success    |   416.36 ns |  8.063 ns |  11.819 ns |  0.93 |    0.05 | 0.0005 |      72 B |        1.00 |
