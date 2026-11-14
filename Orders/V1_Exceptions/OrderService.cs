using Orders.Domain;

namespace Orders.V1_Exceptions;

public abstract class OrderException(string message) : Exception(message);
public sealed class InvalidQuantityException(int quantity) : OrderException($"Invalid quantity: {quantity}");
public sealed class UnknownCustomerException(string id) : OrderException($"Unknown customer: {id}");
public sealed class UnknownProductException(string sku) : OrderException($"Unknown product: {sku}");
public sealed class CustomerBlockedException(string id) : OrderException($"Customer blocked: {id}");
public sealed class OutOfStockException(string sku, int requested, int available)
    : OrderException($"Out of stock: {sku} (requested {requested}, available {available})");
public sealed class CreditLimitExceededException(decimal total, decimal limit)
    : OrderException($"Credit limit exceeded: {total} > {limit}");

public sealed class OrderService(Store store)
{
    // The signature says "I return a confirmation". It does NOT say "I can throw six different things".
    public OrderConfirmation PlaceOrder(PlaceOrderRequest request)
    {
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
