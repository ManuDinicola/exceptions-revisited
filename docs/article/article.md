# Exception Handling in C#, Revisited for .NET 11

*Your methods are lying about what they return.*

In December 2024 I wrote [Exception Handling in C#](https://medium.com/@manu.dinicola/exception-handling-in-c-5f612fc52195)
for .NET 8. Two years and three runtimes later, three things have changed enough to rewrite it:

- **.NET 9 rewrote how the runtime throws and catches.** Exceptions are much cheaper than they were,
  but still far from free.
- **ASP.NET Core 10 fixed the double logging** that made me recommend a logging-filter workaround.
- **C# 15 shipped union types.** For the first time, a method can declare every way it can fail,
  and the compiler checks that callers handle all of them.

Some advice from 2024 still stands, some was incomplete, and some was wrong. This version keeps the
same structure, so you can compare the two section by section. All the code is in
[the companion repo](https://github.com/ManuDinicola/exceptions-revisited), with tests and benchmarks.

---

## 1. Two kinds of failure

In 2024 I split exceptions into "managed" and "unhandled". That vocabulary was misleading: in .NET,
*every* C# exception is a managed exception. The split that matters is a different one:

- **Expected outcomes**: the customer is blocked, the product is out of stock, the input is not a
  number. These are business answers. They happen every day, and the caller **must** decide what
  to do with each of them.
- **Unexpected failures**: the database is down, a configuration value is missing, a bug produced a
  `null`. Nobody near the call site can fix them. They should travel up to one place that logs them
  and answers "something went wrong".

The whole article follows from that split:

> **Exceptions are for the unexpected. Expected outcomes belong in the return type.**

## 2. The running example

The 2024 version divided two numbers. That is too small to show the real problem, so this time we
use a use case with six business rules. `PlaceOrder` can:

- succeed, returning an `OrderConfirmation`;
- reject an invalid quantity;
- not find the customer;
- not find the product;
- refuse a blocked customer;
- run out of stock;
- exceed the customer's credit limit.

The repo implements it four times with **identical rules**: exceptions (V1), `Result<T>` (V2), a
C# 15 union (V3), and the same union written by hand (V3b).

## 3. `try` / `catch` / `finally`, done right

The 2024 template was `catch (Exception ex)`. It is the most common, and the most dangerous, line
of error-handling code, because it catches everything: the case you meant, and the bug you didn't.

```csharp
try
{
    order = service.PlaceOrder(request);
}
catch (OutOfStockException e) when (e.Available > 0)   // a filter: only catch what you can handle
{
    order = service.PlaceOrder(request with { Quantity = e.Available });
}
catch (HttpRequestException e)
{
    // Add context, keep the original as InnerException.
    throw new InventoryUnavailableException("Inventory service unreachable.", e);
}
finally
{
    // Runs whatever happens. For IDisposable resources, prefer `using`, which writes this for you.
}
```

Four rules cover almost every case:

1. **Catch specific types**, and only where you can actually do something. Catching `Exception` belongs
   at the edge of the application (section 5), not in business code.
2. **Use exception filters (`when`)** instead of catching and rethrowing. A filter that returns `false`
   does not unwind the stack, so the original throw site stays intact in a crash dump.
3. **Rethrow with `throw;`, never `throw ex;`.** `throw ex;` resets the stack trace to the current
   line, and you lose where the problem actually happened.
4. **Wrap with context, and keep the cause**: `new XxxException("what we were doing", inner)`.

## 4. How to avoid exceptions

### 4.1 Validate at the boundary

Still true. What changed is that .NET now ships the guard clauses, so there is no reason to
hand-write them:

```csharp
ArgumentNullException.ThrowIfNull(request);
ArgumentException.ThrowIfNullOrWhiteSpace(request.CustomerId);
ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.Quantity);
```

These throw on purpose: passing `null` is a **programming error** (unexpected), not a business outcome.
A quantity of zero typed by a user is different. That is an *expected* outcome, and section 6 returns
it instead of throwing it.

### 4.2 Use the `Try` pattern

`int.TryParse` is still the right tool, and the pattern generalises:
`Dictionary.TryGetValue`, `ConcurrentQueue.TryDequeue`, `Uri.TryCreate`, and so on.
Its limit is that it answers only *yes/no*. When a caller needs to know **why** something failed,
a `bool` is not enough. That is where section 6 starts.

## 5. Centralize the unexpected

In 2024 I showed three options: the `UseExceptionHandler` lambda, a custom middleware, and
`IExceptionHandler`. In .NET 11 there is one answer: **`IExceptionHandler` plus ProblemDetails**.
The two other snippets also had problems I should have caught:

- the lambda returned **400 Bad Request** for any crash. A crash is a server error, **500**.
- the custom middleware sent `exception.Message` and **`exception.StackTrace` to the client**.
- the `IExceptionHandler` version copied **all request headers** into the response outside
  production. Headers carry `Authorization` and cookies, so tokens ended up in response bodies
  on shared QA environments.

Here is the version I would ship today:

```csharp
internal sealed partial class UnexpectedExceptionHandler(
    IProblemDetailsService problemDetails,
    IHostEnvironment environment,
    ILogger<UnexpectedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandled(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
        };

        if (environment.IsDevelopment())
        {
            problem.Detail = exception.ToString();   // never outside Development
        }

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, string method, string path, Exception exception);
}
```

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
```

**The duplicate-log workaround is gone.** In 2024 I showed how to silence
`ExceptionHandlerMiddleware` in `appsettings.json` or Serilog, because every exception was
logged twice. Since ASP.NET Core 10, the middleware no longer records diagnostics for exceptions
that an `IExceptionHandler` reports as handled. You can change this with
`ExceptionHandlerOptions.SuppressDiagnosticsCallback`. The repo's integration test checks it:
one `/boom` request produces **one** error log entry, ours.

Two smaller improvements since .NET 8 also help:

- `ExceptionHandlerOptions.StatusCodeSelector` (.NET 9) maps exception types to status codes without
  writing a handler.
- `WebApplicationFactory<Program>` works without adding `public partial class Program` by hand (.NET 10).

This handler is a **safety net**, not a control-flow mechanism. If your handler contains
`if (exception is CustomerBlockedException) return 403`, it is doing the job of a return type.

## 6. Make failures part of the signature

Here is the exception version of `PlaceOrder`:

```csharp
// The signature says "I return a confirmation".
// It does NOT say "I can throw six different things".
public OrderConfirmation PlaceOrder(PlaceOrderRequest request)
{
    if (request.Quantity <= 0) throw new InvalidQuantityException(request.Quantity);

    var customer = store.FindCustomer(request.CustomerId)
        ?? throw new UnknownCustomerException(request.CustomerId);
    if (customer.IsBlocked) throw new CustomerBlockedException(customer.Id);
    // ... out of stock, credit limit ...
    return new OrderConfirmation(Guid.NewGuid(), customer.Id, product.Sku, request.Quantity, total);
}
```

And a caller, written in good faith:

```csharp
try { var ok = service.PlaceOrder(r); return $"OK {ok.Total}"; }
catch (CustomerBlockedException e) { return $"403 {e.Message}"; }
catch (OutOfStockException e)      { return $"409 {e.Message}"; }
catch (OrderException e)           { return $"400 {e.Message}"; }   // catch-all base class
```

It compiles and looks complete. But when a customer with a credit limit of 50 orders a 900 laptop,
the API answers **400 Bad Request**: the request was valid, and the client is told it was not.
`CreditLimitExceededException` was added after this caller was written, and the catch-all base class
absorbed it silently. Nothing warned anyone.

### 6.1 `Result<T>`, the 2024 answer, improved

My 2024 `Result<T>` carried a `string? Error`. That made failure visible, but a string cannot be
matched on, so callers ended up comparing messages. The improved version uses typed errors:

```csharp
public abstract record Error(string Message);
public sealed record CustomerBlocked(string Id) : Error($"Customer blocked: {Id}");
public sealed record CreditLimitExceeded(decimal Total, decimal Limit)
    : Error($"Credit limit exceeded: {Total} > {Limit}");
// ...

public Result<OrderConfirmation> PlaceOrder(PlaceOrderRequest request)
{
    if (customer.IsBlocked) return new CustomerBlocked(customer.Id);
    // ...
}
```

The failure is in the signature now: callers see `Result<OrderConfirmation>` and know something
can go wrong. But look at the caller:

```csharp
err switch
{
    CustomerBlocked => $"403 {err.Message}",
    OutOfStock      => $"409 {err.Message}",
    _               => $"400 {err.Message}",   // required, because Error is an OPEN hierarchy
}
```

`Error` is an abstract record: anyone, anywhere, can derive from it. The compiler cannot prove the
switch is complete, so it forces a discard arm `_`. That arm hides `CreditLimitExceeded` exactly as
the catch-all did. **Same bug, same 400.**

One more trap, if you make `Result<T>` a struct to avoid allocations: deciding success with
`_error is null` makes `default(Result<T>)` a **success with a null value**. Track success with an
explicit `bool`, so that `default` is not a success.

### 6.2 C# 15 unions: a closed set of outcomes

C# 15 (the default language version for .NET 11) adds the `union` declaration:

```csharp
public sealed record InvalidQuantity(int Quantity);
public sealed record UnknownCustomer(string Id);
public sealed record UnknownProduct(string Sku);
public sealed record CustomerBlocked(string Id);
public sealed record OutOfStock(string Sku, int Requested, int Available);
public sealed record CreditLimitExceeded(decimal Total, decimal Limit);

public union PlaceOrderResult(
    OrderConfirmation,
    InvalidQuantity,
    UnknownCustomer,
    UnknownProduct,
    CustomerBlocked,
    OutOfStock,
    CreditLimitExceeded);
```

The cases are plain records: no base class, no shared `Message`. The method body barely changes:
each case converts implicitly to the union.

```csharp
public PlaceOrderResult PlaceOrder(PlaceOrderRequest request)
{
    if (request.Quantity <= 0) return new InvalidQuantity(request.Quantity);
    // ...
    return new OrderConfirmation(Guid.NewGuid(), customer.Id, product.Sku, request.Quantity, total);
}
```

The difference shows in the caller:

```csharp
public static IResult ToHttpResult(PlaceOrderResult result) => result switch
{
    OrderConfirmation ok  => TypedResults.Ok(ok),
    InvalidQuantity e     => Problem(400, "Invalid quantity", $"Quantity must be positive, got {e.Quantity}."),
    UnknownCustomer e     => Problem(404, "Unknown customer", $"Customer '{e.Id}' does not exist."),
    UnknownProduct e      => Problem(404, "Unknown product", $"Product '{e.Sku}' does not exist."),
    CustomerBlocked e     => Problem(403, "Customer blocked", $"Customer '{e.Id}' is blocked."),
    OutOfStock e          => Problem(409, "Out of stock", $"{e.Requested} x {e.Sku} requested, {e.Available} available."),
    CreditLimitExceeded e => Problem(402, "Credit limit exceeded", $"Order total {e.Total} exceeds the limit of {e.Limit}."),
};
```

No discard arm. The set is closed, so the compiler knows these seven arms are all there is. Delete
one, or add an eighth case to the union, and the build reports:

```text
warning CS8509: The switch expression does not handle all possible values of its input type
(it is not exhaustive). For example, the pattern 'Orders.Unions.CreditLimitExceeded' is not covered.
```

CS8509 is a **warning** by default. Promote it once for the whole solution, in `Directory.Build.props`:

```xml
<WarningsAsErrors>$(WarningsAsErrors);CS8509</WarningsAsErrors>
```

From then on, "I forgot a case" can no longer reach production. The carol-buys-a-laptop request now
answers **402 Payment Required**, because the code would not compile otherwise.

### 6.3 What the keyword generates

`union` is not magic. The compiler emits a struct marked with
`System.Runtime.CompilerServices.UnionAttribute` that implements `IUnion`, with one constructor per
case and an `object? Value` property. You can write the same thing by hand, and pattern matching
and exhaustiveness work identically:

```csharp
[Union]
public readonly struct PlaceOrderResult : IUnion
{
    public PlaceOrderResult(OrderConfirmation value) => Value = value;
    public PlaceOrderResult(InvalidQuantity value) => Value = value;
    // ... one constructor per case ...
    public object? Value { get; }
}
```

This is useful when you need to add members to the union type, and it is a good way to understand
the feature. For everything else, use the keyword.

## 7. Union pitfalls

Three things I learned the hard way:

1. **Only switch *expressions* are checked.** A `switch` *statement* over a union that handles two of
   seven cases compiles **with no diagnostic at all**. My October article on unions used
   switch statements in its exhaustiveness examples, and that claim was wrong. If you want the guarantee,
   write `result switch { ... }`.
2. **`default` is the eighth case.** `default(PlaceOrderResult)` has `Value == null` and matches no arm,
   so the switch throws `SwitchExpressionException` at run time. The compiler warns (CS8655) only
   when it can see the `default` locally, not across a method boundary. Don't put unions in
   uninitialised fields or arrays without thinking about it.
3. **Don't model the unexpected with unions.** `DatabaseUnavailable` as a union case forces every caller,
   at every level, to handle something none of them can fix. That is exactly what exceptions and
   section 5 are for.

## 8. Custom exceptions in 2026

Custom exceptions are still the right tool for the *unexpected* side. The 2024 advice holds:
give them descriptive names, and don't create one per minor issue. Three updates:

- **Seal them**, unless you design a hierarchy on purpose.
- **Drop the serialization constructor and `[Serializable]`.** `BinaryFormatter` is gone, and the
  `(SerializationInfo, StreamingContext)` constructor is obsolete (SYSLIB0051) since .NET 8.
  The three classic constructors are enough:

  ```csharp
  public sealed class InventoryUnavailableException : Exception
  {
      public InventoryUnavailableException() { }
      public InventoryUnavailableException(string message) : base(message) { }
      public InventoryUnavailableException(string message, Exception innerException)
          : base(message, innerException) { }
  }
  ```

- **If you are about to write a custom exception for a business rule, stop.** `CustomerBlockedException`
  is a union case waiting to happen.

## 9. Performance: what changed since .NET 8

The 2024 benchmark looped 1,000 times over a division that failed every other iteration. That mixed
the success and failure paths in one number. Its `Result<T>` was a class, so it allocated on both
paths. It enabled a Windows-only profiler, and it showed results as a screenshot. This time:

- success and failure are measured **separately**, with exceptions as the baseline (`Ratio` column);
- every benchmark consumes the result the same way (returns a `bool`), so no row pays for boxing or
  formatting that another skips;
- BenchmarkDotNet **0.16** runs each benchmark out-of-process on .NET 11. 0.15 did not know
  `net11.0` and needed an in-process workaround;
- raw reports are committed in the repo.

### 9.1 `PlaceOrder`, four ways (.NET 11)

```text
Method                  Mean        Ratio   Allocated
----------------------  ----------  ------  ---------
Success path
  Exceptions              447.7 ns    1.00       72 B
  Result<T>               412.4 ns    0.92       72 B
  union (keyword)         419.7 ns    0.94       72 B
  [Union] (hand-written)  416.4 ns    0.93       72 B
Failure path
  Exceptions            1,860.3 ns   1.000      368 B
  Result<T>                38.0 ns   0.021       96 B
  union (keyword)          17.9 ns   0.010       24 B
  [Union] (hand-written)   19.2 ns   0.010       24 B
```

- **Success path:** a tie. Every version allocates the same 72 B (the confirmation itself).
  Returning a union or a `Result` costs nothing extra when things go well.
- **Failure path:** throwing and catching costs about 49× more than returning a
  `Result`, and about 100× more than returning a union case.
- `Result` allocates 4× more than a union on failure (96 B vs 24 B). This is not `Result<T>`
  itself: the `Error` base record builds its `Message` string eagerly, even if nobody reads it.

### 9.2 .NET 8 vs .NET 11, by stack depth

Here the cost of reporting **one** failure from N frames down is measured two ways: throwing
(caught at the top) or returning a value that every frame passes back. The code is identical on
both runtimes.

```text
Depth   Throw .NET 8   Throw .NET 11   speed-up   Return .NET 8   Return .NET 11
-----   ------------   -------------   --------   -------------   --------------
    1       9.07 µs         1.75 µs       5.2x          2.9 ns           3.4 ns
   10      18.67 µs         5.45 µs       3.4x         16.3 ns          18.9 ns
   50      57.00 µs        22.56 µs       2.5x        378.2 ns         405.6 ns
```

What this says:

- **.NET 9's exception handling rewrite was real.** On identical code, throw/catch is **2.5 to 5×
  faster on .NET 11 than on .NET 8**. If you measured exceptions in 2024, your numbers are stale.
- **Still two orders of magnitude.** On .NET 11, one failure one frame down costs about 1.7 µs when
  thrown and about 3 ns when returned. The gap narrows with depth, but every extra frame still adds
  about 0.4 µs to a throw and about 8 ns to a return.
- **Returning didn't need a rewrite.** The `Return` column is within a few nanoseconds on both
  runtimes: there was nothing to optimise.

So the 2024 conclusion survives, with better numbers behind it. Exceptions are fine for failures
that are *rare*. They are the wrong tool for failures that are *expected*: on a login form, a
checkout, or a validation endpoint, failure is a normal path, not an exceptional one.

*All figures: BenchmarkDotNet default job, .NET 8.0.34 and 11.0.0, on a 4-core Linux cloud VM, so read
the ratios, not the nanoseconds. On Linux the success path is dominated by `Guid.NewGuid()`.
Raw reports are in `docs/benchmarks/`; run `dotnet run -c Release --project benchmarks/Orders.Benchmarks`
on your own machine.*

## 10. Decision guide

```text
Is this outcome part of the business contract (the caller must decide)?
├── Yes → return it.
│         ├── Closed, known set of outcomes   → C# 15 union + switch expression + CS8509 as error
│         ├── Just "found / not found"        → Try pattern (bool + out) or nullable
│         └── Open set, plugins, extensibility → Result<T> with typed errors (accept the discard arm)
└── No, it's a bug or the environment failing → throw.
          ├── Guard clauses: ArgumentNullException.ThrowIfNull & co.
          ├── Catch only where you can act; use `when`; rethrow with `throw;`
          └── One IExceptionHandler at the edge: log once, 500 + ProblemDetails, no leaks
```

## Conclusion

In 2024 my advice was "limit exceptions, they are slow". The performance argument is still true,
but it was never the main one. The main argument is **honesty**: a method that can fail in six
documented ways should say so in its signature, and since C# 15 the compiler can hold every caller
to it.

Exceptions haven't become bad. They have become *specialised*: reserve them for what nobody
expected, catch them in one place, and let the type system handle everything else.

---

*Companion repo: <https://github.com/ManuDinicola/exceptions-revisited>. .NET 11 (SDK 11.0.100),
C# 15, BenchmarkDotNet 0.16.0-preview.2. Clone it, delete a switch arm, and watch the build fail.*
