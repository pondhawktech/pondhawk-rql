// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Rql.Serialization;

/// <summary>
/// How <see cref="LambdaSerializerExtensions.ToExpression{TEntity}(Pondhawk.Rql.IRqlFilter{TEntity}, RqlExpressionOptions)"/>
/// builds a LINQ expression from a filter.
/// </summary>
/// <remarks>
/// The default options build the same expressions as the <c>ToExpression(bool insensitive)</c> overload. Set
/// <see cref="QueryProvider"/> when the expression runs against an <c>IQueryable</c> whose provider translates it to
/// SQL (such as EF Core), and <see cref="ValueConverter"/> when a property's type is one RQL cannot convert an operand
/// to by itself (such as a value object).
/// </remarks>
public sealed class RqlExpressionOptions
{
    /// <summary>The default options: case-sensitive, in-memory expressions, the built-in value conversion.</summary>
    public static RqlExpressionOptions Default { get; } = new();

    /// <summary>When <c>true</c>, string comparisons (starts with, contains, ends with) ignore case.</summary>
    public bool CaseInsensitive { get; init; }

    /// <summary>
    /// When <c>true</c>, only expressions a LINQ query provider can translate are built: a case-insensitive comparison
    /// upper-cases both sides (<c>ToUpper()</c> on the property, the operand upper-cased invariantly) instead of using a
    /// <see cref="StringComparison"/> overload, and <c>in</c> / <c>not in</c> become equalities typed to the property,
    /// joined with or, instead of a list of boxed values.
    /// </summary>
    public bool QueryProvider { get; init; }

    /// <summary>
    /// Converts an operand to the type of the property it is compared with, when the two differ; <c>null</c> uses
    /// <see cref="RqlValueConverter.Convert"/>. A converter can handle its own types and fall back to
    /// <see cref="RqlValueConverter.Convert"/> for the rest.
    /// </summary>
    public Func<object, Type, object>? ValueConverter { get; init; }
}
