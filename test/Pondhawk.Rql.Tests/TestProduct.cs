using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Criteria;

namespace Pondhawk.Rql.Tests;

public class TestProduct
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public long Sku { get; set; }
    public decimal Price { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
}


public class TestSearchCriteria : BaseCriteria
{
    [Criterion(Operation = RqlOperator.Equals)]
    public string Name { get; set; }

    [Criterion(Operation = RqlOperator.GreaterThanOrEqual)]
    public int? MinQuantity { get; set; }

    [Criterion(Name = "Price", Operation = RqlOperator.Between, Operand = OperandKind.From)]
    public decimal? PriceFrom { get; set; }

    [Criterion(Name = "Price", Operation = RqlOperator.Between, Operand = OperandKind.To)]
    public decimal? PriceTo { get; set; }

    [Criterion(Operation = RqlOperator.In, Operand = OperandKind.List)]
    public ICollection<string> Status { get; set; } = new List<string>();

    [Criterion(Operation = RqlOperator.In, Operand = OperandKind.ListOfInt)]
    public ICollection<int> Quantities { get; set; } = new List<int>();

    [Criterion(Operation = RqlOperator.In, Operand = OperandKind.ListOfLong)]
    public ICollection<long> Skus { get; set; } = new List<long>();
}


public class TestDefaultOpCriteria : BaseCriteria
{
    [Criterion]
    public string Name { get; set; }

    [Criterion]
    public int? Quantity { get; set; }
}
