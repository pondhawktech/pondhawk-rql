using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Pondhawk.Rql.Builder;
using Sprache;

namespace Pondhawk.Rql.Parser;

/// <summary>
/// Parses RQL criteria text into an <see cref="RqlTree"/> AST using the Sprache parser combinator library.
/// Value type prefixes: <c>@</c> for DateTime, <c>#</c> for decimal, <c>'...'</c> for strings.
/// </summary>
/// <remarks>
/// <para>Operator codes: <c>eq</c>, <c>ne</c>, <c>lt</c>, <c>gt</c>, <c>le</c>, <c>ge</c>,
/// <c>sw</c> (starts-with), <c>ew</c> (ends-with), <c>cn</c> (contains),
/// <c>bt</c> (between), <c>in</c>, <c>ni</c> (not-in), <c>nu</c> (is-null), <c>nn</c> (is-not-null).</para>
/// <para>Value type prefixes: <c>@</c> for DateTime (<c>@2024-01-15T00:00:00Z</c>),
/// <c>#</c> for decimal (<c>#99.95</c>), single quotes for strings (<c>'hello'</c>),
/// bare values for int/long/bool.</para>
/// </remarks>
/// <example>
/// <code>
/// // Parse RQL text into an AST
/// var tree = RqlLanguageParser.ToCriteria("(eq(Status,'Active'),gt(Total,#100))");
///
/// // Use the parsed tree with a typed filter builder for serialization
/// var filter = new RqlFilterBuilder&lt;Order&gt;(tree);
/// Func&lt;Order, bool&gt; predicate = filter.ToLambda();
/// </code>
/// </example>
public class RqlLanguageParser
{

    static RqlLanguageParser()
    {


        OperatorMap = new Dictionary<string, RqlOperator>(StringComparer.Ordinal)
        {
            ["eq"] = RqlOperator.Equals,
            ["ne"] = RqlOperator.NotEquals,
            ["lt"] = RqlOperator.LesserThan,
            ["gt"] = RqlOperator.GreaterThan,
            ["le"] = RqlOperator.LesserThanOrEqual,
            ["ge"] = RqlOperator.GreaterThanOrEqual,
            ["sw"] = RqlOperator.StartsWith,
            ["cn"] = RqlOperator.Contains,
            ["bt"] = RqlOperator.Between,
            ["in"] = RqlOperator.In,
            ["ni"] = RqlOperator.NotIn,
            ["ew"] = RqlOperator.EndsWith,
            ["nu"] = RqlOperator.IsNull,
            ["nn"] = RqlOperator.IsNotNull
        };



    }


    private static readonly Dictionary<string, RqlOperator> OperatorMap;



    private static readonly Parser<IEnumerable<char>> Whitespace = Parse.Char(' ').Many();

    private static readonly Parser<char> RestrictionOpener = Parse.Char('(');
    private static readonly Parser<char> RestrictionCloser = Parse.Char(')');
    private static readonly Parser<char> PredicateSeparator = Parse.Char(',');

    private static readonly Parser<char> PredicateOpener = Parse.Char('(');
    private static readonly Parser<char> PredicateCloser = Parse.Char(')');

    private static readonly Parser<char> PredicateTypeTerm = Parse.AnyChar.Except(PredicateOpener);
    private static readonly Parser<string> PredicateType = PredicateTypeTerm.XAtLeastOnce().Text();

    private static readonly Parser<char> PredicateValueSeparator = Parse.Char(',');

    private static readonly Parser<char> PredicateValueTerm = Parse.AnyChar.Except(PredicateValueSeparator).Except(PredicateCloser);
    private static readonly Parser<string> PredicateTargetName = PredicateValueTerm.XAtLeastOnce().Text();
    private static readonly Parser<string> PredicateTargetValue = PredicateValueTerm.XAtLeastOnce().Text();


    private static readonly Parser<IRqlPredicate> Predicate =

        from ws in Whitespace
        from type in PredicateType
        from opener in PredicateOpener
        from target in PredicateTargetName
        from values in PredicateSeparator.Then(_ => PredicateTargetValue).Many()
        from closer in PredicateCloser

        select BuildPredicate(type, target, values);



    private static readonly Parser<IEnumerable<IRqlPredicate>> Restriction =

        from open in RestrictionOpener
        from leading in Predicate.Optional()
        from rest in PredicateSeparator.Then(_ => Predicate).Many()
        from close in RestrictionCloser

        select MergePredicates(leading.GetOrDefault(), rest);


    private static IEnumerable<IRqlPredicate> MergePredicates(IRqlPredicate? head, IEnumerable<IRqlPredicate> rest)
    {

        if (head != null)
            yield return head;

        foreach (var item in rest)
            yield return item;

    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Parser method with linear type-detection logic that does not benefit from splitting")]
    private static RqlPredicate BuildPredicate(string op, string name, IEnumerable<string> values)
    {


        if (string.IsNullOrWhiteSpace(op))
            throw new RqlException($"Empty or whitespace-only operator encountered for target '{name}'");

        Type? dataType = null;

        var raw = new List<string>(values);
        var typed = new List<object>();

        foreach (var v in raw)
        {

            if (v.Length == 0)
            {
                dataType ??= typeof(string);
                typed.Add(v);
                continue;
            }

            var indicator = v[0];
            if (indicator == '@' && v.Length > 1 && DateTime.TryParse(v.AsSpan(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
            {
                dataType ??= typeof(DateTime);
                typed.Add(date);
            }
            else if (indicator == '#' && v.Length > 1 && decimal.TryParse(v.AsSpan(1), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var decm))
            {
                dataType ??= typeof(decimal);
                typed.Add(decm);
            }
            else if (indicator == '\'' && v.Length >= 2 && v[^1] == '\'')
            {
                dataType ??= typeof(string);
                var s = v.Substring(1, v.Length - 2).Replace("''", "'");
                typed.Add(s);
            }
            else if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iv))
            {
                dataType ??= typeof(int);
                typed.Add(iv);
            }
            else if (long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lv))
            {
                dataType ??= typeof(long);
                typed.Add(lv);
            }
            else if (decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var dv))
            {
                dataType ??= typeof(decimal);
                typed.Add(dv);
            }
            else if (bool.TryParse(v, out var bv))
            {
                dataType ??= typeof(bool);
                typed.Add(bv);
            }
            else
            {
                dataType ??= typeof(string);
                typed.Add(v);
            }


        }

        if (!OperatorMap.TryGetValue(op, out var opr))
            throw new RqlException($"Invalid RQL operator: ({op})");

        if (opr is RqlOperator.Between && typed.Count != 2)
            throw new RqlException($"Between operator on '{name}' requires exactly 2 values but found {typed.Count}");

        if (opr is RqlOperator.IsNull or RqlOperator.IsNotNull)
            dataType = typeof(object);
        else
            dataType ??= typeof(string);

        var predicate = new RqlPredicate(opr, name, dataType!, typed);

        return predicate;


    }



    /// <summary>
    /// Parses the given RQL criteria string into an <see cref="RqlTree"/>.
    /// </summary>
    /// <param name="input">The RQL criteria text, e.g. <c>(eq(Name,'John'),gt(Age,30))</c>.</param>
    /// <exception cref="RqlException">Thrown when the input cannot be parsed.</exception>
    public static RqlTree ToCriteria(string input)
    {

        try
        {

            var ops = Restriction.Parse(input);

            var expr = new RqlTree();
            foreach (var op in ops)
                expr.Criteria.Add(op);

            return expr;

        }
        catch (ParseException cause)
        {
            throw new RqlException($"Could not parse supplied RQL '{input}'. {cause.Message}", cause);
        }

    }


}
