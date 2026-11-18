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

## 5. To do before publishing

- [ ] Re-run `dotnet run -c Release --project benchmarks/Orders.Benchmarks` (full job, not `--job short`)
      on the reference machine and replace the README table plus `docs/images/benchmark.png`.
      The PNG still shows the old 1,542 / 33 / 20 ns figures.
- [ ] Update article prose for items 1 to 5 in section 2.
- [ ] Update `namespace` / `using` lines in article snippets (section 4).
- [ ] When BenchmarkDotNet 0.16.0 ships stable, bump `Directory.Packages.props`.
