// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Parser;
using Pondhawk.Rql.Serialization;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public enum TestState
{
    Open,
    Closed,
}

/// <summary>A value object a caller converts operands to with its own converter.</summary>
public readonly record struct TestCode(string Value);

public class TestRecord
{
    public string Title { get; set; } = string.Empty;
    public int? Count { get; set; }
    public DateTimeOffset At { get; set; }
    public DateTimeOffset? Due { get; set; }
    public DateOnly Day { get; set; }
    public Guid Key { get; set; }
    public TestState State { get; set; }
    public TestCode Code { get; set; }
    public TestCode? MaybeCode { get; set; }
    public double Score { get; set; }
}

public class ExpressionOptionsTests
{

    private static readonly TestRecord[] Records =
    [
        new() { Title = "Printer jams", Count = 3, At = new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero), Due = new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero),
                Day = new DateOnly(2026, 11, 10), Key = Guid.Parse("8f2b5a3e-7c1d-4e2a-9b6f-0a1b2c3d4e5f"), State = TestState.Open, Code = new TestCode("A"), MaybeCode = new TestCode("A"), Score = 1.5 },
        new() { Title = "VPN drops", Count = null, At = new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero), Due = null,
                Day = new DateOnly(2026, 11, 12), Key = Guid.Parse("1a2b3c4d-5e6f-4a1b-8c9d-0e1f2a3b4c5d"), State = TestState.Closed, Code = new TestCode("B"), MaybeCode = null, Score = 2.5 },
    ];

    private static RqlFilterBuilder<TestRecord> Filter(params IRqlPredicate[] predicates)
    {
        var filter = RqlFilterBuilder<TestRecord>.Create();
        foreach (var predicate in predicates)
            filter.Add(predicate);
        return filter;
    }

    private static List<string> Titles(RqlFilterBuilder<TestRecord> filter, RqlExpressionOptions options) =>
        [.. Records.Where(filter.ToLambda(options)).Select(record => record.Title)];

    private static bool Mentions(Expression expression, Func<Expression, bool> test)
    {
        var finder = new Finder(test);
        finder.Visit(expression);
        return finder.Found;
    }

    private sealed class Finder(Func<Expression, bool> test) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        public override Expression Visit(Expression node)
        {
            if (node is not null && test(node))
                Found = true;
            return base.Visit(node);
        }
    }

    // ========== The built-in conversion ==========

    [Fact]
    public void A_nullable_property_takes_its_underlying_type()
    {
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Count", typeof(int), 3)), RqlExpressionOptions.Default).ShouldBe(["Printer jams"]);
        Titles(Filter(new RqlPredicate(RqlOperator.GreaterThan, "Count", typeof(long), 1L)), RqlExpressionOptions.Default).ShouldBe(["Printer jams"]);
    }

    [Fact]
    public void A_DateTimeOffset_property_takes_a_DateTime_or_a_DateTimeOffset()
    {
        var since = new DateTime(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc);
        Titles(RqlFilterBuilder<TestRecord>.Where(r => r.At).GreaterThanOrEqual(since), RqlExpressionOptions.Default).ShouldBe(["VPN drops"]);

        var due = new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Due", typeof(DateTimeOffset), due)), RqlExpressionOptions.Default).ShouldBe(["Printer jams"]);
    }

    [Fact]
    public void An_unspecified_DateTime_is_taken_as_UTC()
    {
        var at = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Unspecified);

        RqlValueConverter.Convert(at, typeof(DateTimeOffset)).ShouldBe(new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void DateOnly_Guid_enum_and_double_properties_take_what_the_parser_reads()
    {
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Day", typeof(DateTime), new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Utc))), RqlExpressionOptions.Default).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Key", typeof(string), "8f2b5a3e-7c1d-4e2a-9b6f-0a1b2c3d4e5f")), RqlExpressionOptions.Default).ShouldBe(["Printer jams"]);
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "State", typeof(string), "closed")), RqlExpressionOptions.Default).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.GreaterThan, "Score", typeof(decimal), 2m)), RqlExpressionOptions.Default).ShouldBe(["VPN drops"]);
    }

    [Fact]
    public void What_Convert_ChangeType_converted_converts_the_same()
    {
        RqlValueConverter.Convert(5, typeof(long)).ShouldBe(5L);
        RqlValueConverter.Convert("12", typeof(int)).ShouldBe(12);
        RqlValueConverter.Convert(3, typeof(decimal)).ShouldBe(3m);
        Should.Throw<FormatException>(() => RqlValueConverter.Convert("twelve", typeof(int)));
    }

    // ========== A caller's converter ==========

    [Fact]
    public void A_caller_converts_to_its_own_types_and_falls_back_to_the_built_in_conversion()
    {
        var options = new RqlExpressionOptions
        {
            ValueConverter = (value, type) => type == typeof(TestCode) || type == typeof(TestCode?) ? new TestCode((string)value) : RqlValueConverter.Convert(value, type),
        };

        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Code", typeof(string), "B")), options).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "MaybeCode", typeof(string), "A")), options).ShouldBe(["Printer jams"]);
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "Count", typeof(int), 3)), options).ShouldBe(["Printer jams"], "the fallback converts the rest");
    }

    // ========== For a query provider ==========

    [Fact]
    public void A_query_provider_gets_upper_case_comparisons_with_no_StringComparison()
    {
        var filter = Filter(new RqlPredicate(RqlOperator.Contains, "Title", typeof(string), "PRINT"));
        var options = new RqlExpressionOptions { CaseInsensitive = true, QueryProvider = true };

        var expression = filter.ToExpression(options);

        Mentions(expression, node => node is MethodCallExpression { Method.Name: "ToUpper" }).ShouldBeTrue();
        Mentions(expression, node => node.Type == typeof(StringComparison)).ShouldBeFalse();
        Titles(filter, options).ShouldBe(["Printer jams"]);
        Titles(Filter(new RqlPredicate(RqlOperator.StartsWith, "Title", typeof(string), "vpn")), options).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.EndsWith, "Title", typeof(string), "JAMS")), options).ShouldBe(["Printer jams"]);
    }

    [Fact]
    public void A_query_provider_gets_in_as_equalities_typed_to_the_property()
    {
        var options = new RqlExpressionOptions { QueryProvider = true };
        var filter = Filter(new RqlPredicate(RqlOperator.In, "State", typeof(string), ["closed", "open"]));

        var expression = filter.ToExpression(options);

        Mentions(expression, node => node.Type == typeof(List<object>)).ShouldBeFalse();
        Mentions(expression, node => node is BinaryExpression { NodeType: ExpressionType.Equal, Left.Type: var type } && type == typeof(TestState)).ShouldBeTrue();
        Titles(filter, options).ShouldBe(["Printer jams", "VPN drops"]);
        Should.Throw<ArgumentException>(() => Filter(new RqlPredicate(RqlOperator.In, "State", typeof(string), ["Archived"])).ToExpression(options),
            "an enum member that does not exist is refused when the expression is built, as Enum.Parse refuses it");

        Titles(Filter(new RqlPredicate(RqlOperator.In, "State", typeof(string), ["closed"])), options).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.NotIn, "Count", typeof(int), [3])), options).ShouldBe(["VPN drops"]);
        Titles(Filter(new RqlPredicate(RqlOperator.In, "Title", typeof(string), Array.Empty<object>())), options).ShouldBeEmpty("no values match nothing");
    }

    [Fact]
    public void The_bool_overload_builds_what_it_built_before()
    {
        var filter = Filter(new RqlPredicate(RqlOperator.Contains, "Title", typeof(string), "print"));

        var expression = filter.ToExpression(insensitive: true);

        Mentions(expression, node => node.Type == typeof(StringComparison)).ShouldBeTrue("the in-memory form keeps its StringComparison overload");
        Mentions(Filter(new RqlPredicate(RqlOperator.In, "Title", typeof(string), ["VPN drops"])).ToExpression(), node => node.Type == typeof(List<object>)).ShouldBeTrue();
    }

    // ========== Names ==========

    [Fact]
    public void A_field_is_found_whatever_its_case()
    {
        Titles(Filter(new RqlPredicate(RqlOperator.Equals, "title", typeof(string), "VPN drops")), RqlExpressionOptions.Default).ShouldBe(["VPN drops"]);
    }

    // ========== RQL text and descriptions ==========

    [Fact]
    public void New_value_types_are_written_as_RQL_and_read_back_into_the_property_type()
    {
        var due = new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);
        var filter = Filter(
            new RqlPredicate(RqlOperator.Equals, "Due", typeof(DateTimeOffset), due),
            new RqlPredicate(RqlOperator.Equals, "Day", typeof(DateOnly), new DateOnly(2026, 11, 10)),
            new RqlPredicate(RqlOperator.Equals, "Key", typeof(Guid), Guid.Parse("8f2b5a3e-7c1d-4e2a-9b6f-0a1b2c3d4e5f")),
            new RqlPredicate(RqlOperator.LesserThan, "Score", typeof(double), 2.0));

        var rql = filter.ToRql();

        rql.ShouldBe("(eq(Due,@2026-11-11T09:00:00Z),eq(Day,@2026-11-10),eq(Key,'8f2b5a3e-7c1d-4e2a-9b6f-0a1b2c3d4e5f'),lt(Score,#2))");
        var parsed = RqlFilterBuilder<TestRecord>.Create();
        foreach (var predicate in RqlLanguageParser.ToCriteria(rql).Criteria)
            parsed.Add(predicate);
        Titles(parsed, RqlExpressionOptions.Default).ShouldBe(["Printer jams"]);
        filter.ToDescription().ShouldContain("2026-11-11 09:00:00");
    }
}
