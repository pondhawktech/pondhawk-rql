/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rql.Builder
{


    /// <summary>
    /// Strongly-typed RQL filter builder for entity type <typeparamref name="TTarget"/>.
    /// Use <c>Where(expr)</c> to begin, then chain operators and <c>And(expr)</c> for additional predicates.
    /// </summary>
    /// <typeparam name="TTarget">The entity type to filter.</typeparam>
    /// <remarks>
    /// The builder produces an <see cref="RqlTree"/> AST that can be serialized to multiple targets:
    /// <list type="bullet">
    /// <item><c>ToLambda&lt;T&gt;()</c> — compiled LINQ predicate for in-memory filtering</item>
    /// <item><c>ToExpression&lt;T&gt;()</c> — expression tree for IQueryable (EF Core)</item>
    /// <item><c>ToRql()</c> — RQL text format</item>
    /// <item><c>ToSqlWhere()</c> — parameterized SQL WHERE clause</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Build a filter with multiple predicates
    /// var filter = RqlFilterBuilder&lt;Order&gt;
    ///     .Where(o =&gt; o.Status).Equals("Active")
    ///     .And(o =&gt; o.Total).GreaterThan(100m)
    ///     .And(o =&gt; o.Category).In("Electronics", "Books");
    ///
    /// // Serialize to different targets
    /// Func&lt;Order, bool&gt; predicate = filter.ToLambda();
    /// string rql = filter.ToRql();                         // (eq(Status,'Active'),gt(Total,#100),in(Category,'Electronics','Books'))
    /// var (sql, parms) = filter.ToSqlWhere();              // Status = {0} and Total &gt; {1} and Category in ({2},{3})
    /// </code>
    /// </example>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are the intended fluent API entry points for this builder")]
    public class RqlFilterBuilder<TTarget> : AbstractFilterBuilder<RqlFilterBuilder<TTarget>>, IRqlFilter<TTarget> where TTarget : class
    {

        /// <summary>Creates a new empty filter builder instance.</summary>
        /// <returns>A new <see cref="RqlFilterBuilder{TTarget}"/> with no predicates.</returns>
        public static RqlFilterBuilder<TTarget> Create()
        {
            return new RqlFilterBuilder<TTarget>();
        }


        /// <summary>
        /// Begins building a filter by selecting the first property to constrain.
        /// </summary>
        /// <typeparam name="TValue">The type of the selected property.</typeparam>
        /// <param name="prop">An expression selecting the property to filter on.</param>
        /// <returns>The builder, ready to receive an operator (e.g. <c>.Equals()</c>, <c>.GreaterThan()</c>).</returns>
        public static RqlFilterBuilder<TTarget> Where<TValue>(Expression<Func<TTarget, TValue>> prop)
        {
            var builder = new RqlFilterBuilder<TTarget>().And(prop);
            return builder;
        }

        /// <summary>Creates a filter builder with no predicates, representing an unfiltered query.</summary>
        /// <returns>A new <see cref="RqlFilterBuilder{TTarget}"/> with no predicates.</returns>
        public static RqlFilterBuilder<TTarget> All()
        {
            var builder = new RqlFilterBuilder<TTarget>();
            return builder;
        }



        /// <summary>
        /// Initializes a new empty <see cref="RqlFilterBuilder{TTarget}"/>.
        /// </summary>
        protected RqlFilterBuilder()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="RqlFilterBuilder{TTarget}"/> from an existing <see cref="RqlTree"/>.
        /// </summary>
        /// <param name="tree">The parsed RQL tree whose predicates seed this builder.</param>
        public RqlFilterBuilder(RqlTree tree) : base(tree)
        {

        }


        /// <summary>
        /// Selects the next property to constrain, continuing the fluent chain with <c>And</c>.
        /// </summary>
        /// <typeparam name="TValue">The type of the selected property.</typeparam>
        /// <param name="prop">An expression selecting the property to filter on.</param>
        /// <returns>The builder, ready to receive an operator.</returns>
        public RqlFilterBuilder<TTarget> And<TValue>(Expression<Func<TTarget, TValue>> prop)
        {

            Guard.IsNotNull(prop);
            Guard.IsNotNull(prop.Body);

            if (prop.Body is not MemberExpression propExpr)
                throw new ArgumentException("Targets of a builder must be a field or a property on the output model", nameof(prop));

            CurrentName = propExpr.Member.Name;

            return this;

        }


        /// <inheritdoc />
        public override Type Target => typeof(TTarget);



    }


    /// <summary>
    /// Untyped RQL filter builder for dynamic scenarios where the target entity type is not known at compile time.
    /// Use <c>Where(propertyName)</c> to begin building predicates.
    /// </summary>
    public class RqlFilterBuilder : AbstractFilterBuilder<RqlFilterBuilder>
    {


        /// <summary>Creates a new empty untyped filter builder instance.</summary>
        /// <returns>A new <see cref="RqlFilterBuilder"/> with no predicates.</returns>
        public static RqlFilterBuilder Create()
        {
            return new RqlFilterBuilder();
        }


        /// <summary>Creates an untyped filter builder with no predicates, representing an unfiltered query.</summary>
        /// <returns>A new <see cref="RqlFilterBuilder"/> with no predicates.</returns>
        public static RqlFilterBuilder All()
        {
            var builder = new RqlFilterBuilder();
            return builder;
        }



        /// <summary>
        /// Begins building an untyped filter by specifying the first property name to constrain.
        /// </summary>
        /// <param name="prop">The property name to filter on.</param>
        /// <returns>The builder, ready to receive an operator (e.g. <c>.Equals()</c>, <c>.GreaterThan()</c>).</returns>
        public static RqlFilterBuilder Where(string prop)
        {

            Guard.IsNotNullOrWhiteSpace(prop);

            var builder = new RqlFilterBuilder().And(prop);
            return builder;
        }


        /// <summary>
        /// Initializes a new empty <see cref="RqlFilterBuilder"/>.
        /// </summary>
        protected RqlFilterBuilder()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="RqlFilterBuilder"/> from an existing <see cref="RqlTree"/>.
        /// </summary>
        /// <param name="tree">The parsed RQL tree whose predicates seed this builder.</param>
        public RqlFilterBuilder(RqlTree tree) : base(tree)
        {

        }


        /// <inheritdoc />
        public override Type Target => typeof(RqlFilterBuilder);

        /// <summary>Returns <c>true</c> if no predicates have been added, indicating an unfiltered (select-all) query.</summary>
        public bool IsAll => !HasCriteria;


        /// <summary>
        /// Selects the next property to constrain by name, continuing the fluent chain with <c>And</c>.
        /// </summary>
        /// <param name="prop">The property name to filter on.</param>
        /// <returns>The builder, ready to receive an operator.</returns>
        public RqlFilterBuilder And(string prop)
        {

            Guard.IsNotNullOrWhiteSpace(prop);

            CurrentName = prop;

            return this;

        }


    }


}
