using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Orders.Domain;
using V1 = Orders.Exceptions;
using V2 = Orders.Results;
using V3 = Orders.Unions;
using V3b = Orders.Unions.Handwritten;

namespace Orders.Benchmarks;

/// <summary>
/// Calls <c>PlaceOrder</c> and asks one question of the outcome: "did it succeed?".
/// Every method consumes the result the same way and returns a <see cref="bool"/>, so no benchmark pays
/// for boxing or string formatting that another one skips. Exceptions are the baseline of each category.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class OrderBenchmarks
{
    private const string Success = "Success";
    private const string Failure = "Failure";

    private static readonly PlaceOrderRequest Good = new("alice", "BOOK", 2);
    private static readonly PlaceOrderRequest Blocked = new("bob", "BOOK", 1);

    private readonly Store _store = new();
    private V1.OrderService _v1 = null!;
    private V2.OrderService _v2 = null!;
    private V3.OrderService _v3 = null!;
    private V3b.OrderService _v3b = null!;

    [GlobalSetup]
    public void Setup()
    {
        _v1 = new(_store);
        _v2 = new(_store);
        _v3 = new(_store);
        _v3b = new(_store);
    }

    // ---- success path: expected to be equivalent ----
    [Benchmark(Baseline = true), BenchmarkCategory(Success)] public bool Exceptions_Success() => Exceptions(Good);
    [Benchmark, BenchmarkCategory(Success)] public bool Result_Success() => _v2.PlaceOrder(Good).IsSuccess;
    [Benchmark, BenchmarkCategory(Success)] public bool Union_Success() => _v3.PlaceOrder(Good).Value is OrderConfirmation;
    [Benchmark, BenchmarkCategory(Success)] public bool UnionAttr_Success() => _v3b.PlaceOrder(Good).Value is OrderConfirmation;

    // ---- failure path: where exceptions hurt ----
    [Benchmark(Baseline = true), BenchmarkCategory(Failure)] public bool Exceptions_Failure() => Exceptions(Blocked);
    [Benchmark, BenchmarkCategory(Failure)] public bool Result_Failure() => _v2.PlaceOrder(Blocked).IsSuccess;
    [Benchmark, BenchmarkCategory(Failure)] public bool Union_Failure() => _v3.PlaceOrder(Blocked).Value is OrderConfirmation;
    [Benchmark, BenchmarkCategory(Failure)] public bool UnionAttr_Failure() => _v3b.PlaceOrder(Blocked).Value is OrderConfirmation;

    private bool Exceptions(PlaceOrderRequest request)
    {
        try
        {
            _v1.PlaceOrder(request);
            return true;
        }
        catch (V1.OrderException)
        {
            return false;
        }
    }
}
