using Orders.Domain;

namespace Orders.V2_Result;

// A classic hand-rolled Result<T>: success OR an Error. The error is an open hierarchy,
// so the caller still has to guess which Error subtypes exist.
public abstract record Error(string Message);
public sealed record InvalidQuantity(int Quantity) : Error($"Invalid quantity: {Quantity}");
public sealed record UnknownCustomer(string Id) : Error($"Unknown customer: {Id}");
public sealed record UnknownProduct(string Sku) : Error($"Unknown product: {Sku}");
public sealed record CustomerBlocked(string Id) : Error($"Customer blocked: {Id}");
public sealed record OutOfStock(string Sku, int Requested, int Available)
    : Error($"Out of stock: {Sku} (requested {Requested}, available {Available})");
public sealed record CreditLimitExceeded(decimal Total, decimal Limit)
    : Error($"Credit limit exceeded: {Total} > {Limit}");

public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value) { _value = value; _error = null; }
    private Result(Error error) { _value = default; _error = error; }

    public bool IsSuccess => _error is null;
    public T Value => _error is null ? _value! : throw new InvalidOperationException("Result is a failure.");
    public Error Error => _error ?? throw new InvalidOperationException("Result is a success.");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);

    public TOut Match<TOut>(Func<T, TOut> ok, Func<Error, TOut> fail) =>
        _error is null ? ok(_value!) : fail(_error);
}

public sealed class OrderService(Store store)
{
    public Result<OrderConfirmation> PlaceOrder(PlaceOrderRequest request)
    {
        if (request.Quantity <= 0) return new InvalidQuantity(request.Quantity);

        var customer = store.FindCustomer(request.CustomerId);
        if (customer is null) return new UnknownCustomer(request.CustomerId);
        if (customer.IsBlocked) return new CustomerBlocked(customer.Id);

        var product = store.FindProduct(request.Sku);
        if (product is null) return new UnknownProduct(request.Sku);
        if (product.Stock < request.Quantity)
            return new OutOfStock(product.Sku, request.Quantity, product.Stock);

        var total = product.UnitPrice * request.Quantity;
        if (total > customer.CreditLimit) return new CreditLimitExceeded(total, customer.CreditLimit);

        return new OrderConfirmation(Guid.NewGuid(), customer.Id, product.Sku, request.Quantity, total);
    }
}
