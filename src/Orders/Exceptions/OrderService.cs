using Orders.Domain;

namespace Orders.Exceptions;

/// <summary>V1: failures are thrown.</summary>
public sealed class OrderService(Store store)
{
    // The signature says "I return a confirmation". It does NOT say "I can throw six different things".
    public OrderConfirmation PlaceOrder(PlaceOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Quantity <= 0) throw new InvalidQuantityException(request.Quantity);

        var customer = store.FindCustomer(request.CustomerId)
            ?? throw new UnknownCustomerException(request.CustomerId);
        if (customer.IsBlocked) throw new CustomerBlockedException(customer.Id);

        var product = store.FindProduct(request.Sku)
            ?? throw new UnknownProductException(request.Sku);
        if (product.Stock < request.Quantity)
            throw new OutOfStockException(product.Sku, request.Quantity, product.Stock);

        var total = product.UnitPrice * request.Quantity;
        if (total > customer.CreditLimit)
            throw new CreditLimitExceededException(total, customer.CreditLimit);

        return new OrderConfirmation(Guid.NewGuid(), customer.Id, product.Sku, request.Quantity, total);
    }
}
