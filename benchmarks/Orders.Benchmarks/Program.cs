using BenchmarkDotNet.Running;
using Orders.Benchmarks;

// BenchmarkDotNet 0.16 knows net11.0, so benchmarks run out-of-process in a freshly built,
// optimized project (the default toolchain). No in-process workaround needed any more.
//
//   dotnet run -c Release --project benchmarks/Orders.Benchmarks                  # full run
//   dotnet run -c Release --project benchmarks/Orders.Benchmarks -- --job short   # quick look
BenchmarkRunner.Run<OrderBenchmarks>(args: args);
