using Orders.Domain;
using V1 = Orders.V1_Exceptions;
using V2 = Orders.V2_Result;
using V3 = Orders.V3_Unions;

namespace Orders.Tests;

public class OrderServiceTests
{
    private static readonly Store Store = new();

    [Fact]
    public void V1_HappyPath_And_Failures()
    {
        var s = new V1.OrderService(Store);
        Assert.Equal(40m, s.PlaceOrder(new("alice", "BOOK", 2)).Total);
        Assert.Throws<V1.InvalidQuantityException>(() => s.PlaceOrder(new("alice", "BOOK", 0)));
        Assert.Throws<V1.CustomerBlockedException>(() => s.PlaceOrder(new("bob", "BOOK", 1)));
        Assert.Throws<V1.OutOfStockException>(() => s.PlaceOrder(new("alice", "LAPTOP", 5)));
        Assert.Throws<V1.CreditLimitExceededException>(() => s.PlaceOrder(new("carol", "LAPTOP", 1)));
    }

    [Fact]
    public void V2_HappyPath_And_Failures()
    {
        var s = new V2.OrderService(Store);
        Assert.Equal(40m, s.PlaceOrder(new("alice", "BOOK", 2)).Value.Total);
        Assert.IsType<V2.InvalidQuantity>(s.PlaceOrder(new("alice", "BOOK", 0)).Error);
        Assert.IsType<V2.CustomerBlocked>(s.PlaceOrder(new("bob", "BOOK", 1)).Error);
        Assert.IsType<V2.OutOfStock>(s.PlaceOrder(new("alice", "LAPTOP", 5)).Error);
        Assert.IsType<V2.CreditLimitExceeded>(s.PlaceOrder(new("carol", "LAPTOP", 1)).Error);
    }

    [Fact]
    public void V3_HappyPath_And_Failures()
    {
        var s = new V3.OrderService(Store);
        Assert.Equal(40m, Assert.IsType<OrderConfirmation>(s.PlaceOrder(new("alice", "BOOK", 2)).Value).Total);
        Assert.IsType<V3.InvalidQuantity>(s.PlaceOrder(new("alice", "BOOK", 0)).Value);
        Assert.IsType<V3.CustomerBlocked>(s.PlaceOrder(new("bob", "BOOK", 1)).Value);
        Assert.IsType<V3.OutOfStock>(s.PlaceOrder(new("alice", "LAPTOP", 5)).Value);
        Assert.IsType<V3.CreditLimitExceeded>(s.PlaceOrder(new("carol", "LAPTOP", 1)).Value);
    }
}
