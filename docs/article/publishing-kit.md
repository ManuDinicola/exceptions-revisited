# Publishing kit

Everything needed to publish [the article](article.md) and get it read.

## Title options

1. **Your C# Methods Are Lying About What They Return** (recommended: matches the cover image, makes a claim, creates curiosity)
2. Exceptions Got 5× Faster in .NET 11. I Still Stopped Using Them for Business Errors.
3. The 400 That Should Have Been a 402: C# 15 Unions vs Exceptions
4. Exception Handling in C#, Two Years Later: What .NET 11 Changed

Subtitle: *Exception handling, revisited for .NET 11: what two years, three runtimes and C# 15 unions changed.*

## Medium tags (5 max)

`CSharp` · `Dotnet` · `Software Engineering` · `Programming` · `Software Architecture`

## Before publishing

- [ ] Re-run both benchmarks on the reference machine and update the two code-block tables
      (`dotnet run -c Release --project benchmarks/Orders.Benchmarks` and the `Exceptions.Runtimes` command
      in the README). Regenerate `docs/images/benchmark.png` if you use it.
- [ ] Medium does not render Markdown tables: the article keeps its tables in code blocks, so they paste as-is.
- [ ] Upload the images (`cover.png`, `demo-output.png`); the relative paths won't survive the paste.
- [ ] Turn the long snippets into GitHub gists if you prefer syntax highlighting: one file per concept is
      already the layout in `src/Orders/`.
- [ ] Add a short note at the top of the **2024 article**: "Updated for .NET 11: read the 2026 version →".
      Old posts still get search traffic; this sends it to the new one.
- [ ] Add a correction note to the **October union article** about switch statements, linking to the new
      article's "Three traps" section.
- [ ] Submit to a publication (for example *Level Up Coding* or *ITNEXT*) for distribution beyond your followers.

## LinkedIn / X post

> A customer with a credit limit of 50 orders a 900 laptop. Our API answers 400 Bad Request.
> It should have been 402, and the compiler never said a word.
>
> Exceptions and Result<T> share the same blind spot: a catch-all quietly swallows new cases.
> C# 15 unions close that gap. Forget a case and the build fails.
>
> I also re-ran my 2024 benchmarks: on .NET 11, exceptions are 2.5 to 5× faster than on .NET 8,
> and still about 100× slower than returning a value.
>
> Article + repo (38 tests, both benchmarks) 👇
> #dotnet #csharp

## Short version (for newsletters, Reddit r/dotnet, Discord)

> I revisited my 2024 exception-handling article for .NET 11. Three findings: C# 15 unions + CS8509 turn
> "forgot to handle a case" into a build error; switch *statements* over unions are **not** checked (only
> expressions are); and exceptions got 2.5 to 5× faster since .NET 8, but are still ~100× slower than returning.
> Repo with tests and BenchmarkDotNet 0.16 reports: github.com/ManuDinicola/exceptions-revisited
