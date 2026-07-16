/*
The MIT License (MIT)

Copyright (c) 2019 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
