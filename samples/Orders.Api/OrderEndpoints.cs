using Microsoft.AspNetCore.Http.HttpResults;
using Orders.Domain;
using Orders.Unions;

namespace Orders.Api;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        // Expected outcomes: part of the signature, mapped exhaustively to HTTP.
        app.MapPost("/orders", (PlaceOrderRequest request, OrderService service) =>
            ToHttpResult(service.PlaceOrder(request)));

        // Unexpected failure: a bug or a broken dependency. Nobody catches it here, on purpose.
        app.MapGet("/boom", string () => throw new InvalidOperationException("Database connection string is 'Server=secret'."));

        return app;
    }

    // Switch EXPRESSION, not statement: only expressions are checked for exhaustiveness (CS8509).
    public static IResult ToHttpResult(PlaceOrderResult result) => result switch
    {
        OrderConfirmation ok   => TypedResults.Ok(ok),
        InvalidQuantity e      => Problem(StatusCodes.Status400BadRequest, "Invalid quantity", $"Quantity must be positive, got {e.Quantity}."),
        UnknownCustomer e      => Problem(StatusCodes.Status404NotFound, "Unknown customer", $"Customer '{e.Id}' does not exist."),
        UnknownProduct e       => Problem(StatusCodes.Status404NotFound, "Unknown product", $"Product '{e.Sku}' does not exist."),
        CustomerBlocked e      => Problem(StatusCodes.Status403Forbidden, "Customer blocked", $"Customer '{e.Id}' is blocked."),
        OutOfStock e           => Problem(StatusCodes.Status409Conflict, "Out of stock", $"{e.Requested} x {e.Sku} requested, {e.Available} available."),
        CreditLimitExceeded e  => Problem(StatusCodes.Status402PaymentRequired, "Credit limit exceeded", $"Order total {e.Total} exceeds the limit of {e.Limit}."),
    };

    private static ProblemHttpResult Problem(int status, string title, string detail) =>
        TypedResults.Problem(statusCode: status, title: title, detail: detail);
}
