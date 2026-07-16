// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedTypeParameter

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Rql;

/// <summary>
/// Represents a collection of RQL predicates that can be serialized to SQL, LINQ, or RQL text.
/// </summary>
public interface IRqlFilter
{

    /// <summary>The target entity type for this filter.</summary>
    Type Target { get; }

    /// <summary>Returns <c>true</c> if the target type is compatible with <typeparamref name="TTarget"/>.</summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Is<T> is the intended fluent API name for type checking")]
    bool Is<TTarget>();

    /// <summary>Returns <c>true</c> if this filter contains at least one predicate.</summary>
    bool HasCriteria { get; }

    /// <summary>The predicates in this filter.</summary>
    IEnumerable<IRqlPredicate> Criteria { get; }

    /// <summary>Maximum number of rows to return. Zero means unlimited.</summary>
    int RowLimit { get; set; }

    /// <summary>Returns <c>true</c> if at least one predicate matches the given condition.</summary>
    bool AtLeastOne(Func<IRqlPredicate, bool> predicate);

    /// <summary>Returns <c>true</c> if exactly one predicate matches the given condition.</summary>
    bool OnlyOne(Func<IRqlPredicate, bool> predicate);

    /// <summary>Returns <c>true</c> if no predicates match the given condition.</summary>
    bool None(Func<IRqlPredicate, bool> predicate);

    /// <summary>Adds a predicate to the filter.</summary>
    void Add(IRqlPredicate operation);

    /// <summary>Removes all predicates from the filter.</summary>
    void Clear();

}


/// <summary>
/// Strongly-typed variant of <see cref="IRqlFilter"/> bound to entity type <typeparamref name="TEntity"/>.
/// </summary>
public interface IRqlFilter<out TEntity> : IRqlFilter where TEntity : class;
