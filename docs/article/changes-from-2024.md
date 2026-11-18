# 2024 → 2026: what changed between the two articles

How the content of
[Exception Handling in C#](https://medium.com/@manu.dinicola/exception-handling-in-c-5f612fc52195)
(December 2024, .NET 8) maps to [the new article](article.md) (November 2026, .NET 11 / C# 15).

The new article does **not** follow the 2024 outline. The 2024 article was a reference guide,
organised by language feature. The new one is a story that runs from a concrete bug to its fix,
so readers have a reason to keep going. Topics from 2024 survive where they serve that story; the
corrections are grouped into one section, "What I got wrong in 2024".

Legend: **Kept** = still valid · **Updated** = right idea, better code · **Fixed** = was wrong ·
**Removed** = obsolete · **New** = did not exist in 2024.

| 2024 topic | Status | Where it lives now | What changed | Verified by |
|---|---|---|---|---|
| What is an exception | Updated | *Exceptions aren't dead. They're specialised.* | Reframed as expected outcomes vs unexpected failures: the thesis of the article. | — |
| "Managed vs unhandled" | **Fixed** | *What I got wrong in 2024* | Every C# exception is managed; the terms were misleading. | — |
| try/catch/finally | Updated | *Exceptions aren't dead* | `catch (Exception)` template replaced by specific catch + `when` filter + `throw;` + wrapping. | Snippet compiled |
| Input validation | Updated | *Exceptions aren't dead* | Built-in guard clauses (`ThrowIfNull` & co.); programming error (throw) vs user input (return). | Snippet compiled |
| TryParse | Kept | *The cheat sheet* | The Try pattern, for "found / not found". | — |
| `UseExceptionHandler` lambda | **Removed** | *What I got wrong in 2024* | Returned **400** for crashes and echoed `Message`. | — |
| Custom middleware | **Removed** | *What I got wrong in 2024* | Sent `Message` + **`StackTrace`** to clients; pre-.NET 6 routing style. | — |
| `IExceptionHandler` | **Fixed** | *One place for the unexpected* | No longer copies request headers (tokens, cookies) into responses; details only in Development; source-generated logging. | `OrderApiTests` |
| Suppressing middleware logs | **Removed** | *One place for the unexpected* | Unnecessary since ASP.NET Core 10 (`SuppressDiagnosticsCallback`). | Ref-pack docs + test log: one entry |
| Result objects | Updated | *The fix I recommended in 2024, and why it isn't enough* | `string? Error` → typed records; shows the discard arm that hides cases. | `OrderServiceTests` |
| Custom exceptions | Updated | *Exceptions aren't dead* | `sealed`, data as properties, no serialization constructor (SYSLIB0051). | Snippet compiled |
| Best practices | Kept | *Exceptions aren't dead* | Plus "a business-rule exception is a union case waiting to happen". | — |
| "Limit exceptions, they're slow" | Updated | *What I got wrong* + *The numbers* | Still true, now the secondary argument. | — |
| Benchmark | **Fixed** | *The numbers* | Separate success/failure, baselines, out-of-process BDN 0.16, .NET 8 vs .NET 11 by depth. | `docs/benchmarks/` |
| — | **New** | *A valid order, a wrong answer* | Opens on the 400-instead-of-402 bug. | Demo + API tests |
| — | **New** | *C# 15 unions* | `union`, switch expression, CS8509 as an error, `[Union]` lowering. | Build fails when an arm is removed; `UnionShapeTests` |
| — | **New** | *Three traps* | Switch statements aren't checked; `default`; don't model infrastructure failures. | Compiled and ran on SDK 11.0.100 |
| — | **New** | *The cheat sheet*, *Try it yourself* | Decision tree; hands-on call to action. | — |

## Corrections to the October 2026 union types article

- "Compile-Time Exhaustiveness" uses a `switch` **statement**: statements are not checked.
  The new article says so openly (*Three traps*), which also works as a hook to the older post.
- Its null section could mention that `default(union)` matches no case.
