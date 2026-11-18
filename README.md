# Exception Handling in C#, Revisited

![Your C# Methods Are Lying About What They Return](docs/images/cover.png)

Companion repo for the article **"Your C# Methods Are Lying About What They Return"**, a revisit of
[Exception handling in C#](https://medium.com/@manu.dinicola/exception-handling-in-c-5f612fc52195).

One use case (`PlaceOrder`), the same business rules, implemented four ways on **.NET 11 / C# 15**:

| Version | Namespace (folder under `src/Orders`) | Approach | Failure is... |
|---|---|---|---|
| V1 | `Orders.Exceptions` (`Exceptions/`) | `throw` / `catch` | invisible in the signature |
| V2 | `Orders.Results` (`Results/`) | hand-rolled `Result<T>` + open `Error` hierarchy | visible, but the set of errors is open |
| V3 | `Orders.Unions` (`Unions/`) | C# 15 `union` keyword | visible **and** closed: the compiler checks exhaustiveness |
| V3b | `Orders.Unions.Handwritten` (`Unions/Handwritten/`) | hand-written `[Union]` struct | same as V3: it is what the keyword generates, written by hand |

## Repository layout

```text
.
├── src/Orders/                      the four implementations + shared domain
│   ├── Domain/                      requests, records, in-memory Store
│   ├── Exceptions/                  V1  OrderExceptions.cs, OrderService.cs
│   ├── Results/                     V2  Errors.cs, Result.cs, OrderService.cs
│   └── Unions/                      V3  Cases.cs, PlaceOrderResult.cs (union keyword), OrderService.cs
│       └── Handwritten/             V3b PlaceOrderResult.cs ([Union] struct), OrderService.cs
├── samples/Orders.Demo/             console app: same 6 requests through every version
├── tests/Orders.Tests/              xUnit v3 on Microsoft.Testing.Platform
├── benchmarks/Orders.Benchmarks/    BenchmarkDotNet 0.16
├── docs/
│   ├── images/                      figures used by the article
│   └── benchmarks/                  raw BenchmarkDotNet reports, one file per run
├── Directory.Build.props            net11.0, C# 15, nullable, warnings as errors (incl. CS8509)
├── Directory.Packages.props         central package versions
└── global.json                      SDK 11.0.100, MTP test runner
```

## Run it

Requires the [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0) (11.0.100 or later).
Unions ship in C# 15, the default language version for `net11.0`: no `LangVersion=preview` needed.

```bash
dotnet run   --project samples/Orders.Demo                         # same 6 requests through every version
dotnet test                                                        # 30 tests
dotnet run -c Release --project benchmarks/Orders.Benchmarks       # full run (a few minutes)
dotnet run -c Release --project benchmarks/Orders.Benchmarks -- --job short   # quick look
```

## What the demo shows

For a customer who tries to buy a 900 laptop with a credit limit of 50, V1 and V2 answer **400** (the
catch-all swallowed a case nobody handled), while V3 answers **402**, because the compiler would not let
us skip it.

![Demo output: V1 and V2 return 400, V3 returns 402](docs/images/demo-output.png)

## The compiler checks every case

Delete any arm of the V3 `switch` and the build fails with `CS8509`, naming the missing case:

```text
error CS8509: The switch expression does not handle all possible values of its input type
(it is not exhaustive). For example, the pattern 'Orders.Unions.CreditLimitExceeded' is not covered.
```

`CS8509` is normally a warning; `Directory.Build.props` makes it an error for the whole repo.

![Compiler warning CS8509 naming the missing CreditLimitExceeded case](docs/images/compiler-warning.png)

## Benchmarks

Every benchmark calls `PlaceOrder` and answers one question, *did it succeed?*, returning a `bool`, so
no row pays for boxing or string formatting that another one skips. Exceptions are the baseline of each
category, which gives the `Ratio` column.

BenchmarkDotNet 0.16.0-preview.2, .NET 11.0.0, out-of-process (default toolchain), `ShortRun`,
Linux cloud container (Intel Xeon 2.10 GHz, 4 cores):

| Method | Category | Mean | Ratio | Allocated |
|---|---|---:|---:|---:|
| Exceptions | Success | 419.3 ns | 1.00 | 72 B |
| Result | Success | 465.7 ns | 1.11 | 72 B |
| Union (keyword) | Success | 431.3 ns | 1.03 | 72 B |
| Union (`[Union]`) | Success | 407.0 ns | 0.97 | 72 B |
| **Exceptions** | **Failure** | **2,031.0 ns** | **1.000** | **360 B** |
| Result | Failure | 42.4 ns | 0.021 | 96 B |
| Union (keyword) | Failure | 18.9 ns | 0.009 | 24 B |
| Union (`[Union]`) | Failure | 18.0 ns | 0.009 | 24 B |

How to read it:

- **Success path:** all versions are equivalent, within the error margins, and allocate the same
  72 B (the `OrderConfirmation`). On Linux the success path is dominated by `Guid.NewGuid()`.
- **Failure path:** throw/catch is roughly 50x slower than returning a `Result`, and about 100x slower
  than returning a union case.
- `Result` allocates 96 B on failure versus 24 B for a union case: the `Error` record formats its
  `Message` string eagerly, in the constructor.
- The keyword and attribute union forms perform the same, as expected:
  `tests/Orders.Tests/UnionShapeTests.cs` checks that the keyword emits a `[Union]` struct implementing `IUnion`.

Caveats: `ShortRun` (3 iterations) on a shared cloud VM, so treat the ratios as an order of magnitude,
not the nanoseconds. The call stack is shallow (one frame between `throw` and `catch`), and exceptions
get more expensive as the stack gets deeper. Raw reports are in [docs/benchmarks](docs/benchmarks).
