using Orders.Domain;

namespace Orders.Unions;

// C# 15: the return type lists EVERY possible outcome. Closed set, no base class, no guessing.
public union PlaceOrderResult(
    OrderConfirmation,
    InvalidQuantity,
    UnknownCustomer,
    UnknownProduct,
    CustomerBlocked,
    OutOfStock,
    CreditLimitExceeded);
