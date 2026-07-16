using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Rql.Builder;

/// <summary>
/// Specifies how a criteria property maps to an RQL predicate operand during introspection.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single refers to a single operand, not the System.Single type")]
public enum OperandKind
{
    /// <summary>A single operand value, used for most comparison operators.</summary>
    Single,

    /// <summary>The lower bound of a range query (e.g. the <c>from</c> value in a <c>Between</c> predicate).</summary>
    From,

    /// <summary>The upper bound of a range query (e.g. the <c>to</c> value in a <c>Between</c> predicate).</summary>
    To,

    /// <summary>A list of string operand values, used for <c>In</c> and <c>NotIn</c> operators.</summary>
    List,

    /// <summary>A list of integer operand values, used for <c>In</c> and <c>NotIn</c> operators.</summary>
    ListOfInt,

    /// <summary>A list of long integer operand values, used for <c>In</c> and <c>NotIn</c> operators.</summary>
    ListOfLong
}
