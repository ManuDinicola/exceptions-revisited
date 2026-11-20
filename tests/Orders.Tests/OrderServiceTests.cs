using Orders.Domain;
using V1 = Orders.Exceptions;
using V2 = Orders.Results;
using V3 = Orders.Unions;
using V3b = Orders.Unions.Handwritten;

namespace Orders.Tests;

/// <summary>Every implementation must apply the same business rules to the same inputs.</summary>
public sealed class OrderServiceTests
{
    private static readonly Store Store = new();

    private static readonly PlaceOrderRequest HappyPath = new("alice", "BOOK", 2);

    // One row per failure rule. The expected type is given for each version so the table
    // also documents how V1, V2 and V3 spell the same business outcome.
    public static TheoryData<PlaceOrderRequest, Type, Type, Type> Failures => new()
    {
        { new("alice", "BOOK", 0),   typeof(V1.InvalidQuantityException),     typeof(V2.InvalidQuantity),     typeof(V3.InvalidQuantity) },
        { new("zed",   "BOOK", 1),   typeof(V1.UnknownCustomerException),     typeof(V2.UnknownCustomer),     typeof(V3.UnknownCustomer) },
        { new("bob",   "BOOK", 1),   typeof(V1.CustomerBlockedException),     typeof(V2.CustomerBlocked),     typeof(V3.CustomerBlocked) },
        { new("alice", "PEN",  1),   typeof(V1.UnknownProductException),      typeof(V2.UnknownProduct),      typeof(V3.UnknownProduct) },
        { new("alice", "LAPTOP", 5), typeof(V1.OutOfStockException),          typeof(V2.OutOfStock),          typeof(V3.OutOfStock) },
        { new("carol", "LAPTOP", 1), typeof(V1.CreditLimitExceededException), typeof(V2.CreditLimitExceeded), typeof(V3.CreditLimitExceeded) },
    };

    [Fact]
    public void HappyPath_IsTheSameEverywhere()
    {
        Assert.Equal(40m, new V1.OrderService(Store).PlaceOrder(HappyPath).Total);
        Assert.Equal(40m, new V2.OrderService(Store).PlaceOrder(HappyPath).Value.Total);
        Assert.Equal(40m, Assert.IsType<OrderConfirmation>(new V3.OrderService(Store).PlaceOrder(HappyPath).Value).Total);
        Assert.Equal(40m, Assert.IsType<OrderConfirmation>(new V3b.OrderService(Store).PlaceOrder(HappyPath).Value).Total);
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public void V1_Exceptions_Throws(PlaceOrderRequest request, Type v1, Type v2, Type v3)
    {
        _ = (v2, v3);
        var ex = Assert.ThrowsAny<V1.OrderException>(() => new V1.OrderService(Store).PlaceOrder(request));
        Assert.IsType(v1, ex);
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public void V2_Result_ReturnsError(PlaceOrderRequest request, Type v1, Type v2, Type v3)
    {
        _ = (v1, v3);
        var result = new V2.OrderService(Store).PlaceOrder(request);
        Assert.False(result.IsSuccess);
        Assert.IsType(v2, result.Error);
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public void V3_Union_ReturnsCase(PlaceOrderRequest request, Type v1, Type v2, Type v3)
    {
        _ = (v1, v2);
        Assert.IsType(v3, new V3.OrderService(Store).PlaceOrder(request).Value);
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public void V3b_HandwrittenUnion_ReturnsCase(PlaceOrderRequest request, Type v1, Type v2, Type v3)
    {
        _ = (v1, v2);
        Assert.IsType(v3, new V3b.OrderService(Store).PlaceOrder(request).Value);
    }

    [Fact]
    public void DefaultResult_IsNotASuccess()
    {
        // Guards the classic struct-Result pitfall: default(Result<T>) must not read as "success, null value".
        var result = default(V2.Result<OrderConfirmation>);
        Assert.False(result.IsSuccess);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }
}
