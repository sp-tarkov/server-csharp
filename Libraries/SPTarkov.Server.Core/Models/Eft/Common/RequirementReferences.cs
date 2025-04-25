using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RequirementReferences
{
    [JsonPropertyName("Alpinist")]
    public List<Alpinist>? Alpinists
    {
        get;
        set;
    }
}
