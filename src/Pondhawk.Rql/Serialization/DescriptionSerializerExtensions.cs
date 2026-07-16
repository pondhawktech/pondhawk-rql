/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

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

using System.Globalization;
using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql.Serialization;

/// <summary>
/// Extension methods for serializing RQL filters to human-readable English descriptions.
/// </summary>
/// <remarks>
/// <para>Produces descriptions like "Name equals 'Widget' and Price is greater than 99".</para>
/// <para>All operators and data types supported by the RQL builder are handled.</para>
/// </remarks>
/// <example>
/// <code>
/// var filter = RqlFilterBuilder&lt;Product&gt;
///     .Where(p =&gt; p.Category).Equals("Electronics")
///     .And(p =&gt; p.Price).GreaterThan(99)
///     .And(p =&gt; p.Stock).Between(10, 100);
///
/// string description = filter.ToDescription();
/// // Result: "Category equals 'Electronics' and Price is greater than 99 and Stock is between 10 and 100"
/// </code>
/// </example>
public static class DescriptionSerializerExtensions
{
    private static readonly Dictionary<RqlOperator, OperatorPhrase> OperatorPhrases = new()
    {
        [RqlOperator.Equals] = new("equals", ValueStyle.Single),
        [RqlOperator.NotEquals] = new("does not equal", ValueStyle.Single),
        [RqlOperator.LesserThan] = new("is less than", ValueStyle.Single),
        [RqlOperator.GreaterThan] = new("is greater than", ValueStyle.Single),
        [RqlOperator.LesserThanOrEqual] = new("is less than or equal to", ValueStyle.Single),
        [RqlOperator.GreaterThanOrEqual] = new("is greater than or equal to", ValueStyle.Single),
        [RqlOperator.StartsWith] = new("starts with", ValueStyle.Single),
        [RqlOperator.Contains] = new("contains", ValueStyle.Single),
        [RqlOperator.EndsWith] = new("ends with", ValueStyle.Single),
        [RqlOperator.Between] = new("is between", ValueStyle.Pair),
        [RqlOperator.In] = new("is in", ValueStyle.Enumeration),
        [RqlOperator.NotIn] = new("is not in", ValueStyle.Enumeration),
        [RqlOperator.IsNull] = new("is null", ValueStyle.NoValue),
        [RqlOperator.IsNotNull] = new("is not null", ValueStyle.NoValue)
    };

    /// <summary>
    /// Serializes the filter predicates to a human-readable English description.
    /// </summary>
    /// <param name="builder">The filter to describe.</param>
    /// <returns>An English description of the filter, e.g. "Name equals 'Widget' and Price is greater than 99".</returns>
    public static string ToDescription(this IRqlFilter builder)
    {
        var parts = new List<string>();

        foreach (var predicate in builder.Criteria)
        {
            if (!OperatorPhrases.TryGetValue(predicate.Operator, out var phrase))
                throw new RqlException($"{predicate.Operator} is not a supported operation");

            parts.Add(FormatPredicate(predicate, phrase));
        }

        return parts.Count == 0
            ? string.Empty
            : string.Join(" and ", parts);
    }

    private static string FormatPredicate(IRqlPredicate predicate, OperatorPhrase phrase)
    {
        var field = predicate.Target.Name;

        return phrase.Style switch
        {
            ValueStyle.NoValue => $"{field} {phrase.Text}",
            ValueStyle.Single => $"{field} {phrase.Text} {FormatValue(predicate.Values[0], predicate.DataType)}",
            ValueStyle.Pair => $"{field} {phrase.Text} {FormatValue(predicate.Values[0], predicate.DataType)} and {FormatValue(predicate.Values[1], predicate.DataType)}",
            ValueStyle.Enumeration => $"{field} {phrase.Text} ({string.Join(", ", predicate.Values.Select(v => FormatValue(v, predicate.DataType)))})",
            _ => throw new RqlException($"Unsupported value style: {phrase.Style}")
        };
    }

    private static string FormatValue(object value, Type dataType)
    {
        if (dataType == typeof(string))
            return $"'{value}'";

        if (dataType == typeof(DateTime) && value is DateTime dt)
            return dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        if (dataType == typeof(bool))
            return value.ToString()?.ToLowerInvariant() ?? "false";

        return value.ToString() ?? string.Empty;
    }

    private enum ValueStyle
    {
        Single,
        Pair,
        Enumeration,
        NoValue
    }

    private readonly struct OperatorPhrase(string text, ValueStyle style)
    {
        public string Text { get; } = text;
        public ValueStyle Style { get; } = style;
    }
}
