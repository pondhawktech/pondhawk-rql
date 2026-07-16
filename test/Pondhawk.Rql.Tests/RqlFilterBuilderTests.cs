using Pondhawk.Rql.Builder;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class RqlFilterBuilderTests
{

    // ========== Factory methods ==========

    [Fact]
    public void Create_ReturnsEmptyBuilder()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();

        builder.HasCriteria.ShouldBeFalse();
        builder.Criteria.ShouldBeEmpty();
    }

    [Fact]
    public void All_ReturnsEmptyBuilder()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();

        builder.HasCriteria.ShouldBeFalse();
    }

    [Fact]
    public void Where_Expression_ProducesPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.HasCriteria.ShouldBeTrue();
        builder.Criteria.Count().ShouldBe(1);
    }


    // ========== Equals - all types ==========

    [Fact]
    public void Where_Equals_String_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("Widget");
    }

    [Fact]
    public void Where_Equals_Int_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Equals(42);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Quantity");
        pred.DataType.ShouldBe(typeof(int));
        pred.Values[0].ShouldBe(42);
    }

    [Fact]
    public void Where_Equals_Long_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).Equals(9876543210L);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Sku");
        pred.DataType.ShouldBe(typeof(long));
        pred.Values[0].ShouldBe(9876543210L);
    }

    [Fact]
    public void Where_Equals_Decimal_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Equals(19.99m);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Price");
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values[0].ShouldBe(19.99m);
    }

    [Fact]
    public void Where_Equals_DateTime_ProducesCorrectPredicate()
    {
        var dt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Created).Equals(dt);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Created");
        pred.DataType.ShouldBe(typeof(DateTime));
        pred.Values[0].ShouldBe(dt);
    }

    [Fact]
    public void Where_Equals_Bool_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.IsActive).Equals(true);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("IsActive");
        pred.DataType.ShouldBe(typeof(bool));
        pred.Values[0].ShouldBe(true);
    }


    // ========== Other comparison operators ==========

    [Fact]
    public void Where_NotEquals_String_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).NotEquals("Widget");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.Values[0].ShouldBe("Widget");
    }

    [Fact]
    public void Where_LesserThan_Int_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThan(10);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThan);
        pred.Values[0].ShouldBe(10);
    }

    [Fact]
    public void Where_LesserThanOrEqual_Int_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThanOrEqual(10);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
        pred.Values[0].ShouldBe(10);
    }

    [Fact]
    public void Where_GreaterThan_Int_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThan(10);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThan);
        pred.Values[0].ShouldBe(10);
    }

    [Fact]
    public void Where_GreaterThanOrEqual_Int_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThanOrEqual(10);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
        pred.Values[0].ShouldBe(10);
    }


    // ========== String operations ==========

    [Fact]
    public void Where_StartsWith_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("Wid");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.StartsWith);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("Wid");
    }

    [Fact]
    public void Where_Contains_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Contains);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("idg");
    }


    // ========== Multi-value operations ==========

    [Fact]
    public void Where_Between_Int_ProducesCorrectValues()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe(10);
        pred.Values[1].ShouldBe(50);
    }

    [Fact]
    public void Where_In_Strings_ProducesCorrectValues()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).In("Active", "Pending");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe("Active");
        pred.Values[1].ShouldBe("Pending");
    }

    [Fact]
    public void Where_NotIn_Ints_ProducesCorrectValues()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotIn(1, 2, 3);

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }


    // ========== Chaining ==========

    [Fact]
    public void And_ChainsMultiplePredicates()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5)
            .And(p => p.Price).LesserThan(100m);

        builder.Criteria.Count().ShouldBe(3);

        var preds = builder.Criteria.ToList();
        preds[0].Target.Name.ShouldBe("Name");
        preds[1].Target.Name.ShouldBe("Quantity");
        preds[2].Target.Name.ShouldBe("Price");
    }


    // ========== Non-generic builder ==========

    [Fact]
    public void NonGeneric_Where_Equals_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder.Where("Name").Equals("Widget");

        builder.HasCriteria.ShouldBeTrue();
        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.Values[0].ShouldBe("Widget");
    }

    [Fact]
    public void NonGeneric_IsAll_TrueWhenNoCriteria()
    {
        var builder = RqlFilterBuilder.All();

        builder.IsAll.ShouldBeTrue();
    }

    [Fact]
    public void NonGeneric_IsAll_FalseWhenHasCriteria()
    {
        var builder = RqlFilterBuilder.Where("Name").Equals("test");

        builder.IsAll.ShouldBeFalse();
    }

    [Fact]
    public void NonGeneric_And_ChainsPredicates()
    {
        var builder = RqlFilterBuilder
            .Where("Name").Equals("Widget")
            .And("Quantity").GreaterThan(5);

        builder.Criteria.Count().ShouldBe(2);
    }


    // ========== Criteria management ==========

    [Fact]
    public void HasCriteria_FalseWhenEmpty()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();

        builder.HasCriteria.ShouldBeFalse();
    }

    [Fact]
    public void HasCriteria_TrueAfterAddingPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.HasCriteria.ShouldBeTrue();
    }

    [Fact]
    public void Clear_RemovesAllPredicates()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        builder.Criteria.Count().ShouldBe(2);

        builder.Clear();

        builder.HasCriteria.ShouldBeFalse();
        builder.Criteria.ShouldBeEmpty();
    }

    [Fact]
    public void Add_ExternalPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();
        var predicate = new RqlPredicate<string>(RqlOperator.Equals, "Name", "Widget");

        builder.Add(predicate);

        builder.HasCriteria.ShouldBeTrue();
        builder.Criteria.First().ShouldBe(predicate);
    }


    // ========== Predicate queries ==========

    [Fact]
    public void AtLeastOne_ReturnsTrue_WhenMatching()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        builder.AtLeastOne(p => p.Operator == RqlOperator.Equals).ShouldBeTrue();
    }

    [Fact]
    public void AtLeastOne_ReturnsFalse_WhenNoneMatch()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.AtLeastOne(p => p.Operator == RqlOperator.Contains).ShouldBeFalse();
    }

    [Fact]
    public void OnlyOne_ReturnsTrue_WhenExactlyOneMatches()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        builder.OnlyOne(p => p.Operator == RqlOperator.Equals).ShouldBeTrue();
    }

    [Fact]
    public void OnlyOne_ReturnsFalse_WhenMultipleMatch()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Status).Equals("Active");

        builder.OnlyOne(p => p.Operator == RqlOperator.Equals).ShouldBeFalse();
    }

    [Fact]
    public void None_ReturnsTrue_WhenNoneMatch()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.None(p => p.Operator == RqlOperator.Contains).ShouldBeTrue();
    }

    [Fact]
    public void None_ReturnsFalse_WhenSomeMatch()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.None(p => p.Operator == RqlOperator.Equals).ShouldBeFalse();
    }


    // ========== RowLimit ==========

    [Fact]
    public void RowLimit_DefaultsToZero()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();

        builder.RowLimit.ShouldBe(0);
    }

    [Fact]
    public void RowLimit_GetSet()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();
        builder.RowLimit = 25;

        builder.RowLimit.ShouldBe(25);
    }


    // ========== Constructor from RqlTree ==========

    [Fact]
    public void Constructor_FromRqlTree_PopulatesCriteria()
    {
        var tree = new RqlTree();
        tree.Criteria.Add(new RqlPredicate<string>(RqlOperator.Equals, "Name", "Widget"));
        tree.Criteria.Add(new RqlPredicate<int>(RqlOperator.GreaterThan, "Quantity", 10));

        var builder = new RqlFilterBuilder<TestProduct>(tree);

        builder.HasCriteria.ShouldBeTrue();
        builder.Criteria.Count().ShouldBe(2);
    }


    // ========== Target type ==========

    [Fact]
    public void Target_ReturnsEntityType()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();

        builder.Target.ShouldBe(typeof(TestProduct));
    }

    [Fact]
    public void Is_ReturnsTrue_ForMatchingType()
    {
        var builder = RqlFilterBuilder<TestProduct>.Create();

        builder.Is<TestProduct>().ShouldBeTrue();
    }


    // ========== NotEquals - remaining type overloads ==========

    [Fact]
    public void Where_NotEquals_Int()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Quantity).NotEquals(5).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values[0].ShouldBe(5);
    }

    [Fact]
    public void Where_NotEquals_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).NotEquals(9876543210L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.DataType.ShouldBe(typeof(long));
        pred.Values[0].ShouldBe(9876543210L);
    }

    [Fact]
    public void Where_NotEquals_DateTime()
    {
        var dt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).NotEquals(dt).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.DataType.ShouldBe(typeof(DateTime));
        pred.Values[0].ShouldBe(dt);
    }

    [Fact]
    public void Where_NotEquals_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).NotEquals(9.99m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values[0].ShouldBe(9.99m);
    }


    // ========== LesserThan - remaining type overloads ==========

    [Fact]
    public void Where_LesserThan_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Name).LesserThan("M").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThan);
        pred.DataType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Where_LesserThan_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).LesserThan(5000L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThan);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_LesserThan_DateTime()
    {
        var dt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).LesserThan(dt).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThan);
        pred.DataType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void Where_LesserThan_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).LesserThan(50.00m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThan);
        pred.DataType.ShouldBe(typeof(decimal));
    }


    // ========== LesserThanOrEqual - remaining type overloads ==========

    [Fact]
    public void Where_LesserThanOrEqual_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Name).LesserThanOrEqual("M").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
        pred.DataType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Where_LesserThanOrEqual_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).LesserThanOrEqual(5000L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_LesserThanOrEqual_DateTime()
    {
        var dt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).LesserThanOrEqual(dt).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
        pred.DataType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void Where_LesserThanOrEqual_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).LesserThanOrEqual(50.00m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
        pred.DataType.ShouldBe(typeof(decimal));
    }


    // ========== GreaterThan - remaining type overloads ==========

    [Fact]
    public void Where_GreaterThan_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Name).GreaterThan("M").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThan);
        pred.DataType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Where_GreaterThan_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).GreaterThan(5000L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThan);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_GreaterThan_DateTime()
    {
        var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).GreaterThan(dt).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThan);
        pred.DataType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void Where_GreaterThan_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).GreaterThan(10.00m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThan);
        pred.DataType.ShouldBe(typeof(decimal));
    }


    // ========== GreaterThanOrEqual - remaining type overloads ==========

    [Fact]
    public void Where_GreaterThanOrEqual_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Name).GreaterThanOrEqual("M").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
        pred.DataType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Where_GreaterThanOrEqual_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).GreaterThanOrEqual(5000L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_GreaterThanOrEqual_DateTime()
    {
        var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).GreaterThanOrEqual(dt).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
        pred.DataType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void Where_GreaterThanOrEqual_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).GreaterThanOrEqual(10.00m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
        pred.DataType.ShouldBe(typeof(decimal));
    }


    // ========== Between - remaining type overloads ==========

    [Fact]
    public void Where_Between_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).Between(1000L, 9999L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(long));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_Between_DateTime()
    {
        var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).Between(from, to).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(DateTime));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_Between_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).Between(10.00m, 99.99m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_Between_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Name).Between("A", "M").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
    }


    // ========== In - params overloads ==========

    [Fact]
    public void Where_In_Params_Int()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Quantity).In(1, 2, 3).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }

    [Fact]
    public void Where_In_Params_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).In(100L, 200L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(long));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_In_Params_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).In(9.99m, 19.99m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_In_Params_DateTime()
    {
        var d1 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var d2 = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).In(d1, d2).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(DateTime));
        pred.Values.Count.ShouldBe(2);
    }


    // ========== In - IEnumerable overloads ==========

    [Fact]
    public void Where_In_Enumerable_String()
    {
        var values = new List<string> { "Active", "Pending" };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Status).In(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_In_Enumerable_Int()
    {
        var values = new List<int> { 1, 2, 3 };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Quantity).In(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }

    [Fact]
    public void Where_In_Enumerable_Long()
    {
        var values = new List<long> { 100L, 200L };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).In(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_In_Enumerable_Decimal()
    {
        var values = new List<decimal> { 9.99m, 19.99m };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).In(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(decimal));
    }

    [Fact]
    public void Where_In_Enumerable_DateTime()
    {
        var values = new List<DateTime> { new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).In(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(DateTime));
    }


    // ========== NotIn - params overloads ==========

    [Fact]
    public void Where_NotIn_Params_String()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Status).NotIn("Inactive", "Deleted").Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Where_NotIn_Params_Long()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).NotIn(100L, 200L).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_NotIn_Params_Decimal()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).NotIn(9.99m, 19.99m).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(decimal));
    }

    [Fact]
    public void Where_NotIn_Params_DateTime()
    {
        var d1 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var d2 = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).NotIn(d1, d2).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(DateTime));
    }


    // ========== NotIn - IEnumerable overloads ==========

    [Fact]
    public void Where_NotIn_Enumerable_String()
    {
        var values = new List<string> { "Inactive", "Deleted" };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Status).NotIn(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(string));
    }

    [Fact]
    public void Where_NotIn_Enumerable_Int()
    {
        var values = new List<int> { 1, 2, 3 };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Quantity).NotIn(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(int));
    }

    [Fact]
    public void Where_NotIn_Enumerable_Long()
    {
        var values = new List<long> { 100L, 200L };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Sku).NotIn(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Where_NotIn_Enumerable_Decimal()
    {
        var values = new List<decimal> { 9.99m, 19.99m };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Price).NotIn(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(decimal));
    }

    [Fact]
    public void Where_NotIn_Enumerable_DateTime()
    {
        var values = new List<DateTime> { new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) };
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.Created).NotIn(values).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(DateTime));
    }


    // ========== Implicit operator ==========

    [Fact]
    public void ImplicitOperator_ConvertsToPredicateList()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        List<IRqlPredicate> list = builder;

        list.Count.ShouldBe(2);
    }


    // ========== NotEquals(bool) ==========

    [Fact]
    public void Where_NotEquals_Bool()
    {
        var pred = RqlFilterBuilder<TestProduct>.Where(p => p.IsActive).NotEquals(true).Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.NotEquals);
        pred.DataType.ShouldBe(typeof(bool));
        pred.Values[0].ShouldBe(true);
    }


    // ========== EndsWith ==========

    [Fact]
    public void Where_EndsWith_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.EndsWith);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("get");
    }


    // ========== IsNull / IsNotNull ==========

    [Fact]
    public void Where_IsNull_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.IsNull);
        pred.Target.Name.ShouldBe("Description");
        pred.DataType.ShouldBe(typeof(object));
        pred.Values.Count.ShouldBe(0);
    }

    [Fact]
    public void Where_IsNotNull_ProducesCorrectPredicate()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        var pred = builder.Criteria.First();
        pred.Operator.ShouldBe(RqlOperator.IsNotNull);
        pred.Target.Name.ShouldBe("Description");
        pred.DataType.ShouldBe(typeof(object));
        pred.Values.Count.ShouldBe(0);
    }


    // ========== Introspect ==========

    [Fact]
    public void Introspect_SingleValueCriteria_BuildsPredicates()
    {
        var criteria = new TestSearchCriteria
        {
            Name = "Widget",
            MinQuantity = 10
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeTrue();
        builder.AtLeastOne(p => p.Target == "Name" && p.Operator == RqlOperator.Equals).ShouldBeTrue();
        builder.AtLeastOne(p => p.Target == "MinQuantity" && p.Operator == RqlOperator.GreaterThanOrEqual).ShouldBeTrue();
    }

    [Fact]
    public void Introspect_NullValues_AreSkipped()
    {
        var criteria = new TestSearchCriteria
        {
            Name = "Widget",
            MinQuantity = null
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.Criteria.Count().ShouldBe(1);
        builder.AtLeastOne(p => p.Target == "Name").ShouldBeTrue();
    }

    [Fact]
    public void Introspect_BetweenFromTo_BuildsSinglePredicate()
    {
        var criteria = new TestSearchCriteria
        {
            PriceFrom = 10.00m,
            PriceTo = 50.00m
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeTrue();
        var pred = builder.Criteria.First(p => p.Target == "Price");
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Introspect_ListOperand_BuildsInPredicate()
    {
        var criteria = new TestSearchCriteria
        {
            Status = new List<string> { "Active", "Pending" }
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeTrue();
        var pred = builder.Criteria.First(p => p.Target == "Status");
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
    }

    [Fact]
    public void Introspect_EmptyCollection_IsSkipped()
    {
        var criteria = new TestSearchCriteria
        {
            Status = new List<string>()
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeFalse();
    }

    [Fact]
    public void Introspect_ListOfInt_BuildsInPredicate()
    {
        var criteria = new TestSearchCriteria
        {
            Quantities = new List<int> { 10, 20, 30 }
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeTrue();
        var pred = builder.Criteria.First(p => p.Target == "Quantities");
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }

    [Fact]
    public void Introspect_ListOfLong_BuildsInPredicate()
    {
        var criteria = new TestSearchCriteria
        {
            Skus = new List<long> { 100L, 200L }
        };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        builder.HasCriteria.ShouldBeTrue();
        var pred = builder.Criteria.First(p => p.Target == "Skus");
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(long));
    }

    [Fact]
    public void Introspect_DefaultOp_StringDefaultsToStartsWith()
    {
        var criteria = new TestDefaultOpCriteria { Name = "Wid" };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        var pred = builder.Criteria.First(p => p.Target == "Name");
        pred.Operator.ShouldBe(RqlOperator.StartsWith);
    }

    [Fact]
    public void Introspect_DefaultOp_NonStringDefaultsToEquals()
    {
        var criteria = new TestDefaultOpCriteria { Quantity = 42 };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria);

        var pred = builder.Criteria.First(p => p.Target == "Quantity");
        pred.Operator.ShouldBe(RqlOperator.Equals);
    }

    [Fact]
    public void Introspect_WithNameMap_RemapsTarget()
    {
        var criteria = new TestSearchCriteria { Name = "Widget" };
        var map = new Dictionary<string, string> { ["Name"] = "ProductName" };

        var builder = RqlFilterBuilder<TestProduct>.Create().Introspect(criteria, map);

        var pred = builder.Criteria.First();
        pred.Target.Name.ShouldBe("ProductName");
    }

}
