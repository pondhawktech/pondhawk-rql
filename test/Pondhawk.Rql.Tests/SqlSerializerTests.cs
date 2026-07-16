using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Serialization;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class SqlSerializerTests
{

    // ========== ToSqlQuery ==========

    [Fact]
    public void ToSqlQuery_Generic_UsesTypeName()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var (sql, parameters) = builder.ToSqlQuery();

        sql.ShouldStartWith("select * from TestProduct");
        sql.ShouldContain("where");
    }

    [Fact]
    public void ToSqlQuery_WithTableName()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var (sql, _) = builder.ToSqlQuery("Products");

        sql.ShouldStartWith("select * from Products");
    }

    [Fact]
    public void ToSqlQuery_NoCriteria_NoWhere()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();

        var (sql, parameters) = builder.ToSqlQuery();

        sql.ShouldBe("select * from TestProduct");
        parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ToSqlQuery_NoCriteria_WithRowLimit()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();
        builder.RowLimit = 25;

        var (sql, parameters) = builder.ToSqlQuery();

        sql.ShouldBe("select * from TestProduct limit 25");
        parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ToSqlQuery_WithCriteria_WithRowLimit()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");
        builder.RowLimit = 10;

        var (sql, _) = builder.ToSqlQuery();

        sql.ShouldContain("where");
        sql.ShouldEndWith("limit 10");
    }


    // ========== ToSqlWhere ==========

    [Fact]
    public void ToSqlWhere_NoCriteria_ReturnsEmpty()
    {
        var builder = RqlFilterBuilder<TestProduct>.All();

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBeEmpty();
        parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ToSqlWhere_Equals_Parameterized()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Name = {0}");
        parameters.Length.ShouldBe(1);
        parameters[0].ShouldBe("Widget");
    }

    [Fact]
    public void ToSqlWhere_NonIndexed_UsesQuestionMark()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget");

        var (sql, parameters) = builder.ToSqlWhere(indexed: false);

        sql.ShouldBe("Name = ?");
        parameters.Length.ShouldBe(1);
        parameters[0].ShouldBe("Widget");
    }


    // ========== All operators ==========

    [Fact]
    public void ToSqlWhere_NotEquals()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).NotEquals(5);

        var (sql, _) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity <> {0}");
    }

    [Fact]
    public void ToSqlWhere_LesserThan()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThan(10);

        var (sql, _) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity < {0}");
    }

    [Fact]
    public void ToSqlWhere_GreaterThan()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThan(10);

        var (sql, _) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity > {0}");
    }

    [Fact]
    public void ToSqlWhere_LesserThanOrEqual()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).LesserThanOrEqual(10);

        var (sql, _) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity <= {0}");
    }

    [Fact]
    public void ToSqlWhere_GreaterThanOrEqual()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).GreaterThanOrEqual(10);

        var (sql, _) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity >= {0}");
    }

    [Fact]
    public void ToSqlWhere_StartsWith_LikePattern()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).StartsWith("Wid");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Name like {0}");
        parameters[0].ShouldBe("Wid%");
    }

    [Fact]
    public void ToSqlWhere_Contains_LikePattern()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Contains("idg");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Name like {0}");
        parameters[0].ShouldBe("%idg%");
    }

    [Fact]
    public void ToSqlWhere_Between_TwoParameters()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity between {0} and {1}");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe(10);
        parameters[1].ShouldBe(50);
    }

    [Fact]
    public void ToSqlWhere_Between_NonIndexed()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).Between(10, 50);

        var (sql, _) = builder.ToSqlWhere(indexed: false);

        sql.ShouldBe("Quantity between ? and ?");
    }

    [Fact]
    public void ToSqlWhere_In_Parameterized_Strings()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).In("Active", "Pending");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Status in ({0},{1})");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe("Active");
        parameters[1].ShouldBe("Pending");
    }

    [Fact]
    public void ToSqlWhere_In_Parameterized_Ints()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Quantity).In(1, 2, 3);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Quantity in ({0},{1},{2})");
        parameters.Length.ShouldBe(3);
        parameters[0].ShouldBe(1);
        parameters[1].ShouldBe(2);
        parameters[2].ShouldBe(3);
    }

    [Fact]
    public void ToSqlWhere_NotIn_Parameterized()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Status).NotIn("Inactive", "Deleted");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Status not in ({0},{1})");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe("Inactive");
        parameters[1].ShouldBe("Deleted");
    }


    [Fact]
    public void ToSqlWhere_EndsWith_LikePattern()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).EndsWith("get");

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Name like {0}");
        parameters[0].ShouldBe("%get");
    }

    [Fact]
    public void ToSqlWhere_IsNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNull();

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Description is null");
        parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ToSqlWhere_IsNotNull()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Description).IsNotNull();

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Description is not null");
        parameters.ShouldBeEmpty();
    }


    // ========== Multiple predicates ==========

    [Fact]
    public void ToSqlWhere_MultiplePredicates_JoinedWithAnd()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Name).Equals("Widget")
            .And(p => p.Quantity).GreaterThan(5);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Name = {0} and Quantity > {1}");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe("Widget");
        parameters[1].ShouldBe(5);
    }


    // ========== Additional type coverage ==========

    [Fact]
    public void ToSqlWhere_Equals_Decimal_Parameter()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Equals(19.99m);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Price = {0}");
        parameters[0].ShouldBe(19.99m);
    }

    [Fact]
    public void ToSqlWhere_Equals_Long_Parameter()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).Equals(9876543210L);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Sku = {0}");
        parameters[0].ShouldBe(9876543210L);
    }

    [Fact]
    public void ToSqlWhere_Equals_DateTime_Parameter()
    {
        var dt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Created).Equals(dt);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Created = {0}");
        parameters[0].ShouldBeOfType<DateTime>();
    }

    [Fact]
    public void ToSqlWhere_Equals_Bool_Parameter()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.IsActive).Equals(true);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("IsActive = {0}");
        parameters[0].ShouldBe(true);
    }

    [Fact]
    public void ToSqlWhere_Between_Decimal_Parameters()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).Between(10.00m, 50.00m);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Price between {0} and {1}");
        parameters[0].ShouldBe(10.00m);
        parameters[1].ShouldBe(50.00m);
    }

    [Fact]
    public void ToSqlWhere_In_Decimals_Parameterized()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Price).In(9.99m, 19.99m);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Price in ({0},{1})");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe(9.99m);
        parameters[1].ShouldBe(19.99m);
    }

    [Fact]
    public void ToSqlWhere_NotIn_Longs_Parameterized()
    {
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Sku).NotIn(100L, 200L);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Sku not in ({0},{1})");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBe(100L);
        parameters[1].ShouldBe(200L);
    }

    [Fact]
    public void ToSqlWhere_In_DateTime_Parameterized()
    {
        var d1 = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var d2 = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var builder = RqlFilterBuilder<TestProduct>
            .Where(p => p.Created).In(d1, d2);

        var (sql, parameters) = builder.ToSqlWhere();

        sql.ShouldBe("Created in ({0},{1})");
        parameters.Length.ShouldBe(2);
        parameters[0].ShouldBeOfType<DateTime>();
        parameters[1].ShouldBeOfType<DateTime>();
    }

}
