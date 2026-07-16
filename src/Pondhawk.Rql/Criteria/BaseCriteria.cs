// ReSharper disable CollectionNeverUpdated.Local

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pondhawk.Rql.Criteria;

/// <summary>
/// Base class for criteria DTOs. Detects overposted properties via <see cref="System.Text.Json.Serialization.JsonExtensionDataAttribute"/>.
/// </summary>
public class BaseCriteria : ICriteria
{
    /// <inheritdoc />
    public string[]? Rql { get; set; }

    [JsonExtensionData]
    private Dictionary<string, JsonElement> Overposts { get; } = new(StringComparer.Ordinal);

    /// <summary>Returns <c>true</c> if the deserialized payload contained properties not defined on this criteria type.</summary>
    public bool IsOverposted() => Overposts.Count > 0;

    /// <summary>Returns the names of any properties that were present in the payload but not defined on this criteria type.</summary>
    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
