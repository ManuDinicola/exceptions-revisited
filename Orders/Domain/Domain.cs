namespace Orders.Domain;

public sealed record PlaceOrderRequest(string CustomerId, string Sku, int Quantity);

public sealed record OrderConfirmation(Guid OrderId, string CustomerId, string Sku, int Quantity, decimal Total);

public sealed record Customer(string Id, bool IsBlocked, decimal CreditLimit);

public sealed record Product(string Sku, decimal UnitPrice, int Stock);

/// <summary>Tiny in-memory store so the three implementations share the exact same rules.</summary>
public sealed class Store
{
    private readonly Dictionary<string, Customer> _customers = new()
    {
        ["alice"] = new("alice", false, 1_000m),
        ["bob"]   = new("bob",   true,  1_000m),   // blocked
        ["carol"] = new("carol", false, 50m),      // tiny credit limit
    };

    private readonly Dictionary<string, Product> _products = new()
    {
        ["BOOK"]  = new("BOOK",  20m,  100),
        ["LAPTOP"] = new("LAPTOP", 900m, 2),
    };

    public Customer? FindCustomer(string id) => _customers.GetValueOrDefault(id);
    public Product? FindProduct(string sku) => _products.GetValueOrDefault(sku);
}
