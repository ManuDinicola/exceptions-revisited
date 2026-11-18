# Technical review: "Your C# Methods Are Lying About What They Return"

Review of the companion repo and the claims it backs, after the move to .NET 11 GA (SDK 11.0.100,
runtime 11.0.0) and BenchmarkDotNet 0.16.0-preview.2. Each claim was re-checked by building and
running the code, not by reading it.

## 1. Claims verified on .NET 11 GA

| Claim | Status | Evidence |
|---|---|---|
| Unions are C# 15 | ✅, and **no longer preview** | Builds with `LangVersion` 15 and default. `LangVersion` 14 fails with `CS1001` on the `union` declaration. |
| Removing an arm is a compile error naming the case | ✅ | `CS8509 … the pattern 'Orders.Unions.CreditLimitExceeded' is not covered`. It is a *warning* by default; the repo promotes it. |
| The keyword lowers to a `[Union]` struct implementing `IUnion` | ✅ | `UnionShapeTests` checks both forms: value type, `UnionAttribute`, `IUnion`, one constructor per case. |
| V1/V2 answer 400 where V3 answers 402 | ✅ | Demo output unchanged; `docs/images/demo-output.png` is still accurate. |
| Success path is equivalent across approaches | ✅ | Ratios 0.97 to 1.11, all within error. |
| Throw/catch is much slower on failure | ✅ | ~2,031 ns vs 42 ns (`Result`) vs 19 ns (union), about 50x and 100x. |

## 2. Corrections to make in the article

1. **Drop "preview" wording.** "Unions are still behind `<LangVersion>preview</LangVersion>`" was true
   on RC2 and is false on GA. Say instead: "C# 15, the default for `net11.0`."
2. **The old allocation numbers were a measurement artifact.** In the previous benchmark, success
   methods returned `object`, so `Result<T>` (104 B) and the unions (96 B) paid for **boxing the
   returned struct**. With boxing removed, all four allocate 72 B on success. Any sentence that reads
   meaning into 72 vs 96 vs 104 B needs to go.
3. **The old numbers were in-process.** BenchmarkDotNet 0.15.8 could not build a `net11.0` project,
   so it ran in the host process with the optimizations validator disabled. 0.16 runs out-of-process
   with the default toolchain. Quote only the new run; the old report is kept in
   `docs/benchmarks/` and marked superseded.
4. **The 384 B / 1,542 ns exception figures came from a different shape.** The old `Exceptions_Failure`
   read `e.Message`, while `Result_Failure` read only `IsSuccess`. Every method now consumes the
   result the same way (returns `bool`), so the comparison is apples to apples.
5. **CS8509 is a warning, not an error.** The screenshot is titled "compiler warning", which is right.
   Make sure the prose says the project *promotes* it (`WarningsAsErrors`), or readers will try it in a
   fresh project and see only a squiggle.

## 3. Technical points worth adding (they make the argument stronger)

- **`default(Result<T>)` was a success.** The original struct decided success with `_error is null`,
  so `default` read as "success with a null value", which is precisely the kind of lie the article
  is about. `Result<T>` now carries an explicit `_isSuccess` flag (test: `DefaultResult_IsNotASuccess`).
  Unions are not immune either: `default(PlaceOrderResult)` has `Value == null`, matches none of the
  arms, and the switch throws `SwitchExpressionException` at run time (verified). Nullable analysis
  warns with `CS8655` when it can *see* the `default`, but not across a method boundary. A one-line
  caveat makes the article more honest ("closed, except for `default`").
- **Why `Result` allocates 4x more than a union on failure** (96 B vs 24 B): the `Error` record
  builds its `Message` string eagerly, in the base constructor. That is a design cost of the
  "every error has a message" hierarchy, not of `Result<T>` itself. It is worth one sentence.
- **Depth matters for exceptions.** The benchmark has one frame between `throw` and `catch`. Stack
  unwinding cost grows with depth, so the 50x is a floor, not a ceiling. If you want a figure, a
  `[Params(1, 10, 50)]` depth parameter is the cheapest addition.
- **Platform noise:** on Linux the success path costs ~420 ns, versus ~85 ns on the original
  Windows box. That is `Guid.NewGuid()` (a `getrandom` syscall), not the error-handling strategy.
  Present ratios, not nanoseconds, when comparing machines.

## 4. Structure and naming changes in the repo

- Standard layout: `src/`, `samples/`, `tests/`, `benchmarks/`, `docs/`, with solution folders in
  `ExceptionsRevisited.slnx` that mirror the disk layout.
- Namespaces no longer contain underscores (`Orders.V1_Exceptions` → `Orders.Exceptions`, etc.,
  CA1707). The V1/V2/V3/V3b labels remain the article's narrative device: they are in the README
  table, the demo output and the XML doc comments. **Update any snippet in the article that shows a
  `namespace` or `using` line.**
- V3b moved under `Unions/Handwritten`, so it picks up the shared case records from the enclosing
  namespace without `using Orders.V3_Unions`.
- One concept per file (cases, union, service), so each file can be embedded as a gist on its own.
- Shared settings in `Directory.Build.props` (net11.0, C# 15, nullable, warnings as errors,
  `latest-recommended` analyzers), package versions in `Directory.Packages.props`.
- Tests moved to xUnit v3 on Microsoft.Testing.Platform (`global.json` → `test.runner`).
  They went from 5 facts to 30 cases, and now cover all six failure rules (`UnknownProduct` was untested).
- CI workflow: build, test and demo on every push.

## 5. Review of the published articles the revisit builds on

Source: the author's Medium RSS feed. The article page itself sits behind a bot challenge.

### 5.1 "Exception Handling in C#" (Dec 2024): what the revisit should correct or supersede

| § | Original statement | Issue | Suggested revisit wording |
|---|---|---|---|
| 2 | "Managed exceptions" vs "unhandled exceptions" | In .NET, *managed exception* means a CLR exception as opposed to a native one; every C# exception is managed. The real distinction is **expected** (part of the contract) vs **unexpected** (a bug or environment failure). | Use *expected outcome* vs *exceptional failure*. That split is the whole thesis of the revisit: expected → union case, unexceptional → `throw`. |
| 3 | `catch (Exception ex)` as the template | Teaches the catch-all that the revisit later shows hiding cases. Missing: `throw;` vs `throw ex;` (stack trace loss), exception filters (`catch (X e) when (...)`), `ExceptionDispatchInfo`. | Show a specific catch first; mention `throw;` and `when` in one paragraph. |
| 5.1 | `UseExceptionHandler` writes `StatusCode = 400` and `exception.Error.Message` | An unhandled exception is a server fault (**500**), not a client error. Echoing `Message` can leak internals. | 500 + ProblemDetails, no raw message. |
| 5.2 | Custom middleware returns `Title = exception.Message`, `Detail = exception.StackTrace` | **Information disclosure** in production (stack traces, paths, SQL fragments). Also `UseRouting` / `UseEndpoints` is pre-.NET 6 style. | Remove `StackTrace`; use `app.MapControllers()`. Better: drop 5.2 and point to 5.3. |
| 5.3 | Dev/QA/ACC responses include **all request headers** | Headers carry `Authorization` and `Cookie`, so tokens end up in response bodies and logs on shared QA environments. | Include `exception.ToString()` only in `Development`; never echo headers. |
| 5.3 | Suppress `ExceptionHandlerMiddleware` logs through config/Serilog | Since ASP.NET Core 10, the middleware no longer emits its diagnostics when `TryHandleAsync` returns `true` (configurable with `ExceptionHandlerOptions.SuppressDiagnosticsCallback`). The workaround is now unnecessary. *Check the exact behaviour in the .NET 10 breaking-changes list before citing it.* | One sentence: "On .NET 10+, handled exceptions are no longer double-logged." |
| 6 | `Result<T>` with `string? Error`, class-based | Stringly-typed error, and a class allocates on success too. The revisit's V2 (typed `Error` records) is already the better version; say so explicitly, as the bridge between the two articles. | "In 2024 I used a string; here's why that wasn't enough." |
| 7.2 | Custom exception with the three standard constructors | Fine. Mark it `sealed` and note that the `[Serializable]` / serialization constructor is obsolete (SYSLIB0051) since .NET 8. | |
| 9 | Benchmark: 1,000-iteration loop, half failing, `catch { }`, `EtwProfiler`, `ThreadingDiagnoser` | Mixes the success and failure paths in one number. The `Result` class allocates on both paths. `EtwProfiler` is Windows-only and `ThreadingDiagnoser` measures nothing relevant. The results are an embedded image with no numbers in the text. | The revisit fixes all of this. Say so: separate success/failure categories, baseline ratios, out-of-process, raw report in the repo. |
| 9 | "Exceptions … are performance-intensive" | Still true, but .NET 9 rewrote managed exception handling (ported from NativeAOT), making throw/catch roughly 2 to 4x faster than .NET 8. The gap shrank and is still ~50 to 100x here. | Mention that the gap narrowed and is still decisive on hot failure paths. |

### 5.2 "Why Union Types Might Be the Most Important C# 15 Feature" (Oct 2026)

- **Exhaustiveness is checked for switch *expressions*, not switch *statements*.** Verified on
  SDK 11.0.100: a `switch` statement over `PlaceOrderResult` handling 2 of 7 cases compiles with
  **no diagnostic**, while the equivalent switch expression reports `CS8509`. The "Compile-Time
  Exhaustiveness" section and `MakePetSpeak` use switch statements, so the claim "the compiler can
  detect it" does not hold as written. Either switch to expressions, or note the difference. This
  matters for the revisit too: the demo deliberately uses switch expressions.
- "No default case is required" is true for both forms. Only the expression form *proves* it.
- Section 5 (null): add the `default(union)` caveat from §3 above.

## 6. To do before publishing

- [ ] Re-run `dotnet run -c Release --project benchmarks/Orders.Benchmarks` (full job, not `--job short`)
      on the reference machine and replace the README table plus `docs/images/benchmark.png`.
      The PNG still shows the old 1,542 / 33 / 20 ns figures.
- [ ] Update article prose for items 1 to 5 in section 2, and decide which §5.1 corrections the revisit should mention explicitly.
- [ ] Fix the switch-statement exhaustiveness claim in the union types article (§5.2).
- [ ] Update `namespace` / `using` lines in article snippets (section 4).
- [ ] When BenchmarkDotNet 0.16.0 ships stable, bump `Directory.Packages.props`.
