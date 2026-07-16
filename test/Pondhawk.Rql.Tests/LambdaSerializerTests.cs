using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Serialization;
using Shouldly;
using Xunit;

// RqlTree, RqlPredicate used in type-conversion tests

namespace Pondhawk.Rql.Tests;

public class LambdaSerializerTests
{

    private static TestProduct MakeProduct(
        string name = "Widget",
        int quantity = 10,
        long sku = 1000L,
        decimal price = 19.99m,
        string status = "Active",
        bool isActive = true)
    {
        return new TestProduct
        {
            Name = name,
            Quantity = quantity,
            Sku = sku,
            Price = price,
            Created = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = status,
            IsActive = isActive
        };
    }


    // ========== Equals ==========

    [Fact]
    public void ToLambda_Equals_MatchesCorrectValue()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "Gadget")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Equals_Int()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Equals(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 5)).ShouldBeFalse();
    }


    // ========== NotEquals ==========

    [Fact]
    public void ToLambda_NotEquals_MatchesCorrectly()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).NotEquals("Widget");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Gadget")).ShouldBeTrue();
        lambda(MakeProduct(name: "Widget")).ShouldBeFalse();
    }


    // ========== Comparison operators ==========

    [Fact]
    public void ToLambda_LesserThan_MatchesCorrectly()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThan(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 5)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 10)).ShouldBeFalse();
        lambda(MakeProduct(quantity: 15)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_GreaterThan_MatchesCorrectly()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThan(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 15)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 10)).ShouldBeFalse();
        lambda(MakeProduct(quantity: 5)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_LesserThanOrEqual_MatchesCorrectly()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThanOrEqual(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 5)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 15)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_GreaterThanOrEqual_MatchesCorrectly()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThanOrEqual(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 15)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 5)).ShouldBeFalse();
    }


    // ========== String operations ==========

    [Fact]
    public void ToLambda_StartsWith_CaseSensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("Wid");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "widget")).ShouldBeFalse();
        lambda(MakeProduct(name: "Gadget")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_StartsWith_CaseInsensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("wid");

        var lambda = filter.ToLambda(insensitive: true);

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "Gadget")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Contains_CaseSensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "WIDGET")).ShouldBeFalse();
        lambda(MakeProduct(name: "Gadget")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Contains_CaseInsensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        var lambda = filter.ToLambda(insensitive: true);

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "WIDGET")).ShouldBeTrue();
        lambda(MakeProduct(name: "Gadget")).ShouldBeFalse();
    }


    // ========== EndsWith ==========

    [Fact]
    public void ToLambda_EndsWith_CaseSensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "Gadget")).ShouldBeTrue();
        lambda(MakeProduct(name: "WIDGET")).ShouldBeFalse();
        lambda(MakeProduct(name: "Gizmo")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_EndsWith_CaseInsensitive()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        var lambda = filter.ToLambda(insensitive: true);

        lambda(MakeProduct(name: "Widget")).ShouldBeTrue();
        lambda(MakeProduct(name: "WIDGET")).ShouldBeTrue();
        lambda(MakeProduct(name: "Gizmo")).ShouldBeFalse();
    }


    // ========== IsNull / IsNotNull ==========

    [Fact]
    public void ToLambda_IsNull_NullValue_ReturnsTrue()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        var lambda = filter.ToLambda();

        lambda(MakeProduct()).ShouldBeTrue();
    }

    [Fact]
    public void ToLambda_IsNull_NonNullValue_ReturnsFalse()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        var lambda = filter.ToLambda();

        var product = MakeProduct();
        product.Description = "A fine widget";
        lambda(product).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_IsNotNull_NullValue_ReturnsFalse()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        var lambda = filter.ToLambda();

        lambda(MakeProduct()).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_IsNotNull_NonNullValue_ReturnsTrue()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        var lambda = filter.ToLambda();

        var product = MakeProduct();
        product.Description = "A fine widget";
        lambda(product).ShouldBeTrue();
    }

    [Fact]
    public void ToLambda_IsNull_ValueType_AlwaysFalse()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).IsNull();

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 0)).ShouldBeFalse();
        lambda(MakeProduct(quantity: 42)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_IsNotNull_ValueType_AlwaysTrue()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).IsNotNull();

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 0)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 42)).ShouldBeTrue();
    }


    // ========== Between ==========

    [Fact]
    public void ToLambda_Between_InRange_Passes()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 30)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 50)).ShouldBeTrue();
    }

    [Fact]
    public void ToLambda_Between_OutOfRange_Fails()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 9)).ShouldBeFalse();
        lambda(MakeProduct(quantity: 51)).ShouldBeFalse();
    }


    // ========== In / NotIn ==========

    [Fact]
    public void ToLambda_In_MemberPasses()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).In("Active", "Pending");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(status: "Active")).ShouldBeTrue();
        lambda(MakeProduct(status: "Pending")).ShouldBeTrue();
        lambda(MakeProduct(status: "Inactive")).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_NotIn_NonMemberPasses()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).NotIn("Inactive", "Deleted");

        var lambda = filter.ToLambda();

        lambda(MakeProduct(status: "Active")).ShouldBeTrue();
        lambda(MakeProduct(status: "Inactive")).ShouldBeFalse();
        lambda(MakeProduct(status: "Deleted")).ShouldBeFalse();
    }


    // ========== No criteria ==========

    [Fact]
    public void ToLambda_NoCriteria_AlwaysTrue()
    {
        var filter = RqlFilterBuilder<TestProduct>.All();

        var lambda = filter.ToLambda();

        lambda(MakeProduct()).ShouldBeTrue();
        lambda(MakeProduct(name: "Anything", quantity: 0)).ShouldBeTrue();
    }


    // ========== Multiple predicates ==========

    [Fact]
    public void ToLambda_MultiplePredicates_AndedTogether()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(name: "Widget", quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(name: "Widget", quantity: 3)).ShouldBeFalse();
        lambda(MakeProduct(name: "Gadget", quantity: 10)).ShouldBeFalse();
    }


    // ========== ToExpression ==========

    [Fact]
    public void ToExpression_ReturnsExpressionTree()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var expression = filter.ToExpression();

        expression.ShouldNotBeNull();
        expression.Compile().ShouldNotBeNull();

        var compiled = expression.Compile();
        compiled(MakeProduct(name: "Widget")).ShouldBeTrue();
    }

    [Fact]
    public void ToExpression_NoCriteria_ReturnsTrueExpression()
    {
        var filter = RqlFilterBuilder<TestProduct>.All();

        var expression = filter.ToExpression();
        var compiled = expression.Compile();

        compiled(MakeProduct()).ShouldBeTrue();
    }


    // ========== Additional type coverage ==========

    [Fact]
    public void ToLambda_Equals_Decimal()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Equals(19.99m);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(price: 19.99m)).ShouldBeTrue();
        lambda(MakeProduct(price: 29.99m)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Equals_Long()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).Equals(1000L);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(sku: 1000L)).ShouldBeTrue();
        lambda(MakeProduct(sku: 2000L)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Equals_Bool()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.IsActive).Equals(true);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(isActive: true)).ShouldBeTrue();
        lambda(MakeProduct(isActive: false)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_Between_Decimal()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Between(10.00m, 50.00m);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(price: 19.99m)).ShouldBeTrue();
        lambda(MakeProduct(price: 10.00m)).ShouldBeTrue();
        lambda(MakeProduct(price: 50.00m)).ShouldBeTrue();
        lambda(MakeProduct(price: 9.99m)).ShouldBeFalse();
        lambda(MakeProduct(price: 50.01m)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_In_Ints()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).In(10, 20, 30);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 10)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 20)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 5)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_NotIn_Ints()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotIn(10, 20, 30);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 5)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 10)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_GreaterThan_Decimal()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).GreaterThan(10.00m);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(price: 19.99m)).ShouldBeTrue();
        lambda(MakeProduct(price: 10.00m)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_LesserThan_Decimal()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).LesserThan(20.00m);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(price: 19.99m)).ShouldBeTrue();
        lambda(MakeProduct(price: 20.00m)).ShouldBeFalse();
    }

    [Fact]
    public void ToLambda_NotEquals_Int()
    {
        var filter = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotEquals(10);

        var lambda = filter.ToLambda();

        lambda(MakeProduct(quantity: 5)).ShouldBeTrue();
        lambda(MakeProduct(quantity: 10)).ShouldBeFalse();
    }


    // ========== Type conversion path (parsed predicates with mismatched type) ==========

    [Fact]
    public void ToLambda_ParsedPredicate_TypeConversion_IntToLong()
    {
        // Parser creates RqlPredicate with DataType=int, but Sku property is long
        var tree = new RqlTree();
        tree.Criteria.Add(new RqlPredicate(RqlOperator.Equals, "Sku", typeof(int), 42));

        var filter = new RqlFilterBuilder<TestProduct>(tree);
        var lambda = filter.ToLambda();

        lambda(MakeProduct(sku: 42L)).ShouldBeTrue();
        lambda(MakeProduct(sku: 99L)).ShouldBeFalse();
    }

}
