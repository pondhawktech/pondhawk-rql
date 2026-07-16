namespace Pondhawk.Rql.Builder;

/// <summary>
/// Root AST node produced by parsing an RQL criteria string. Contains a list of <see cref="IRqlPredicate"/> nodes.
/// </summary>
public class RqlTree
{
    /// <summary>Returns <c>true</c> if this tree contains at least one predicate.</summary>
    public bool HasCriteria => Criteria.Count > 0;

    /// <summary>The list of parsed predicates.</summary>
    public IList<IRqlPredicate> Criteria { get; } = [];

}
