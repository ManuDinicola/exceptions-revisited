namespace Orders.Domain;

public sealed record PlaceOrderRequest(string CustomerId, string Sku, int Quantity);

public sealed record OrderConfirmation(Guid OrderId, string CustomerId, string Sku, int Quantity, decimal Total);

public sealed record Customer(string Id, bool IsBlocked, decimal CreditLimit);

public sealed record Product(string Sku, decimal UnitPrice, int Stock);
