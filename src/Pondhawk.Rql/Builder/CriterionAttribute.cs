using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Rql.Builder;

/// <summary>
/// Marks a property on an <see cref="Criteria.ICriteria"/> object for automatic RQL filter building via <c>Introspect()</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CriterionAttribute : Attribute
{
    /// <summary>The target field name. Defaults to the property name if empty.</summary>
    public string Name { get; set; } = "";

    /// <summary>The RQL operator to apply. Defaults to <see cref="RqlOperator.StartsWith"/> for strings, <see cref="RqlOperator.Equals"/> for other types.</summary>
    public RqlOperator Operation { get; set; } = RqlOperator.NotSet;

    /// <summary>How this property maps to predicate values (single value, range boundary, or list).</summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single refers to a single operand, not the System.Single type")]
    public OperandKind Operand { get; set; } = OperandKind.Single;

}
