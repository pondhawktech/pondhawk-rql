<p align="center">
  <img src="pht-small-logo.png" alt="Pondhawk.Rql" width="120" />
</p>

<h1 align="center">Pondhawk.Rql</h1>

<p align="center">
  A Resource Query Language — filtering DSL with an AST, fluent builder, text parser, and RQL / LINQ / SQL / description serialization.
</p>

<p align="center">
  <a href="https://github.com/pondhawktech/pondhawk-rql/actions/workflows/build.yml"><img src="https://github.com/pondhawktech/pondhawk-rql/actions/workflows/build.yml/badge.svg" alt="Build" /></a>
  <img src="https://img.shields.io/badge/.NET-8.0-512bd4" alt=".NET 8" />
  <img src="https://img.shields.io/badge/license-MIT-blue" alt="MIT License" />
  <a href="https://www.nuget.org/packages/Pondhawk.Rql"><img src="https://img.shields.io/nuget/v/Pondhawk.Rql?label=NuGet" alt="Pondhawk.Rql on NuGet" /></a>
</p>

A **Resource Query Language (RQL)** — a filtering DSL with an AST, a strongly-typed fluent builder, a text parser, and serialization to RQL text, LINQ expressions, parameterized SQL, and human-readable English. Fully standalone: its only dependencies are [Sprache](https://github.com/sprache/Sprache) (parser combinators) and CommunityToolkit.Diagnostics.

```csharp
var filter = RqlFilterBuilder<Product>
    .Where(p => p.Category).Equals("Electronics")
    .And(p => p.Price).GreaterThan(99);

var results = dbContext.Products.Where(filter.ToExpression<Product>());   // IQueryable / EF Core
string rql   = filter.ToRql();                                            // (eq(Category,'Electronics'),gt(Price,99))
string human = filter.ToDescription();                                    // Category equals 'Electronics' and Price is greater than 99
```

## Installation

```bash
dotnet add package Pondhawk.Rql
```

## Fluent Builder

Build filters with strongly-typed lambda expressions:

```csharp
using Pondhawk.Rql.Builder;

var filter = RqlFilterBuilder<Product>
    .Where(p => p.Category).Equals("Electronics")
    .And(p => p.Price).GreaterThan(99)
    .And(p => p.InStock).Equals(true);
```

## Supported Operators

| Builder Method | RQL Syntax | SQL (`WHERE` fragment) |
|----------------|-----------|------------------------|
| `.Equals(v)` | `eq(Field,v)` | `Field = {0}` |
| `.NotEquals(v)` | `ne(Field,v)` | `Field <> {0}` |
| `.LesserThan(v)` | `lt(Field,v)` | `Field < {0}` |
| `.GreaterThan(v)` | `gt(Field,v)` | `Field > {0}` |
| `.LesserThanOrEqual(v)` | `le(Field,v)` | `Field <= {0}` |
| `.GreaterThanOrEqual(v)` | `ge(Field,v)` | `Field >= {0}` |
| `.Between(a, b)` | `bt(Field,a,b)` | `Field between {0} and {1}` |
| `.In(a, b, c)` | `in(Field,a,b,c)` | `Field in ({0},{1},{2})` |
| `.NotIn(a, b, c)` | `ni(Field,a,b,c)` | `Field not in ({0},{1},{2})` |
| `.StartsWith(v)` | `sw(Field,v)` | `Field like {0}`  (parameter `v%`) |
| `.Contains(v)` | `cn(Field,v)` | `Field like {0}`  (parameter `%v%`) |
| `.EndsWith(v)` | `ew(Field,v)` | `Field like {0}`  (parameter `%v`) |
| `.IsNull()` | `nu(Field)` | `Field is null` |
| `.IsNotNull()` | `nn(Field)` | `Field is not null` |

Values support `string`, `bool`, `short`, `int`, `long`, `decimal`, and `DateTime`. For `like` operators the `%` wildcards live in the **parameter value**, so the query stays fully parameterized.

## Serialization

A single filter serializes to four targets:

```csharp
var filter = RqlFilterBuilder<Product>
    .Where(p => p.Category).Equals("Electronics")
    .And(p => p.Price).GreaterThan(99);

// 1. RQL text
string rql = filter.ToRql();
// "(eq(Category,'Electronics'),gt(Price,99))"

// 2a. Compiled LINQ predicate (in-memory filtering)
Func<Product, bool> predicate = filter.ToLambda<Product>();
var matches = products.Where(predicate);

// 2b. Expression tree (IQueryable / EF Core)
Expression<Func<Product, bool>> expr = filter.ToExpression<Product>();
var results = dbContext.Products.Where(expr);

// 3. Parameterized SQL
var (sql, parameters) = filter.ToSqlQuery<Product>();
// sql:        "select * from Product where Category = {0} and Price > {1}"
// parameters: ["Electronics", 99]

var (where, args) = filter.ToSqlWhere();          // WHERE fragment only
// where: "Category = {0} and Price > {1}"

// 4. Human-readable English
string description = filter.ToDescription();
// "Category equals 'Electronics' and Price is greater than 99"
```

`ToSqlQuery`/`ToSqlWhere` default to indexed placeholders (`{0}`, `{1}`); pass `indexed: false` for positional `?` placeholders. `ToLambda`/`ToExpression` accept `insensitive: true` for case-insensitive string comparisons:

```csharp
var predicate = filter.ToLambda<Product>(insensitive: true);
```

## Parsing

Parse RQL text back into a filter AST:

```csharp
using Pondhawk.Rql.Parser;

var tree = RqlLanguageParser.ToCriteria("(eq(Status,'Active'),gt(Total,#100))");
// tree.Criteria contains the parsed predicates
```

Value prefixes in RQL text:

- **Strings** — single-quoted, escape `'` as `''` (`'O''Brien'`)
- **DateTime** — `@` prefix (`@2025-01-15T00:00:00Z`)
- **Decimal** — `#` prefix (`#99.95`)
- **Integers & booleans** — bare (`30`, `true`)

## Introspection

Build filters automatically from a criteria object whose properties are decorated with `[Criterion]` (the object implements `ICriteria` — `BaseCriteria` is a convenient base):

```csharp
using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Criteria;

public class ProductSearch : BaseCriteria
{
    [Criterion(Operation = RqlOperator.Contains)]
    public string? Name { get; set; }

    [Criterion(Operation = RqlOperator.Equals)]
    public string? Category { get; set; }

    [Criterion(Name = "Price", Operand = OperandKind.From)]
    public decimal? MinPrice { get; set; }

    [Criterion(Name = "Price", Operand = OperandKind.To)]
    public decimal? MaxPrice { get; set; }
}

var search = new ProductSearch { Category = "Electronics", MinPrice = 50m, MaxPrice = 200m };
var filter = RqlFilterBuilder<Product>.Create().Introspect(search);
// Produces: eq(Category,'Electronics'), bt(Price,#50,#200)
```

Null-valued properties are skipped, so the same criteria object drives optional search fields. `OperandKind.From`/`OperandKind.To` sharing one `Name` collapse into a single `Between`.

## Untyped Builder

For dynamic scenarios where the target type isn't known at compile time:

```csharp
var filter = RqlFilterBuilder
    .Where("Status").Equals("Active")
    .And("CreatedDate").GreaterThan(DateTime.UtcNow.AddDays(-30));

var (sql, args) = filter.ToSqlWhere();
```

## Namespaces

| Namespace | Contents |
|-----------|----------|
| `Pondhawk.Rql.Builder` | `RqlFilterBuilder<T>` / `RqlFilterBuilder`, `RqlOperator`, `RqlTree`, `[Criterion]`, `OperandKind` |
| `Pondhawk.Rql.Parser` | `RqlLanguageParser` (RQL text → AST) |
| `Pondhawk.Rql.Serialization` | `ToRql` / `ToLambda` / `ToExpression` / `ToSqlQuery` / `ToSqlWhere` / `ToDescription` extensions |
| `Pondhawk.Rql.Criteria` | `ICriteria` / `BaseCriteria` for introspection |

## Repository Layout

```
src/Pondhawk.Rql/         The library (net8.0)
test/Pondhawk.Rql.Tests/  xUnit tests (250 tests)
build/                    Cake (Frosting) build script
.github/workflows/        build.yml (build/test/pack → GitHub Packages) + publish.yml (→ NuGet.org)
```

## Building

```bash
# Restore, build, and test via the Cake script
dotnet run --project build/Build.csproj -- --target=Test

# Or use the SDK directly
dotnet build pondhawk-rql.slnx
dotnet test pondhawk-rql.slnx
```

## Versioning & Packaging

The package version is `major.minor` from [`src/Pondhawk.Rql/version.json`](src/Pondhawk.Rql/version.json) with a build number as the patch. Off-CI builds get a `-local` prerelease suffix.

```bash
dotnet run --project build/Build.csproj -- --target=Pack --build-number=<n>   # writes ./artifacts/*.nupkg
```

## CI/CD

- **`build.yml`** — on push/PR to `main`: build + test; on `main` it also packs and pushes to the `pondhawktech` GitHub Packages feed and uploads the `.nupkg` as an artifact.
- **`publish.yml`** — manual `workflow_dispatch` that promotes a build's artifact to **[NuGet.org](https://www.nuget.org/packages/Pondhawk.Rql)** (uses the org-level `NUGET_ORG_API_KEY`).

## History

Extracted from the [pondhawktech/tools](https://github.com/pondhawktech/tools) monorepo, where it lived as `src/Pondhawk.Rql` and had no internal dependents.

## License

MIT — see [LICENSE](LICENSE).
