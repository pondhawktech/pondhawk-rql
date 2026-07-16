namespace Pondhawk.Rql.Criteria
{

    /// <summary>
    /// Marker interface for criteria objects that can carry raw RQL strings and be introspected by the filter builder.
    /// </summary>
    public interface ICriteria
    {
        /// <summary>Optional raw RQL criteria strings.</summary>
        public string[]? Rql { get; }

    }


}
