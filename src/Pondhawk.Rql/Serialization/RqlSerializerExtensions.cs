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

using System.Globalization;
using System.Text;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql.Serialization
{


    /// <summary>
    /// Extension methods for serializing RQL filters to RQL text format, e.g. <c>(eq(Name,'John'),gt(Age,30))</c>.
    /// </summary>
    /// <remarks>
    /// <para>Supported data types: string, bool, DateTime, decimal, short, int, long.</para>
    /// <para>Strings are single-quoted with <c>'</c> escaping (<c>''</c>). DateTime values are prefixed with <c>@</c>.
    /// Decimal values are prefixed with <c>#</c>. Boolean values are lower-cased.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var filter = RqlFilterBuilder&lt;Person&gt;
    ///     .Where(p =&gt; p.Name).Equals("O'Brien")
    ///     .And(p =&gt; p.Age).GreaterThan(21);
    ///
    /// string rql = filter.ToRql();
    /// // Result: (eq(Name,'O''Brien'),gt(Age,21))
    /// </code>
    /// </example>
    public static class RqlSerializerExtensions
    {


        #region implementation

        static RqlSerializerExtensions()
        {

            // ***************************************************************************
            var kindMap = new Dictionary<RqlOperator, KindSpec>
            {
                [RqlOperator.Equals] = new() { Operation = "eq", MultiValue = false },
                [RqlOperator.NotEquals] = new() { Operation = "ne", MultiValue = false },
                [RqlOperator.Contains] = new() { Operation = "cn", MultiValue = false },
                [RqlOperator.StartsWith] = new() { Operation = "sw", MultiValue = false },
                [RqlOperator.LesserThan] = new() { Operation = "lt", MultiValue = false },
                [RqlOperator.GreaterThan] = new() { Operation = "gt", MultiValue = false },
                [RqlOperator.LesserThanOrEqual] = new() { Operation = "le", MultiValue = false },
                [RqlOperator.GreaterThanOrEqual] = new() { Operation = "ge", MultiValue = false },
                [RqlOperator.Between] = new() { Operation = "bt", MultiValue = true },
                [RqlOperator.In] = new() { Operation = "in", MultiValue = true },
                [RqlOperator.NotIn] = new() { Operation = "ni", MultiValue = true },
                [RqlOperator.EndsWith] = new() { Operation = "ew", MultiValue = false },
                [RqlOperator.IsNull] = new() { Operation = "nu", MultiValue = false, NoValue = true },
                [RqlOperator.IsNotNull] = new() { Operation = "nn", MultiValue = false, NoValue = true }
            };


            KindMap = kindMap;



            // ***************************************************************************
            var typeMap = new Dictionary<Type, TypeSpec>();

            string DefaultFormatter(object o) => o.ToString() ?? string.Empty;
            string LowerCaseFormatter(object o) => o.ToString()?.ToLowerInvariant() ?? string.Empty;

            typeMap[typeof(string)] = new TypeSpec { NeedsQuotes = true, Prefix = "", Formatter = DefaultFormatter };
            typeMap[typeof(bool)] = new TypeSpec { NeedsQuotes = false, Prefix = "", Formatter = LowerCaseFormatter };
            typeMap[typeof(DateTime)] = new TypeSpec { NeedsQuotes = false, Prefix = "@", Formatter = _dateTimeFormatter };
            typeMap[typeof(decimal)] = new TypeSpec { NeedsQuotes = false, Prefix = "#", Formatter = DefaultFormatter };
            typeMap[typeof(short)] = new TypeSpec { NeedsQuotes = false, Prefix = "", Formatter = DefaultFormatter };
            typeMap[typeof(int)] = new TypeSpec { NeedsQuotes = false, Prefix = "", Formatter = DefaultFormatter };
            typeMap[typeof(long)] = new TypeSpec { NeedsQuotes = false, Prefix = "", Formatter = DefaultFormatter };

            TypeMap = typeMap;


        }

        private static string _dateTimeFormatter(object source)
        {

            Guard.IsNotNull(source);

            if (source is not DateTime time)
                throw new InvalidOperationException($"Object of type: {source.GetType().FullName} can not be cast to a DateTime");

            var dtStr = time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            return dtStr;

        }


        private static Dictionary<RqlOperator, KindSpec> KindMap { get; }
        private static Dictionary<Type, TypeSpec> TypeMap { get; }


        private struct KindSpec
        {
            public string Operation;
            public bool MultiValue;
            public bool NoValue;
        }


        private struct TypeSpec
        {
            public bool NeedsQuotes;
            public string Prefix;
            public Func<object, string> Formatter;
        }


        private static List<string> BuildRestrictionParts(IEnumerable<IRqlPredicate> meta)
        {

            var parts = new List<string>();

            foreach (var op in meta)
            {

                if (!(KindMap.TryGetValue(op.Operator, out var kindSpec)))
                    throw new RqlException($"{op.Operator} is not a supported operation");

                if (kindSpec.NoValue)
                {
                    parts.Add($"{kindSpec.Operation}({op.Target.Name})");
                    continue;
                }

                if (!(TypeMap.TryGetValue(op.DataType, out var typeSpec)))
                    throw new RqlException($"{op.DataType.Name} is not a supported data type");


                if (!(kindSpec.MultiValue))
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (typeSpec.NeedsQuotes)
                        parts.Add(
                            $"{kindSpec.Operation}({op.Target.Name},{typeSpec.Prefix}'{EscapeQuotes(typeSpec.Formatter(op.Values[0]))}')");
                    else
                        parts.Add(
                            $"{kindSpec.Operation}({op.Target.Name},{typeSpec.Prefix}{typeSpec.Formatter(op.Values[0])})");
                }
                else
                {

                    var values = new List<string>();

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (typeSpec.NeedsQuotes)
                        values.AddRange(op.Values.Select(v => $"{typeSpec.Prefix}'{EscapeQuotes(typeSpec.Formatter(v))}'"));
                    else
                        values.AddRange(op.Values.Select(v => $"{typeSpec.Prefix}{typeSpec.Formatter(v)}"));

                    parts.Add($"{kindSpec.Operation}({op.Target.Name},{String.Join(",", values)})");

                }


            }


            return parts;

        }


        private static string EscapeQuotes(string value) => value.Replace("'", "''");

        #endregion



        /// <summary>
        /// Serializes the filter predicates to RQL text format, e.g. <c>(eq(Name,'John'),gt(Age,30))</c>.
        /// </summary>
        /// <param name="builder">The filter to serialize.</param>
        /// <returns>The RQL text representation of the filter.</returns>
        public static string ToRql(this IRqlFilter builder)
        {

            var parts = BuildRestrictionParts(builder.Criteria);

            var sb = new StringBuilder();
            sb.Append('(');
            sb.Append(string.Join(",", parts));
            sb.Append(')');

            return sb.ToString();

        }


    }


}
