/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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


// ReSharper disable UnusedMember.Global

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rql.Criteria;

namespace Pondhawk.Rql.Builder;

/// <summary>
/// Base class for RQL filter builders providing fluent predicate construction with operators
/// such as <c>Equals</c>, <c>Between</c>, <c>In</c>, <c>StartsWith</c>, and more.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type for fluent method chaining.</typeparam>
public abstract class AbstractFilterBuilder<TBuilder> : IRqlFilter where TBuilder : AbstractFilterBuilder<TBuilder>
{



    /// <summary>
    /// Implicitly converts the builder to a list of predicates.
    /// </summary>
    /// <param name="builder">The builder to convert.</param>
    /// <returns>A new list containing the builder's predicates.</returns>
    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation", Justification = "Implicit operator must declare the concrete target type for the conversion")]
    public static implicit operator List<IRqlPredicate>(AbstractFilterBuilder<TBuilder> builder)
    {
        return new List<IRqlPredicate>(builder.Predicates);
    }

    /// <summary>
    /// Initializes a new empty <see cref="AbstractFilterBuilder{TBuilder}"/>.
    /// </summary>
    protected AbstractFilterBuilder()
    {

        CurrentName = string.Empty;
        Predicates = new List<IRqlPredicate>();

    }

    /// <summary>
    /// Initializes a new <see cref="AbstractFilterBuilder{TBuilder}"/> seeded with predicates from an existing <see cref="RqlTree"/>.
    /// </summary>
    /// <param name="tree">The parsed RQL tree whose predicates seed this builder.</param>
    protected AbstractFilterBuilder(RqlTree tree) : this()
    {

        CurrentName = "";
        Predicates = new List<IRqlPredicate>(tree.Criteria);

    }



    /// <inheritdoc />
    public abstract Type Target { get; }

    /// <inheritdoc />
    public bool Is<TTarget>()
    {

        var result = (Target == typeof(TTarget)) || (Target.IsAssignableFrom(typeof(TTarget))) || (typeof(TTarget).IsAssignableFrom(Target));

        return result;

    }


    #region Criteria related members

    /// <summary>
    /// Builds predicates by reflecting over a criteria object's properties decorated with <see cref="CriterionAttribute"/>.
    /// </summary>
    /// <param name="source">The criteria object to introspect.</param>
    /// <param name="map">Optional dictionary mapping property names to alternate target field names.</param>
    /// <returns>The builder for fluent chaining.</returns>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Introspect method has a large but linear switch/case structure that does not benefit from splitting")]
    public TBuilder Introspect(ICriteria source, IDictionary<string, string>? map = null)
    {

        Guard.IsNotNull(source);


        var parts = new Dictionary<string, RqlPredicate>(StringComparer.Ordinal);

        foreach (var prop in source.GetType().GetProperties())
        {


            if (prop.GetCustomAttribute<CriterionAttribute>() is not { } attr)
                continue;


            if (!prop.CanRead)
                continue;


            var value = prop.GetValue(source);
            if (value == null)
                continue;



            var includeMethod = source.GetType().GetMethod($"Include{prop.Name}");
            if (includeMethod is not null)
            {
                var ret = includeMethod.Invoke(source, Array.Empty<object>());
                if (ret is false)
                    continue;
            }
            else
            {


                if (prop.PropertyType == typeof(ICollection<string>) && ((ICollection<string>)value).Count == 0)
                    continue;
                if (prop.PropertyType == typeof(ICollection<int>) && ((ICollection<int>)value).Count == 0)
                    continue;
                if (prop.PropertyType == typeof(ICollection<long>) && ((ICollection<long>)value).Count == 0)
                    continue;

            }


            var target = string.IsNullOrWhiteSpace(attr.Name) ? prop.Name : attr.Name;
            var dataType = prop.PropertyType;

            if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>))
                dataType = Nullable.GetUnderlyingType(dataType)!;


            switch (attr.Operand)
            {
                case OperandKind.Single:
                case OperandKind.From:
                case OperandKind.To:
                    break;
                case OperandKind.List:
                    dataType = typeof(string);
                    break;
                case OperandKind.ListOfInt:
                    dataType = typeof(int);
                    break;
                case OperandKind.ListOfLong:
                    dataType = typeof(long);
                    break;
            }



            if (!parts.TryGetValue(target, out var oper))
            {

                var op = attr.Operation;
                if (attr.Operation == RqlOperator.NotSet && dataType == typeof(string))
                    op = RqlOperator.StartsWith;
                else if (attr.Operation == RqlOperator.NotSet)
                    op = RqlOperator.Equals;

                var mapped = target;
                if (map != null && map.TryGetValue(target, out var found))
                    mapped = found;

                oper = new RqlPredicate(op, mapped, dataType, Array.Empty<object>());
                parts[target] = oper;

            }



            switch (attr.Operand)
            {

                case OperandKind.Single:
                case OperandKind.From:
                    oper.Values.Insert(0, value);
                    break;
                case OperandKind.To:
                    oper.Values.Insert(1, value);
                    break;
                case OperandKind.List:
                    foreach (var s in (ICollection<string>)value)
                        oper.Values.Add(s.Trim());
                    break;
                case OperandKind.ListOfInt:
                    foreach (var i in (ICollection<int>)value)
                        oper.Values.Add(i);
                    break;
                case OperandKind.ListOfLong:
                    foreach (var i in (ICollection<long>)value)
                        oper.Values.Add(i);
                    break;

                default:
                    throw new RqlException($"Invalid usage. Property: {prop.Name} Operation: {attr.Operation} DataType: {prop.PropertyType.Name} Operand: {attr.Operand} - Target: {target} Value: {value}");

            }


        }


        foreach (var o in parts.Values)
            Add(o);

        return (TBuilder)this;

    }

    /// <summary>The mutable list of predicates accumulated by this builder.</summary>
    protected IList<IRqlPredicate> Predicates { get; }

    /// <inheritdoc />
    public bool HasCriteria => Predicates.Count > 0;

    /// <inheritdoc />
    public IEnumerable<IRqlPredicate> Criteria => Predicates;

    /// <inheritdoc />
    public int RowLimit { get; set; }

    /// <inheritdoc />
    public bool AtLeastOne(Func<IRqlPredicate, bool> predicate)
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count > 0;

    }

    /// <inheritdoc />
    public bool OnlyOne(Func<IRqlPredicate, bool> predicate)
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count == 1;

    }

    /// <inheritdoc />
    public bool None(Func<IRqlPredicate, bool> predicate)
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count == 0;

    }


    /// <inheritdoc />
    public void Add(IRqlPredicate operation)
    {

        Guard.IsNotNull(operation);

        Predicates.Add(operation);

    }

    /// <inheritdoc />
    public void Clear()
    {
        Predicates.Clear();
    }

    #endregion


    /// <summary>The name of the current target field being constrained in the fluent chain.</summary>
    protected string CurrentName { get; set; }



    #region Equals


    /// <summary>Adds an equality predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }


    /// <summary>Adds an equality predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds an equality predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }


    /// <summary>Adds an equality predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds an equality predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds an equality predicate for the current field with a boolean value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Equals(bool value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<bool>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    #endregion


    #region NotEquals

    /// <summary>Adds a not-equal predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-equal predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-equal predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-equal predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-equal predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-equal predicate for the current field with a boolean value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotEquals(bool value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<bool>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    #endregion


    #region LesserThan

    /// <summary>Adds a less-than predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThan(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThan(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThan(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThan(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThan(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region LesserThanOrEqual

    /// <summary>Adds a less-than-or-equal predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThanOrEqual(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than-or-equal predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThanOrEqual(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than-or-equal predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThanOrEqual(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a less-than-or-equal predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThanOrEqual(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.LesserThanOrEqual, CurrentName, value));


        return (TBuilder)this;

    }

    /// <summary>Adds a less-than-or-equal predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder LesserThanOrEqual(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region GreaterThan

    /// <summary>Adds a greater-than predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThan(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThan(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThan(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThan(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThan(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region GreaterThanOrEqual

    /// <summary>Adds a greater-than-or-equal predicate for the current field with a string value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqual(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than-or-equal predicate for the current field with an integer value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqual(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than-or-equal predicate for the current field with a long value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqual(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than-or-equal predicate for the current field with a DateTime value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqual(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a greater-than-or-equal predicate for the current field with a decimal value.</summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqual(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region String operations

    /// <summary>Adds a starts-with predicate for the current field.</summary>
    /// <param name="value">The prefix string to match.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder StartsWith(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.StartsWith, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds a contains predicate for the current field.</summary>
    /// <param name="value">The substring to search for.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Contains(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.Contains, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds an ends-with predicate for the current field.</summary>
    /// <param name="value">The suffix string to match.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder EndsWith(string value)
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.EndsWith, CurrentName, value));

        return (TBuilder)this;

    }

    /// <summary>Adds an is-null predicate for the current field.</summary>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder IsNull()
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate(RqlOperator.IsNull, CurrentName, typeof(object), Array.Empty<object>()));

        return (TBuilder)this;

    }

    /// <summary>Adds an is-not-null predicate for the current field.</summary>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder IsNotNull()
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate(RqlOperator.IsNotNull, CurrentName, typeof(object), Array.Empty<object>()));

        return (TBuilder)this;

    }

    #endregion


    #region Between

    /// <summary>Adds a between (inclusive range) predicate for the current field with integer bounds.</summary>
    /// <param name="from">The lower bound of the range.</param>
    /// <param name="to">The upper bound of the range.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Between(int from, int to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.Between, CurrentName, new[] { from, to }));


        return (TBuilder)this;

    }

    /// <summary>Adds a between (inclusive range) predicate for the current field with long bounds.</summary>
    /// <param name="from">The lower bound of the range.</param>
    /// <param name="to">The upper bound of the range.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Between(long from, long to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    /// <summary>Adds a between (inclusive range) predicate for the current field with DateTime bounds.</summary>
    /// <param name="from">The lower bound of the range.</param>
    /// <param name="to">The upper bound of the range.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Between(DateTime from, DateTime to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    /// <summary>Adds a between (inclusive range) predicate for the current field with decimal bounds.</summary>
    /// <param name="from">The lower bound of the range.</param>
    /// <param name="to">The upper bound of the range.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Between(decimal from, decimal to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    /// <summary>Adds a between (inclusive range) predicate for the current field with string bounds.</summary>
    /// <param name="from">The lower bound of the range.</param>
    /// <param name="to">The upper bound of the range.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder Between(string from, string to)
    {

        Guard.IsNotNull(from);
        Guard.IsNotNull(to);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    #endregion


    #region In

    /// <summary>Adds an in-set predicate for the current field with string values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(params string[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with an enumerable of string values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(IEnumerable<string> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with integer values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(params int[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with an enumerable of integer values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(IEnumerable<int> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with long values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(params long[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with an enumerable of long values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(IEnumerable<long> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with decimal values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(params decimal[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with an enumerable of decimal values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(IEnumerable<decimal> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with DateTime values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(params DateTime[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds an in-set predicate for the current field with an enumerable of DateTime values.</summary>
    /// <param name="values">The set of values to match against.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder In(IEnumerable<DateTime> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    #endregion


    #region NotIn

    /// <summary>Adds a not-in-set predicate for the current field with string values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(params string[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with an enumerable of string values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(IEnumerable<string> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with integer values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(params int[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with an enumerable of integer values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(IEnumerable<int> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with long values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(params long[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with an enumerable of long values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(IEnumerable<long> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with decimal values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(params decimal[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with an enumerable of decimal values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(IEnumerable<decimal> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with DateTime values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(params DateTime[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    /// <summary>Adds a not-in-set predicate for the current field with an enumerable of DateTime values.</summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public TBuilder NotIn(IEnumerable<DateTime> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    #endregion


}
