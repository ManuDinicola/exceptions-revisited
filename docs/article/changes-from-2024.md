# 2024 → 2026: what changed between the two articles

Section-by-section comparison of
[Exception Handling in C#](https://medium.com/@manu.dinicola/exception-handling-in-c-5f612fc52195)
(December 2024, .NET 8) and [the revisit](article.md) (November 2026, .NET 11 / C# 15).

Legend: **Kept** = still valid · **Updated** = right idea, better code · **Fixed** = was wrong ·
**Removed** = obsolete · **New** = did not exist in 2024.

| 2024 section | Status | What changed in 2026 | Verified by |
|---|---|---|---|
| Intro | Updated | Adds the three changes: .NET 9 exception handling rewrite, ASP.NET Core 10 logging fix, C# 15 unions. | — |
| §1 What is an exception | Updated | Reframed as **expected outcomes vs unexpected failures**. This is now the thesis of the article. | — |
| §2 "Managed vs unhandled" | **Fixed** | Every C# exception is managed; the terms were misleading. Replaced by expected/unexpected. | — |
| §3 try/catch/finally | Updated | `catch (Exception)` template replaced by specific catch + `when` filter + `throw;` + wrapping with inner exception. | Snippet compiled |
| §4.1 Input validation | Updated | Hand-written `if` replaced by `ArgumentNullException.ThrowIfNull` & co., plus the distinction between programming errors (throw) and user input (return). | Snippet compiled |
| §4.2 TryParse | Kept | Generalised to the `Try` pattern, and its limit (yes/no, no *why*) leads into §6. | — |
| §5.1 `UseExceptionHandler` lambda | **Removed** | Returned **400** for crashes (should be 500) and echoed `Message`. | — |
| §5.2 Custom middleware | **Removed / Fixed** | Sent `Message` + **`StackTrace`** to clients. Also used the pre-.NET 6 `UseRouting`/`UseEndpoints` style. | — |
| §5.3 `IExceptionHandler` | **Fixed** | Still the recommended approach, but no longer copies **request headers** (tokens, cookies) into responses; details only in `Development`; source-generated logging. | `OrderApiTests`: 500 + `application/problem+json`, no secret, no exception type in body |
| §5 Suppressing middleware logs | **Removed** | Unnecessary since ASP.NET Core 10: diagnostics are suppressed when an `IExceptionHandler` handles the exception (`SuppressDiagnosticsCallback`). | Ref-pack XML docs + test log: one error entry |
| §6 Result objects | Updated | `string? Error` → typed `Error` records. Shows why an open hierarchy still forces a discard arm that hides cases. Documents the `default(Result<T>)` trap. | `OrderServiceTests`, `DefaultResult_IsNotASuccess` |
| — | **New** | §6 the 400-vs-402 bug: identical in exceptions and `Result`, impossible with unions. | Demo + `EveryOutcome_MapsToItsOwnStatus` |
| — | **New** | §6.2 C# 15 `union`, switch expression, CS8509 as an error. | Build fails when an arm is removed |
| — | **New** | §6.3 hand-written `[Union]` / `IUnion` equivalent. | `UnionShapeTests` |
| — | **New** | §7 union pitfalls: switch statements are not checked; `default` throws `SwitchExpressionException`; don't model infrastructure failures as cases. | Compiled and ran on SDK 11.0.100 |
| §7 Custom exceptions | Updated | Same three constructors, plus `sealed`, no serialization constructor (SYSLIB0051), data exposed as properties. | Snippet compiled |
| §8 Best practices | Kept | Plus: "a custom exception for a business rule is a union case waiting to happen". | — |
| §9 Limit exceptions | Updated | Performance is now the secondary argument; the primary one is that signatures should be honest. | — |
| §9 Benchmark | **Fixed** | Mixed success and failure in one loop; class-based `Result` allocated on both paths; Windows-only `EtwProfiler`; results only as an image. Now: separate categories, baseline ratios, out-of-process BDN 0.16, .NET 8 vs .NET 11 by stack depth, raw reports committed. | `docs/benchmarks/` |
| — | **New** | §10 decision guide. | — |

## Corrections to the October 2026 union types article

- "Compile-Time Exhaustiveness" uses a `switch` **statement**: statements are not checked.
  Use a switch expression, or note the difference.
- §5 (null) could mention that `default(union)` matches no case.
