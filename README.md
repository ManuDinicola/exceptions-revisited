# Exception Handling in C#, Revisited

Companion repo for the article **"Exceptions, Result<T>, or Unions? Exception Handling in C# 15"**.

The same use case (`PlaceOrder`) is implemented three ways, with identical business rules:

| Folder | Approach | Failure is... |
|---|---|---|
| `Orders/V1_Exceptions` | `throw` / `catch` | invisible in the signature |
| `Orders/V2_Result` | hand-rolled `Result<T>` + open `Error` hierarchy | visible, but the set of errors is open |
| `Orders/V3_Unions` | C# 15 `union` | visible **and** closed: the compiler checks exhaustiveness |

## Run it

Requires the .NET 11 SDK (tested with `11.0.100-rc.2`). Unions are still behind `<LangVersion>preview</LangVersion>`.

```bash
dotnet run  --project Orders.Demo                 # same 6 requests through all 3 versions
dotnet test Orders.Tests                          # 3 tests
dotnet run  --project Orders.Benchmarks -c Release
```

## The interesting bit

`Orders.Demo/Program.cs` shows the caller side. Notice that for a customer over their credit limit, V1 and V2 answer **400**
(the catch-all swallowed a case nobody handled), while V3 answers **402**, because the compiler would not let us skip it.
Delete any arm of the V3 `switch` and the build fails with `CS8509` (we promote it to an error in `Orders.Demo.csproj`).

## Benchmarks

See [benchmark-results.md](benchmark-results.md). Short run, shallow call stack: treat the ratios as an order of magnitude, not gospel.
