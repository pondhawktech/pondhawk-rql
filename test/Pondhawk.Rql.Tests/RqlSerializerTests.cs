using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Parser;
using Pondhawk.Rql.Serialization;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class RqlSerializerTests
{

    // ========== ToRql format ==========

    [Fact]
    public void ToRql_ProducesCorrectFormat()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var rql = builder.ToRql();

        rql.ShouldBe("(eq(Name,'Widget'))");
    }

    [Fact]
    public void ToRql_NoCriteria_ProducesEmptyParens()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();

        var rql = builder.ToRql();

        rql.ShouldBe("()");
    }


    // ========== All operators ==========

    [Fact]
    public void Serialize_Equals_String()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("John");

        builder.ToRql().ShouldBe("(eq(Name,'John'))");
    }

    [Fact]
    public void Serialize_NotEquals_String()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).NotEquals("John");

        builder.ToRql().ShouldBe("(ne(Name,'John'))");
    }

    [Fact]
    public void Serialize_LesserThan_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThan(10);

        builder.ToRql().ShouldBe("(lt(Quantity,10))");
    }

    [Fact]
    public void Serialize_GreaterThan_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThan(10);

        builder.ToRql().ShouldBe("(gt(Quantity,10))");
    }

    [Fact]
    public void Serialize_LesserThanOrEqual_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThanOrEqual(10);

        builder.ToRql().ShouldBe("(le(Quantity,10))");
    }

    [Fact]
    public void Serialize_GreaterThanOrEqual_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThanOrEqual(10);

        builder.ToRql().ShouldBe("(ge(Quantity,10))");
    }

    [Fact]
    public void Serialize_StartsWith()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("Wid");

        builder.ToRql().ShouldBe("(sw(Name,'Wid'))");
    }

    [Fact]
    public void Serialize_Contains()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        builder.ToRql().ShouldBe("(cn(Name,'idg'))");
    }


    [Fact]
    public void Serialize_EndsWith()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        builder.ToRql().ShouldBe("(ew(Name,'get'))");
    }

    [Fact]
    public void Serialize_IsNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        builder.ToRql().ShouldBe("(nu(Description))");
    }

    [Fact]
    public void Serialize_IsNotNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        builder.ToRql().ShouldBe("(nn(Description))");
    }


    // ========== All types ==========

    [Fact]
    public void Serialize_Int_NoPrefix()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Equals(42);

        builder.ToRql().ShouldBe("(eq(Quantity,42))");
    }

    [Fact]
    public void Serialize_Long_NoPrefix()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).Equals(9876543210L);

        builder.ToRql().ShouldBe("(eq(Sku,9876543210))");
    }

    [Fact]
    public void Serialize_Decimal_HashPrefix()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Equals(19.99m);

        builder.ToRql().ShouldBe("(eq(Price,#19.99))");
    }

    [Fact]
    public void Serialize_DateTime_AtPrefix()
    {
        var dt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Created).Equals(dt);

        builder.ToRql().ShouldBe("(eq(Created,@2024-01-15T00:00:00Z))");
    }

    [Fact]
    public void Serialize_Bool_Lowercase()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.IsActive).Equals(true);

        builder.ToRql().ShouldBe("(eq(IsActive,true))");
    }


    // ========== Multi-value operations ==========

    [Fact]
    public void Serialize_Between_Int()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        builder.ToRql().ShouldBe("(bt(Quantity,10,50))");
    }

    [Fact]
    public void Serialize_Between_Decimal()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Between(10.00m, 50.00m);

        builder.ToRql().ShouldBe("(bt(Price,#10.00,#50.00))");
    }

    [Fact]
    public void Serialize_In_Strings()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).In("Active", "Pending");

        builder.ToRql().ShouldBe("(in(Status,'Active','Pending'))");
    }

    [Fact]
    public void Serialize_NotIn_Ints()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotIn(1, 2, 3);

        builder.ToRql().ShouldBe("(ni(Quantity,1,2,3))");
    }


    // ========== Multiple predicates ==========

    [Fact]
    public void Serialize_MultiplePredicates()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(10);

        builder.ToRql().ShouldBe("(eq(Name,'Widget'),gt(Quantity,10))");
    }


    // ========== Roundtrip ==========

    [Fact]
    public void Roundtrip_EndsWith_SerializeParseReserialize()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        var criteria1 = builder.ToRql();
        var tree = RqlLanguageParser.ToCriteria(criteria1);
        var rebuilt = new RqlFilterBuilder<TestProduct>(tree);
        var criteria2 = rebuilt.ToRql();

        criteria2.ShouldBe(criteria1);
    }

    [Fact]
    public void Roundtrip_IsNull_SerializeParseReserialize()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        var criteria1 = builder.ToRql();
        var tree = RqlLanguageParser.ToCriteria(criteria1);
        var rebuilt = new RqlFilterBuilder<TestProduct>(tree);
        var criteria2 = rebuilt.ToRql();

        criteria2.ShouldBe(criteria1);
    }

    [Fact]
    public void Roundtrip_IsNotNull_SerializeParseReserialize()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        var criteria1 = builder.ToRql();
        var tree = RqlLanguageParser.ToCriteria(criteria1);
        var rebuilt = new RqlFilterBuilder<TestProduct>(tree);
        var criteria2 = rebuilt.ToRql();

        criteria2.ShouldBe(criteria1);
    }

    [Fact]
    public void Roundtrip_SerializeParseReserialize_ProducesSameOutput()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(10)
            .And(p => p.Price).Between(5.00m, 100.00m)
            .And(p => p.Status).In("Active", "Pending");

        var criteria1 = builder.ToRql();

        var tree = RqlLanguageParser.ToCriteria(criteria1);
        var rebuilt = new RqlFilterBuilder<TestProduct>(tree);
        var criteria2 = rebuilt.ToRql();

        criteria2.ShouldBe(criteria1);
    }

}
