using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Parser;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class RqlLanguageParserTests
{

    // ========== Basic operator parsing ==========

    [Fact]
    public void ToCriteria_ParsesEquals_String()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Name,'John'))");

        tree.HasCriteria.ShouldBeTrue();
        tree.Criteria.Count.ShouldBe(1);

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("John");
    }

    [Fact]
    public void ToCriteria_ParsesEquals_Int()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Quantity,42))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Quantity");
        pred.DataType.ShouldBe(typeof(int));
        pred.Values[0].ShouldBe(42);
    }

    [Fact]
    public void ToCriteria_ParsesEquals_Long()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Sku,9876543210))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(long));
        pred.Values[0].ShouldBe(9876543210L);
    }

    [Fact]
    public void ToCriteria_ParsesEquals_Decimal()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Price,#19.99))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values[0].ShouldBe(19.99m);
    }

    [Fact]
    public void ToCriteria_ParsesEquals_DateTime()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Created,@2024-01-15T00:00:00Z))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(DateTime));

        var dt = (DateTime)pred.Values[0];
        dt.Year.ShouldBe(2024);
        dt.Month.ShouldBe(1);
        dt.Day.ShouldBe(15);
    }

    [Fact]
    public void ToCriteria_ParsesEquals_Bool()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(IsActive,true))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(bool));
        pred.Values[0].ShouldBe(true);
    }


    // ========== All operator codes ==========

    [Fact]
    public void ToCriteria_ParsesNotEquals()
    {
        var tree = RqlLanguageParser.ToCriteria("(ne(Name,'John'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.NotEquals);
    }

    [Fact]
    public void ToCriteria_ParsesLesserThan()
    {
        var tree = RqlLanguageParser.ToCriteria("(lt(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.LesserThan);
    }

    [Fact]
    public void ToCriteria_ParsesGreaterThan()
    {
        var tree = RqlLanguageParser.ToCriteria("(gt(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.GreaterThan);
    }

    [Fact]
    public void ToCriteria_ParsesLesserThanOrEqual()
    {
        var tree = RqlLanguageParser.ToCriteria("(le(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
    }

    [Fact]
    public void ToCriteria_ParsesGreaterThanOrEqual()
    {
        var tree = RqlLanguageParser.ToCriteria("(ge(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
    }

    [Fact]
    public void ToCriteria_ParsesStartsWith()
    {
        var tree = RqlLanguageParser.ToCriteria("(sw(Name,'Wid'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.StartsWith);
        tree.Criteria[0].Values[0].ShouldBe("Wid");
    }

    [Fact]
    public void ToCriteria_ParsesContains()
    {
        var tree = RqlLanguageParser.ToCriteria("(cn(Name,'idg'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.Contains);
        tree.Criteria[0].Values[0].ShouldBe("idg");
    }


    [Fact]
    public void ToCriteria_ParsesEndsWith()
    {
        var tree = RqlLanguageParser.ToCriteria("(ew(Name,'son'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.EndsWith);
        tree.Criteria[0].Values[0].ShouldBe("son");
    }

    [Fact]
    public void ToCriteria_ParsesIsNull()
    {
        var tree = RqlLanguageParser.ToCriteria("(nu(Name))");

        tree.Criteria.Count.ShouldBe(1);
        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.IsNull);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(object));
        pred.Values.Count.ShouldBe(0);
    }

    [Fact]
    public void ToCriteria_ParsesIsNotNull()
    {
        var tree = RqlLanguageParser.ToCriteria("(nn(Name))");

        tree.Criteria.Count.ShouldBe(1);
        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.IsNotNull);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(object));
        pred.Values.Count.ShouldBe(0);
    }


    // ========== Multi-value operations ==========

    [Fact]
    public void ToCriteria_ParsesBetween_Decimal()
    {
        var tree = RqlLanguageParser.ToCriteria("(bt(Price,#10.00,#50.00))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe(10.00m);
        pred.Values[1].ShouldBe(50.00m);
    }

    [Fact]
    public void ToCriteria_ParsesIn_Strings()
    {
        var tree = RqlLanguageParser.ToCriteria("(in(Status,'Active','Pending'))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe("Active");
        pred.Values[1].ShouldBe("Pending");
    }

    [Fact]
    public void ToCriteria_ParsesNotIn_Ints()
    {
        var tree = RqlLanguageParser.ToCriteria("(ni(Quantity,1,2,3))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }


    // ========== Multiple predicates ==========

    [Fact]
    public void ToCriteria_ParsesMultiplePredicates()
    {
        var tree = RqlLanguageParser.ToCriteria("(eq(Name,'Widget'),gt(Quantity,10))");

        tree.Criteria.Count.ShouldBe(2);
        tree.Criteria[0].Operator.ShouldBe(RqlOperator.Equals);
        tree.Criteria[0].Target.Name.ShouldBe("Name");
        tree.Criteria[1].Operator.ShouldBe(RqlOperator.GreaterThan);
        tree.Criteria[1].Target.Name.ShouldBe("Quantity");
    }


    // ========== Edge cases ==========

    [Fact]
    public void ToCriteria_EmptyCriteria_ReturnsEmptyTree()
    {
        var tree = RqlLanguageParser.ToCriteria("()");

        tree.HasCriteria.ShouldBeFalse();
        tree.Criteria.Count.ShouldBe(0);
    }

    [Fact]
    public void ToCriteria_InvalidInput_ThrowsRqlException()
    {
        Should.Throw<RqlException>(() => RqlLanguageParser.ToCriteria("invalid"));
    }

    [Fact]
    public void ToCriteria_ParsesMultiplePredicates_Consistently()
    {
        var result1 = RqlLanguageParser.ToCriteria("(eq(Name,'John'),gt(Quantity,10))");
        var result2 = RqlLanguageParser.ToCriteria("(eq(Name,'John'),gt(Quantity,10))");

        result1.Criteria.Count.ShouldBe(result2.Criteria.Count);
        result1.Criteria[0].Operator.ShouldBe(result2.Criteria[0].Operator);
        result1.Criteria[0].Target.Name.ShouldBe(result2.Criteria[0].Target.Name);
        result1.Criteria[1].Operator.ShouldBe(result2.Criteria[1].Operator);
    }

}
