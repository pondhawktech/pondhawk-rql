using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Serialization;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class DescriptionSerializerTests
{

    // ========== Basic format ==========

    [Fact]
    public void ToDescription_NoCriteria_ProducesEmptyString()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();

        builder.ToDescription().ShouldBe(string.Empty);
    }

    [Fact]
    public void ToDescription_SinglePredicate_ProducesDescription()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        builder.ToDescription().ShouldBe("Name equals 'Widget'");
    }

    [Fact]
    public void ToDescription_MultiplePredicates_JoinedWithAnd()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(10);

        builder.ToDescription().ShouldBe("Name equals 'Widget' and Quantity is greater than 10");
    }

    // ========== All operators ==========

    [Fact]
    public void Describe_Equals()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("John");

        builder.ToDescription().ShouldBe("Name equals 'John'");
    }

    [Fact]
    public void Describe_NotEquals()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).NotEquals("John");

        builder.ToDescription().ShouldBe("Name does not equal 'John'");
    }

    [Fact]
    public void Describe_LesserThan()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThan(10);

        builder.ToDescription().ShouldBe("Quantity is less than 10");
    }

    [Fact]
    public void Describe_GreaterThan()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThan(10);

        builder.ToDescription().ShouldBe("Quantity is greater than 10");
    }

    [Fact]
    public void Describe_LesserThanOrEqual()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThanOrEqual(10);

        builder.ToDescription().ShouldBe("Quantity is less than or equal to 10");
    }

    [Fact]
    public void Describe_GreaterThanOrEqual()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThanOrEqual(10);

        builder.ToDescription().ShouldBe("Quantity is greater than or equal to 10");
    }

    [Fact]
    public void Describe_StartsWith()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("Wid");

        builder.ToDescription().ShouldBe("Name starts with 'Wid'");
    }

    [Fact]
    public void Describe_Contains()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        builder.ToDescription().ShouldBe("Name contains 'idg'");
    }

    [Fact]
    public void Describe_EndsWith()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        builder.ToDescription().ShouldBe("Name ends with 'get'");
    }

    [Fact]
    public void Describe_IsNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        builder.ToDescription().ShouldBe("Description is null");
    }

    [Fact]
    public void Describe_IsNotNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        builder.ToDescription().ShouldBe("Description is not null");
    }

    // ========== Multi-value operators ==========

    [Fact]
    public void Describe_Between_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        builder.ToDescription().ShouldBe("Quantity is between 10 and 50");
    }

    [Fact]
    public void Describe_Between_Decimal()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Between(10.00m, 50.00m);

        builder.ToDescription().ShouldBe("Price is between 10.00 and 50.00");
    }

    [Fact]
    public void Describe_In_Strings()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).In("Active", "Pending");

        builder.ToDescription().ShouldBe("Status is in ('Active', 'Pending')");
    }

    [Fact]
    public void Describe_NotIn_Ints()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotIn(1, 2, 3);

        builder.ToDescription().ShouldBe("Quantity is not in (1, 2, 3)");
    }

    // ========== Data types ==========

    [Fact]
    public void Describe_DateTime_FormattedReadably()
    {
        var dt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Created).Equals(dt);

        builder.ToDescription().ShouldBe("Created equals 2024-01-15 10:30:00");
    }

    [Fact]
    public void Describe_Bool_Lowercase()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.IsActive).Equals(true);

        builder.ToDescription().ShouldBe("IsActive equals true");
    }

    [Fact]
    public void Describe_Long()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).Equals(9876543210L);

        builder.ToDescription().ShouldBe("Sku equals 9876543210");
    }

    // ========== Complex query ==========

    [Fact]
    public void Describe_ComplexQuery()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("Widget")
            .And(p => p.Price).Between(5.00m, 100.00m)
            .And(p => p.Status).In("Active", "Pending")
            .And(p => p.Quantity).GreaterThan(0)
            .And(p => p.Description).IsNotNull();

        builder.ToDescription().ShouldBe(
            "Name contains 'Widget' and Price is between 5.00 and 100.00 and Status is in ('Active', 'Pending') and Quantity is greater than 0 and Description is not null");
    }
}
