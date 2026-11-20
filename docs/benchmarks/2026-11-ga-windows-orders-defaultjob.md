```

BenchmarkDotNet v0.16.0-preview.2, Windows 11 (10.0.26300.9974)
AMD Ryzen 7 5700X 3.40GHz, 1 CPU, 16 logical and 8 physical cores
Memory: 15,92 GB Total, 2,03 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3


```
| Method             | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Exceptions_Failure | Failure    | 1,435.94 ns | 23.016 ns | 21.529 ns | 1.000 |    0.00 | 0.0210 |     368 B |        1.00 |
| Result_Failure     | Failure    |    21.25 ns |  0.400 ns |  0.354 ns | 0.015 |    0.00 | 0.0057 |      96 B |        0.26 |
| Union_Failure      | Failure    |    12.27 ns |  0.228 ns |  0.213 ns | 0.009 |    0.00 | 0.0014 |      24 B |        0.07 |
| UnionAttr_Failure  | Failure    |    12.22 ns |  0.240 ns |  0.247 ns | 0.009 |    0.00 | 0.0014 |      24 B |        0.07 |
|                    |            |             |           |           |       |         |        |           |             |
| Exceptions_Success | Success    |    86.85 ns |  1.300 ns |  1.086 ns |  1.00 |    0.00 | 0.0043 |      72 B |        1.00 |
| Result_Success     | Success    |    74.74 ns |  0.768 ns |  0.681 ns |  0.86 |    0.01 | 0.0043 |      72 B |        1.00 |
| Union_Success      | Success    |    73.69 ns |  1.288 ns |  1.205 ns |  0.85 |    0.02 | 0.0043 |      72 B |        1.00 |
| UnionAttr_Success  | Success    |    76.13 ns |  1.515 ns |  1.621 ns |  0.88 |    0.02 | 0.0043 |      72 B |        1.00 |
