using Orders.Domain;
using V1 = Orders.V1_Exceptions;
using V2 = Orders.V2_Result;
using V3 = Orders.V3_Unions;

var store = new Store();
var requests = new PlaceOrderRequest[]
{
    new("alice", "BOOK", 2),
    new("alice", "BOOK", 0),
    new("bob",   "BOOK", 1),
    new("alice", "LAPTOP", 5),
    new("carol", "LAPTOP", 1),
    new("zed",   "BOOK", 1),
};

foreach (var r in requests)
{
    Console.WriteLine($"--- {r}");
    Console.WriteLine($"  V1 exceptions : {CallV1(r)}");
    Console.WriteLine($"  V2 Result<T>  : {CallV2(r)}");
    Console.WriteLine($"  V3 union      : {CallV3(r)}");
}

// V1: the caller must KNOW which exceptions exist. Forget one and it escapes as a 500.
string CallV1(PlaceOrderRequest r)
{
    try
    {
        var ok = new V1.OrderService(store).PlaceOrder(r);
        return $"OK {ok.Total}";
    }
    catch (V1.CustomerBlockedException e) { return $"403 {e.Message}"; }
    catch (V1.OutOfStockException e) { return $"409 {e.Message}"; }
    catch (V1.OrderException e) { return $"400 {e.Message}"; }   // catch-all base: new cases hide here silently
}

// V2: no exception, but Error is an open hierarchy, so the compiler cannot tell us we missed a case.
string CallV2(PlaceOrderRequest r) =>
    new V2.OrderService(store).PlaceOrder(r).Match(
        ok => $"OK {ok.Total}",
        err => err switch
        {
            V2.CustomerBlocked => $"403 {err.Message}",
            V2.OutOfStock => $"409 {err.Message}",
            _ => $"400 {err.Message}",                          // the discard arm hides new cases too
        });

// V3: closed set. No catch-all needed, and adding a case breaks the build here.
string CallV3(PlaceOrderRequest r) =>
    new V3.OrderService(store).PlaceOrder(r) switch
    {
        OrderConfirmation ok => $"OK {ok.Total}",
        V3.InvalidQuantity e => $"400 invalid quantity {e.Quantity}",
        V3.UnknownCustomer e => $"400 unknown customer {e.Id}",
        V3.UnknownProduct e => $"400 unknown product {e.Sku}",
        V3.CustomerBlocked e => $"403 customer blocked {e.Id}",
        V3.OutOfStock e => $"409 out of stock {e.Sku} ({e.Available} left)",
        V3.CreditLimitExceeded e => $"402 credit limit {e.Limit} exceeded by order of {e.Total}",
    };
