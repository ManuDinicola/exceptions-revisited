using System.Runtime.CompilerServices;
using Orders.Domain;

namespace Orders.Unions.Handwritten;

// The hand-written equivalent of `public union PlaceOrderResult(...)`:
// [Union] + IUnion, one public constructor per case type, and a Value property.
// The compiler supplies the rest (implicit conversions, pattern matching, exhaustiveness).
// The case records come from the enclosing Orders.Unions namespace, shared with V3.
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
