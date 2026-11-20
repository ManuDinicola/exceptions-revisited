namespace Orders.Domain;

/// <summary>
/// Tiny in-memory store, so that every implementation of <c>PlaceOrder</c> applies exactly the same rules
/// to exactly the same data.
/// </summary>
public sealed class Store
{
    private readonly Dictionary<string, Customer> _customers = new()
    {
        ["alice"] = new("alice", IsBlocked: false, CreditLimit: 1_000m),
        ["bob"]   = new("bob",   IsBlocked: true,  CreditLimit: 1_000m),
        ["carol"] = new("carol", IsBlocked: false, CreditLimit: 50m),
    };

    private readonly Dictionary<string, Product> _products = new()
    {
        ["BOOK"]   = new("BOOK",   UnitPrice: 20m,  Stock: 100),
        ["LAPTOP"] = new("LAPTOP", UnitPrice: 900m, Stock: 2),
    };

    public Customer? FindCustomer(string id) => _customers.GetValueOrDefault(id);

    public Product? FindProduct(string sku) => _products.GetValueOrDefault(sku);
}
