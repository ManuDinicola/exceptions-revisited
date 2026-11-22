namespace Orders.Results;

// An OPEN hierarchy: anyone can derive from Error, so the compiler can never prove
// that a switch over it is exhaustive. The caller still has to guess which subtypes exist.
public abstract record Error(string Message);

public sealed record InvalidQuantity(int Quantity) : Error($"Invalid quantity: {Quantity}");

public sealed record UnknownCustomer(string Id) : Error($"Unknown customer: {Id}");

public sealed record UnknownProduct(string Sku) : Error($"Unknown product: {Sku}");

public sealed record CustomerBlocked(string Id) : Error($"Customer blocked: {Id}");

public sealed record OutOfStock(string Sku, int Requested, int Available)
    : Error($"Out of stock: {Sku} (requested {Requested}, available {Available})");

public sealed record CreditLimitExceeded(decimal Total, decimal Limit)
    : Error($"Credit limit exceeded: {Total} > {Limit}");
