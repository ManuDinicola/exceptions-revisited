using Orders.Domain;

namespace Orders.V3_Unions;

public sealed record InvalidQuantity(int Quantity);
public sealed record UnknownCustomer(string Id);
public sealed record UnknownProduct(string Sku);
public sealed record CustomerBlocked(string Id);
public sealed record OutOfStock(string Sku, int Requested, int Available);
public sealed record CreditLimitExceeded(decimal Total, decimal Limit);

// C# 15: the return type lists EVERY possible outcome. Closed set, no base class, no guessing.
public union PlaceOrderResult(
    OrderConfirmation,
    InvalidQuantity,
    UnknownCustomer,
    UnknownProduct,
    CustomerBlocked,
    OutOfStock,
    CreditLimitExceeded);

public sealed class OrderService(Store store)
{
    public PlaceOrderResult PlaceOrder(PlaceOrderRequest request)
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
