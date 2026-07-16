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
using System.Globalization;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql.Serialization;

/// <summary>
/// Extension methods for serializing RQL filters to parameterized SQL queries.
/// </summary>
/// <remarks>
/// All values are parameterized (never inlined) to prevent SQL injection.
/// Use <c>ToSqlWhere()</c> for just the WHERE clause, or <c>ToSqlQuery()</c> for a complete SELECT statement.
/// </remarks>
/// <example>
/// <code>
/// var filter = RqlFilterBuilder&lt;Order&gt;
///     .Where(o =&gt; o.Status).Equals("Active")
///     .And(o =&gt; o.Total).Between(100m, 500m);
///
/// // WHERE clause only
/// var (where, parms) = filter.ToSqlWhere();
/// // where: "Status = {0} and Total between {1} and {2}"
/// // parms: ["Active", 100m, 500m]
///
/// // Full SELECT query (table name inferred from type)
/// var (sql, sqlParms) = filter.ToSqlQuery();
/// // sql: "select * from Order where Status = {0} and Total between {1} and {2}"
/// </code>
/// </example>
public static class SqlSerializerExtensions
{


    static SqlSerializerExtensions()
    {

        // ***************************************************************************
        object DefaultKindFormatter(object o) => o;

        OperatorMap = new Dictionary<RqlOperator, KindSpec>
        {
            [RqlOperator.Equals] = new() { Operation = "{0} = {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.NotEquals] = new() { Operation = "{0} <> {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.Contains] = new() { Operation = "{0} like {1}", Style = ValueStyle.Single, Formatter = _containsFormatter },
            [RqlOperator.StartsWith] = new() { Operation = "{0} like {1}", Style = ValueStyle.Single, Formatter = _startsWithFormatter },
            [RqlOperator.LesserThan] = new() { Operation = "{0} < {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.GreaterThan] = new() { Operation = "{0} > {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.LesserThanOrEqual] = new() { Operation = "{0} <= {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.GreaterThanOrEqual] = new() { Operation = "{0} >= {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter },
            [RqlOperator.Between] = new() { Operation = "{0} between {1} and {2}", Style = ValueStyle.Pair, Formatter = DefaultKindFormatter },
            [RqlOperator.In] = new() { Operation = "{0} in ({1})", Style = ValueStyle.Enumeration, Formatter = DefaultKindFormatter },
            [RqlOperator.NotIn] = new() { Operation = "{0} not in ({1})", Style = ValueStyle.Enumeration, Formatter = DefaultKindFormatter },
            [RqlOperator.EndsWith] = new() { Operation = "{0} like {1}", Style = ValueStyle.Single, Formatter = _endsWithFormatter },
            [RqlOperator.IsNull] = new() { Operation = "{0} is null", Style = ValueStyle.NoValue, Formatter = DefaultKindFormatter },
            [RqlOperator.IsNotNull] = new() { Operation = "{0} is not null", Style = ValueStyle.NoValue, Formatter = DefaultKindFormatter }
        };


        // ***************************************************************************
        string DefaultFormatter(object o) => o.ToString() ?? "";

        TypeMap = new Dictionary<Type, TypeSpec>
        {
            [typeof(string)] = new() { NeedsQuotes = true, Formatter = DefaultFormatter },
            [typeof(bool)] = new() { NeedsQuotes = false, Formatter = DefaultFormatter },
            [typeof(DateTime)] = new() { NeedsQuotes = true, Formatter = _dateTimeFormatter },
            [typeof(decimal)] = new() { NeedsQuotes = false, Formatter = DefaultFormatter },
            [typeof(short)] = new() { NeedsQuotes = false, Formatter = DefaultFormatter },
            [typeof(int)] = new() { NeedsQuotes = false, Formatter = DefaultFormatter },
            [typeof(long)] = new() { NeedsQuotes = false, Formatter = DefaultFormatter }
        };


    }

    private static string _dateTimeFormatter(object source)
    {

        Guard.IsNotNull(source);

        if (source is not DateTime time)
            throw new InvalidOperationException($"Object of type: {source.GetType().FullName} can not be cast to a DateTime");

        var dtStr = time.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        return dtStr;

    }

    private static string _containsFormatter(object value)
    {

        Guard.IsNotNull(value);

        return $"%{value}%";

    }

    private static string _startsWithFormatter(object value)
    {

        Guard.IsNotNull(value);

        return $"{value}%";

    }

    private static string _endsWithFormatter(object value)
    {

        Guard.IsNotNull(value);

        return $"%{value}";

    }


    private static Dictionary<RqlOperator, KindSpec> OperatorMap { get; }
    private static Dictionary<Type, TypeSpec> TypeMap { get; }


    private enum ValueStyle
    {
        Single,
        Pair,
        Enumeration,
        NoValue
    };

    private struct KindSpec
    {
        public string Operation;
        public ValueStyle Style;
        public Func<object, object> Formatter;
    }


    private struct TypeSpec
    {
        public bool NeedsQuotes;
        public Func<object, string> Formatter;
    }



    /// <summary>
    /// Serializes the filter to a complete <c>SELECT * FROM [entity]</c> SQL query with parameterized values, using the entity type name as the table name.
    /// </summary>
    /// <typeparam name="TEntity">The entity type whose name is used as the table name.</typeparam>
    /// <param name="builder">The filter builder to serialize.</param>
    /// <returns>A tuple of the SQL query string and its parameter values.</returns>
    public static (string sql, object[] parameters) ToSqlQuery<TEntity>(this RqlFilterBuilder<TEntity> builder) where TEntity : class
    {

        var tableName = typeof(TEntity).Name;

        var result = ToSqlQuery(builder, tableName);

        return result;

    }

    /// <summary>
    /// Serializes the filter to a complete <c>SELECT * FROM [tableName]</c> SQL query with parameterized values.
    /// </summary>
    /// <param name="builder">The filter to serialize.</param>
    /// <param name="tableName">The table name to use in the query.</param>
    /// <param name="indexed">When <c>true</c>, parameters use indexed placeholders (<c>{0}</c>, <c>{1}</c>); when <c>false</c>, uses <c>?</c> placeholders.</param>
    /// <returns>A tuple of the SQL query string and its parameter values.</returns>
    public static (string sql, object[] parameters) ToSqlQuery(this IRqlFilter builder, string tableName, bool indexed = true)
    {

        Guard.IsNotNull(tableName);

        var pair = builder.ToSqlWhere(indexed);

        var hasWhere = !string.IsNullOrWhiteSpace(pair.sql);
        var hasLimit = builder.RowLimit > 0;

        var query = $"select * from {tableName}";

        if (hasWhere)
            query += $" where {pair.sql}";

        if (hasLimit)
            query += $" limit {builder.RowLimit}";

        return (query, hasWhere ? pair.parameters : []);

    }


    /// <summary>
    /// Serializes the filter predicates to a parameterized SQL WHERE clause.
    /// </summary>
    /// <param name="builder">The filter to serialize.</param>
    /// <param name="indexed">When <c>true</c>, parameters use indexed placeholders (<c>{0}</c>, <c>{1}</c>); when <c>false</c>, uses <c>?</c> placeholders.</param>
    /// <returns>A tuple of the SQL WHERE clause (without the <c>WHERE</c> keyword) and its parameter values.</returns>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "SQL serialization method with linear per-operator formatting that does not benefit from splitting")]
    public static (string sql, object[] parameters) ToSqlWhere(this IRqlFilter builder, bool indexed = true)
    {

        string Build(int index)
        {
            return indexed ? $"{{{index}}}" : "?";
        }

        var parameters = new List<object>();
        var parts = new List<string>();

        foreach (var op in builder.Criteria)
        {

            if (!OperatorMap.TryGetValue(op.Operator, out var kindSpec))
                throw new RqlException($"{op.Operator} is not a supported operation");


            if (kindSpec.Style == ValueStyle.NoValue)
            {
                parts.Add(string.Format(CultureInfo.InvariantCulture, kindSpec.Operation, op.Target));
                continue;
            }

            if (!TypeMap.TryGetValue(op.DataType, out var typeSpec))
                throw new RqlException($"{op.DataType.Name} is not a supported data type");


            if (kindSpec.Style == ValueStyle.Single)
            {

                var value = kindSpec.Formatter(op.Values[0]);

                var actValue = value;
                if (value is IConvertible convertible)
                    actValue = ConvertValue(convertible, op.DataType, op.Target.Name);

                parameters.Add(actValue);

                parts.Add(string.Format(CultureInfo.InvariantCulture, kindSpec.Operation, op.Target, Build(parameters.Count - 1)));

            }
            else if (kindSpec.Style == ValueStyle.Pair)
            {

                if (op.Values.Count < 2)
                    throw new RqlException($"Between operator on '{op.Target}' requires exactly 2 values but found {op.Values.Count}");

                var value1 = kindSpec.Formatter(op.Values[0]);
                var actValue1 = value1;
                if (value1 is IConvertible convertible1)
                    actValue1 = ConvertValue(convertible1, op.DataType, op.Target.Name);

                parameters.Add(actValue1);

                var value2 = kindSpec.Formatter(op.Values[1]);
                var actValue2 = value2;
                if (value2 is IConvertible convertible2)
                    actValue2 = ConvertValue(convertible2, op.DataType, op.Target.Name);

                parameters.Add(actValue2);

                parts.Add(string.Format(CultureInfo.InvariantCulture, kindSpec.Operation, op.Target, Build(parameters.Count - 2), Build(parameters.Count - 1)));

            }
            else
            {

                var placeholders = new List<string>();

                foreach (var v in op.Values)
                {
                    var value = kindSpec.Formatter(v);
                    var actValue = value;
                    if (value is IConvertible convertible)
                        actValue = ConvertValue(convertible, op.DataType, op.Target.Name);

                    parameters.Add(actValue);
                    placeholders.Add(Build(parameters.Count - 1));
                }

                parts.Add(string.Format(CultureInfo.InvariantCulture, kindSpec.Operation, op.Target, string.Join(",", placeholders)));


            }


        }


        if (parts.Count == 0)
            return ("", Array.Empty<object>());


        var join = string.Join(" and ", parts);

        return (join, parameters.ToArray());

    }

    private static object ConvertValue(IConvertible convertible, Type targetType, string fieldName)
    {
        try
        {
            return convertible.ToType(targetType, CultureInfo.CurrentCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            throw new RqlException($"Cannot convert value '{convertible}' to {targetType.Name} for field '{fieldName}'", ex);
        }
    }

}
