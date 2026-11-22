namespace Orders.Unions;

// Plain records: no common base class, no Message property. Each case carries only its own data.
// Shared by the keyword union (V3) and the hand-written [Union] struct (V3b).
public sealed record InvalidQuantity(int Quantity);

public sealed record UnknownCustomer(string Id);

public sealed record UnknownProduct(string Sku);

public sealed record CustomerBlocked(string Id);

public sealed record OutOfStock(string Sku, int Requested, int Available);

public sealed record CreditLimitExceeded(decimal Total, decimal Limit);
