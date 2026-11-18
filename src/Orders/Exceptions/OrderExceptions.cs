namespace Orders.Exceptions;

// Kept deliberately minimal for the article: real-world exceptions would also expose
// the (message, innerException) constructor.
public abstract class OrderException(string message) : Exception(message);

public sealed class InvalidQuantityException(int quantity)
    : OrderException($"Invalid quantity: {quantity}");

public sealed class UnknownCustomerException(string id)
    : OrderException($"Unknown customer: {id}");

public sealed class UnknownProductException(string sku)
    : OrderException($"Unknown product: {sku}");

public sealed class CustomerBlockedException(string id)
    : OrderException($"Customer blocked: {id}");

public sealed class OutOfStockException(string sku, int requested, int available)
    : OrderException($"Out of stock: {sku} (requested {requested}, available {available})");

public sealed class CreditLimitExceededException(decimal total, decimal limit)
    : OrderException($"Credit limit exceeded: {total} > {limit}");
