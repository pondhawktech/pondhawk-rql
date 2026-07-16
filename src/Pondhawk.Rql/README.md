# Pondhawk.Rql

A Resource Query Language (RQL) implementation with AST, fluent builder, text parser, and serialization to LINQ lambdas, SQL, and RQL text. Standalone -- no dependency on other Pondhawk packages.

## Quick Start

### Build Filters

```csharp
using Pondhawk.Rql.Builder;

var filter = RqlFilterBuilder<Product>
    .Where(p => p.Name).Equals("Widget")
    .And(p => p.Quantity).GreaterThan(5)
    .And(p => p.Price).LesserThan(100m);
```

### Serialize to LINQ Lambda

```csharp
using Pondhawk.Rql.Serialization;

Func<Product, bool> lambda = filter.ToLambda();
bool matches = lambda(new Product { Name = "Widget", Quantity = 10, Price = 50m });

// Or get a compiled Expression<Func<T, bool>> for EF Core / IQueryable:
Expression<Func<Product, bool>> expr = filter.ToExpression();
var results = dbContext.Products.Where(expr);
```

### Serialize to SQL

```csharp
var (sql, parameters) = filter.ToSqlWhere();
// sql:        "Name = {0} and Quantity > {1} and Price < {2}"
// parameters: ["Widget", 5, 100]

var (query, queryParams) = filter.ToSqlQuery("Products");
// query: "select * from Products where Name = {0} and Quantity > {1} and Price < {2}"
```

### Serialize to RQL Text

```csharp
string rql = filter.ToRql();
// "(eq(Name,'Widget'),gt(Quantity,5),lt(Price,#100))"
```

### Serialize to English Description

```csharp
string description = filter.ToDescription();
// "Name equals 'Widget' and Quantity is greater than 5 and Price is less than 100"
```

### Parse RQL Text

```csharp
using Pondhawk.Rql.Parser;

var tree = RqlLanguageParser.ToCriteria("(eq(Name,'Widget'),gt(Quantity,10))");
// tree.Criteria contains the parsed predicates
```

### Build Filters from Annotated Objects

```csharp
public class ProductFilter
{
    [Criterion("Name", RqlOperator.Equals)]
    public string Name { get; set; }

    [Criterion("Quantity", RqlOperator.GreaterThan)]
    public int? MinQuantity { get; set; }
}

var criteria = new ProductFilter { Name = "Widget", MinQuantity = 5 };
var filter = RqlFilterBuilder.Introspect(criteria);
```

## RQL Operators

| Operator | Builder Method | RQL Syntax | SQL Output |
|----------|---------------|------------|------------|
| Equals | `.Equals(v)` | `eq(Field,v)` | `Field = ?` |
| NotEquals | `.NotEquals(v)` | `ne(Field,v)` | `Field <> ?` |
| LesserThan | `.LesserThan(v)` | `lt(Field,v)` | `Field < ?` |
| GreaterThan | `.GreaterThan(v)` | `gt(Field,v)` | `Field > ?` |
| LesserThanOrEqual | `.LesserThanOrEqual(v)` | `le(Field,v)` | `Field <= ?` |
| GreaterThanOrEqual | `.GreaterThanOrEqual(v)` | `ge(Field,v)` | `Field >= ?` |
| Between | `.Between(lo, hi)` | `bw(Field,lo,hi)` | `Field between ? and ?` |
| In | `.In(v1, v2, ...)` | `in(Field,v1,v2)` | `Field in (?,?)` |
| NotIn | `.NotIn(v1, v2, ...)` | `ni(Field,v1,v2)` | `Field not in (?,?)` |
| StartsWith | `.StartsWith(v)` | `sw(Field,v)` | `Field like ?%` |
| Contains | `.Contains(v)` | `cn(Field,v)` | `Field like %?%` |
| EndsWith | `.EndsWith(v)` | `ew(Field,v)` | `Field like %?` |
| IsNull | `.IsNull()` | `nu(Field)` | `Field is null` |
| IsNotNull | `.IsNotNull()` | `nn(Field)` | `Field is not null` |

## RQL Value Prefixes

When parsing RQL text, values use type prefixes:

- **Strings**: `'value'` (single-quoted)
- **Decimals**: `#19.99` (hash prefix)
- **DateTimes**: `@2024-01-15T00:00:00Z` (at-sign prefix)
- **Integers**: `42` (no prefix)
