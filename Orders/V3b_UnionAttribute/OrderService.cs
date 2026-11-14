using System.Runtime.CompilerServices;
using Orders.Domain;
using Orders.V3_Unions;   // reuse the same case records as V3

namespace Orders.V3b_UnionAttribute;

// The hand-written equivalent of: public union PlaceOrderResult(...);
// One public constructor per case type, plus a Value property. The compiler does the rest.
[Union]
public readonly struct PlaceOrderResult : IUnion
{
    public PlaceOrderResult(OrderConfirmation value) => Value = value;
    public PlaceOrderResult(InvalidQuantity value) => Value = value;
    public PlaceOrderResult(UnknownCustomer value) => Value = value;
    public PlaceOrderResult(UnknownProduct value) => Value = value;
    public PlaceOrderResult(CustomerBlocked value) => Value = value;
    public PlaceOrderResult(OutOfStock value) => Value = value;
    public PlaceOrderResult(CreditLimitExceeded value) => Value = value;

    public object? Value { get; }
}

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
