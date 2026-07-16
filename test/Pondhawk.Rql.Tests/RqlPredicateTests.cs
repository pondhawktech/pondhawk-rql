using Pondhawk.Rql.Builder;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class RqlPredicateTests
{

    // ========== RqlPredicate<T> - single value ==========

    [Fact]
    public void GenericPredicate_SingleValue_SetsAllProperties()
    {
        var pred = new RqlPredicate<string>(RqlOperator.Equals, "Name", "Widget");

        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(string));
        pred.Value.ShouldBe("Widget");
        pred.Values.Count.ShouldBe(1);
        pred.Values[0].ShouldBe("Widget");
    }

    [Fact]
    public void GenericPredicate_Int_SetsDataType()
    {
        var pred = new RqlPredicate<int>(RqlOperator.GreaterThan, "Quantity", 42);

        pred.DataType.ShouldBe(typeof(int));
        pred.Value.ShouldBe(42);
    }


    // ========== RqlPredicate<T> - multi value ==========

    [Fact]
    public void GenericPredicate_MultiValue_SetsValues()
    {
        var pred = new RqlPredicate<int>(RqlOperator.In, "Quantity", new[] { 1, 2, 3 });

        pred.Operator.ShouldBe(RqlOperator.In);
        pred.Values.Count.ShouldBe(3);
        pred.Values[0].ShouldBe(1);
        pred.Values[1].ShouldBe(2);
        pred.Values[2].ShouldBe(3);
    }

    [Fact]
    public void GenericPredicate_Between_HasTwoValues()
    {
        var pred = new RqlPredicate<decimal>(RqlOperator.Between, "Price", new[] { 10.00m, 50.00m });

        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe(10.00m);
        pred.Values[1].ShouldBe(50.00m);
    }


    // ========== RqlPredicate (non-generic) ==========

    [Fact]
    public void NonGenericPredicate_SingleValue_SetsExplicitDataType()
    {
        var pred = new RqlPredicate(RqlOperator.Equals, "Name", typeof(string), "Widget");

        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(string));
        ((IRqlPredicate)pred).Values[0].ShouldBe("Widget");
    }

    [Fact]
    public void NonGenericPredicate_MultiValue_SetsExplicitDataType()
    {
        var values = new object[] { 10, 50 };
        var pred = new RqlPredicate(RqlOperator.Between, "Quantity", typeof(int), values);

        pred.DataType.ShouldBe(typeof(int));
        ((IRqlPredicate)pred).Values.Count.ShouldBe(2);
    }


    // ========== IRqlPredicate.Values ==========

    [Fact]
    public void IRqlPredicate_Values_ReturnsObjectList()
    {
        var pred = new RqlPredicate<string>(RqlOperator.In, "Status", new[] { "A", "B" });

        IRqlPredicate iface = pred;
        iface.Values.Count.ShouldBe(2);
        iface.Values[0].ShouldBe("A");
        iface.Values[1].ShouldBe("B");
    }


    // ========== Target ==========

    [Fact]
    public void Target_EqualityOperator_CaseInsensitive()
    {
        var target = new Target("Name");

        (target == "Name").ShouldBeTrue();
        (target == "name").ShouldBeTrue();
        (target == "NAME").ShouldBeTrue();
        (target == "Other").ShouldBeFalse();
    }

    [Fact]
    public void Target_InequalityOperator()
    {
        var target = new Target("Name");

        (target != "Other").ShouldBeTrue();
        (target != "Name").ShouldBeFalse();
        (target != "").ShouldBeTrue();
    }

    [Fact]
    public void Target_Equals_SameName()
    {
        var t1 = new Target("Name");
        var t2 = new Target("Name");

        t1.Equals(t2).ShouldBeTrue();
    }

    [Fact]
    public void Target_Equals_DifferentName()
    {
        var t1 = new Target("Name");
        var t2 = new Target("Other");

        t1.Equals(t2).ShouldBeFalse();
    }

    [Fact]
    public void Target_ToString_ReturnsName()
    {
        var target = new Target("Name");

        target.ToString().ShouldBe("Name");
    }

    [Fact]
    public void Target_GetHashCode_SameForSameName()
    {
        var t1 = new Target("Name");
        var t2 = new Target("Name");

        t1.GetHashCode().ShouldBe(t2.GetHashCode());
    }

}
