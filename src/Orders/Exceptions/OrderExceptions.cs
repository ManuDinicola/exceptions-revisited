namespace Orders.Exceptions;

// Each exception exposes its data as properties, so callers (and filters such as
// `catch (OutOfStockException e) when (e.Available > 0)`) never have to parse Message.
// Kept minimal for the article: real-world exceptions would also expose the (message, inner) constructor.
public abstract class OrderException(string message) : Exception(message);

public sealed class InvalidQuantityException(int quantity)
    : OrderException($"Invalid quantity: {quantity}")
{
    public int Quantity { get; } = quantity;
}

public sealed class UnknownCustomerException(string customerId)
    : OrderException($"Unknown customer: {customerId}")
{
    public string CustomerId { get; } = customerId;
}

public sealed class UnknownProductException(string sku)
    : OrderException($"Unknown product: {sku}")
{
    public string Sku { get; } = sku;
}

public sealed class CustomerBlockedException(string customerId)
    : OrderException($"Customer blocked: {customerId}")
{
    public string CustomerId { get; } = customerId;
}

public sealed class OutOfStockException(string sku, int requested, int available)
    : OrderException($"Out of stock: {sku} (requested {requested}, available {available})")
{
    public string Sku { get; } = sku;
    public int Requested { get; } = requested;
    public int Available { get; } = available;
}

public sealed class CreditLimitExceededException(decimal total, decimal limit)
    : OrderException($"Credit limit exceeded: {total} > {limit}")
{
    public decimal Total { get; } = total;
    public decimal Limit { get; } = limit;
}
