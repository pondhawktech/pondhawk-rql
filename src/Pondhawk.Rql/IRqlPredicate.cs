using System.Diagnostics.CodeAnalysis;
using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql
{


    /// <summary>
    /// A single filter predicate consisting of an operator, target field, data type, and values.
    /// </summary>
    public interface IRqlPredicate
    {
        /// <summary>The comparison operator (e.g. Equals, GreaterThan, Between).</summary>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Operator is the domain-specific name for the RQL comparison operator")]
        RqlOperator Operator { get; }

        /// <summary>The target field name.</summary>
        Target Target { get; }

        /// <summary>The CLR type of the predicate values.</summary>
        Type DataType { get; }

        /// <summary>The operand values for this predicate.</summary>
        IReadOnlyList<object> Values { get; }

    }



}
