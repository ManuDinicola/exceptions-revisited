using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BenchmarkDotNet.Running;
using Orders.Domain;
using V1 = Orders.V1_Exceptions;
using V2 = Orders.V2_Result;
using V3 = Orders.V3_Unions;

// BenchmarkDotNet 0.15.x does not know net11.0 yet, so run in-process (no auto-generated project).
var config = DefaultConfig.Instance
    .AddJob(Job.ShortRun.WithToolchain(InProcessNoEmitToolchain.Instance))
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);
BenchmarkRunner.Run<OrderBenchmarks>(config, args);

[MemoryDiagnoser]
public class OrderBenchmarks
{
    private readonly Store _store = new();
    private V1.OrderService _v1 = null!;
    private V2.OrderService _v2 = null!;
    private V3.OrderService _v3 = null!;

    private static readonly PlaceOrderRequest Good = new("alice", "BOOK", 2);
    private static readonly PlaceOrderRequest Blocked = new("bob", "BOOK", 1);

    [GlobalSetup]
    public void Setup()
    {
        _v1 = new(_store); _v2 = new(_store); _v3 = new(_store);
    }

    // ---- success path: all three should be equivalent ----
    [Benchmark] public object Exceptions_Success() => _v1.PlaceOrder(Good);
    [Benchmark] public object Result_Success() => _v2.PlaceOrder(Good);
    [Benchmark] public object Union_Success() => _v3.PlaceOrder(Good);

    // ---- failure path: where exceptions hurt ----
    [Benchmark]
    public string Exceptions_Failure()
    {
        try { _v1.PlaceOrder(Blocked); return "ok"; }
        catch (V1.CustomerBlockedException e) { return e.Message; }
    }

    [Benchmark] public bool Result_Failure() => _v2.PlaceOrder(Blocked).IsSuccess;
    [Benchmark] public bool Union_Failure() => _v3.PlaceOrder(Blocked).Value is OrderConfirmation;
}

