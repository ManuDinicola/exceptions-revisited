using Orders.Domain;

namespace Orders.Results;

/// <summary>V2: failures are returned as an open <see cref="Error"/> hierarchy.</summary>
public sealed class OrderService(Store store)
{
    public Result<OrderConfirmation> PlaceOrder(PlaceOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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
