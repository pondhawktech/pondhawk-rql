namespace Pondhawk.Rql.Builder
{


    /// <summary>
    /// The set of comparison operators supported by RQL predicates.
    /// </summary>
    public enum RqlOperator
    {
        /// <summary>No operator has been set; used as a default before resolution.</summary>
        NotSet,

        /// <summary>Tests whether the field value is equal to the operand.</summary>
        Equals,

        /// <summary>Tests whether the field value is not equal to the operand.</summary>
        NotEquals,

        /// <summary>Tests whether the field value is strictly less than the operand.</summary>
        LesserThan,

        /// <summary>Tests whether the field value is strictly greater than the operand.</summary>
        GreaterThan,

        /// <summary>Tests whether the field value is less than or equal to the operand.</summary>
        LesserThanOrEqual,

        /// <summary>Tests whether the field value is greater than or equal to the operand.</summary>
        GreaterThanOrEqual,

        /// <summary>Tests whether the string field value starts with the operand.</summary>
        StartsWith,

        /// <summary>Tests whether the string field value contains the operand as a substring.</summary>
        Contains,

        /// <summary>Tests whether the field value falls within an inclusive range defined by two operands.</summary>
        Between,

        /// <summary>Tests whether the field value is present in a set of operands.</summary>
        In,

        /// <summary>Tests whether the field value is not present in a set of operands.</summary>
        NotIn,

        /// <summary>Tests whether the string field value ends with the operand.</summary>
        EndsWith,

        /// <summary>Tests whether the field value is null.</summary>
        IsNull,

        /// <summary>Tests whether the field value is not null.</summary>
        IsNotNull
    }



}
