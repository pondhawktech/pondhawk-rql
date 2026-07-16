# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore, build, and test via the Cake build script
dotnet run --project build/Build.csproj -- --target=Test

# Build the solution directly
dotnet build pondhawk-rql.slnx

# Build just the library
dotnet build src/Pondhawk.Rql/Pondhawk.Rql.csproj

# Run tests directly
dotnet test pondhawk-rql.slnx

# Pack NuGet packages (writes to ./artifacts)
dotnet run --project build/Build.csproj -- --target=Pack --build-number=<n>
```

## Project Setup

- **.NET 8** targeting `net8.0` (`LangVersion=latestmajor`)
- **Central package management** via `Directory.Packages.props`
- **Nullable reference types** enabled; `TreatWarningsAsErrors` on; Meziantou analyzer enforced (`src/Directory.Build.props`)
- Versioning: `src/Pondhawk.Rql/version.json` holds `major.minor`; the Cake `Pack` target appends the build number (and a `-local`/`-<suffix>` prerelease tag off CI).

## Architecture

**Pondhawk.Rql** is a Resource Query Language: a filtering DSL with an AST, fluent builder, text parser, and multiple serialization targets. It is fully standalone — no dependency on any other Pondhawk package. Its only runtime dependencies are **Sprache** (parser combinators) and **CommunityToolkit.Diagnostics** (guard clauses).

- **AST**: `RqlTree` (root) contains `Criteria` (list of `IRqlPredicate`). `RqlOperator` enum: Equals, NotEquals, LesserThan, GreaterThan, Between, In, NotIn, StartsWith, Contains, etc.
- **Builder** (`Pondhawk.Rql.Builder`): `RqlFilterBuilder<TTarget>` provides fluent API: `.Where(expr).Equals(value).And(expr).GreaterThan(value)`. `Introspect()` builds filters from objects decorated with `[CriterionAttribute]`.
- **Parser** (`Pondhawk.Rql.Parser`): Parses RQL criteria text back into `RqlTree` AST using the **Sprache** parser combinator library. `RqlLanguageParser.ToCriteria(string)` parses criteria. Value type prefixes: `@` for DateTime, `#` for decimal, `'...'` for strings.
- **Serialization** (`Pondhawk.Rql.Serialization`): Four output formats:
  - `ToRql()` — RQL text: `(eq(Name,'John'),gt(Age,30))`
  - `ToLambda<T>()` / `ToExpression<T>()` — compiled LINQ expressions
  - `ToSqlQuery()` / `ToSqlWhere()` — parameterized SQL
  - `ToDescription()` — human-readable English: `"Name equals 'John' and Age is greater than 30"`

## Conventions

- Namespaces match folder structure: `Pondhawk.Rql`, `Pondhawk.Rql.Builder`, `Pondhawk.Rql.Parser`, `Pondhawk.Rql.Serialization`.

## History

This project was extracted from the [pondhawktech/tools](https://github.com/pondhawktech/tools) monorepo, where it lived as `src/Pondhawk.Rql`. It had no internal dependents there, so it moves cleanly into its own repo and publishes to NuGet.org independently.

## CI/CD

- `.github/workflows/build.yml` — builds, tests, packs, and pushes to the pondhawk GitHub Packages feed on pushes to `main`; uploads `.nupkg` artifacts.
- `.github/workflows/publish.yml` — `workflow_dispatch` that promotes a build's artifacts to **NuGet.org** (requires the `NUGET_ORG_API_KEY` repo secret).
