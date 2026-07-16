using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rql.Builder
{


    /// <summary>
    /// An untyped RQL predicate with an explicit <see cref="IRqlPredicate.DataType"/>.
    /// Used by the parser when the value type is determined at parse time.
    /// </summary>
    public class RqlPredicate : RqlPredicate<object>
    {


        /// <summary>
        /// Initializes a new <see cref="RqlPredicate"/> with a single value and an explicit data type.
        /// </summary>
        /// <param name="op">The comparison operator.</param>
        /// <param name="name">The target field name.</param>
        /// <param name="dataType">The CLR type of the value.</param>
        /// <param name="value">The operand value.</param>
        public RqlPredicate(RqlOperator op, string name, Type dataType, object value) : base(op, name, value)
        {
            DataType = dataType;
        }


        /// <summary>
        /// Initializes a new <see cref="RqlPredicate"/> with multiple values and an explicit data type.
        /// </summary>
        /// <param name="op">The comparison operator.</param>
        /// <param name="name">The target field name.</param>
        /// <param name="dataType">The CLR type of the values.</param>
        /// <param name="values">The operand values.</param>
        public RqlPredicate(RqlOperator op, string name, Type dataType, IEnumerable<object> values) : base(op, name, values)
        {
            DataType = dataType;
        }


    }


    /// <summary>
    /// A strongly-typed RQL predicate holding values of type <typeparamref name="TType"/>.
    /// </summary>
    public class RqlPredicate<TType> : IRqlPredicate
    {

        private IReadOnlyList<object>? _cachedValues;

        /// <summary>
        /// Initializes a new <see cref="RqlPredicate{TType}"/> with a single value.
        /// </summary>
        /// <param name="op">The comparison operator.</param>
        /// <param name="name">The target field name.</param>
        /// <param name="value">The operand value.</param>
        public RqlPredicate(RqlOperator op, string name, TType value)
        {

            Guard.IsNotNull(value);
            Guard.IsNotNullOrWhiteSpace(name);

            Operator = op;
            Target = new Target(name);

            Values = new List<TType>();

            DataType = typeof(TType);
            Value = value;

        }


        /// <summary>
        /// Initializes a new <see cref="RqlPredicate{TType}"/> with multiple values.
        /// </summary>
        /// <param name="op">The comparison operator.</param>
        /// <param name="name">The target field name.</param>
        /// <param name="values">The operand values.</param>
        public RqlPredicate(RqlOperator op, string name, IEnumerable<TType> values)
        {

            Guard.IsNotNull(values);
            Guard.IsNotNullOrWhiteSpace(name);

            Operator = op;
            Target = new Target(name);

            DataType = typeof(TType);
            Values = new List<TType>(values);

        }


        /// <inheritdoc />
        public RqlOperator Operator { get; set; }

        /// <inheritdoc />
        public Target Target { get; set; }

        /// <inheritdoc />
        public Type DataType { get; set; }

        /// <summary>The strongly-typed list of operand values for this predicate.</summary>
        public IList<TType> Values { get; }

        /// <inheritdoc />
        IReadOnlyList<object> IRqlPredicate.Values => _cachedValues ??= Values.Cast<object>().ToList();

        /// <summary>Gets or sets the primary operand value (the first element in <see cref="Values"/>).</summary>
        public TType Value
        {
            get => (Values.Count > 0 ? Values[0] : default!);

            set
            {
                _cachedValues = null;
                Values.Clear();
                Values.Add(value);
            }

        }


    }



}
