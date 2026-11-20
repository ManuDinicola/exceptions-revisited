using BenchmarkDotNet.Running;
using Exceptions.Runtimes;

// Same code, two runtimes:
//   dotnet run -c Release -f net11.0 --project benchmarks/Exceptions.Runtimes -- --runtimes net8.0 net11.0
// Requires the .NET 8 runtime to be installed next to the .NET 11 SDK.
BenchmarkRunner.Run<FailureCostBenchmarks>(args: args);
