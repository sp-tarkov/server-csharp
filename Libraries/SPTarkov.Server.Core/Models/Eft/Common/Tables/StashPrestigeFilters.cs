using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record StashPrestigeFilters
{
    [JsonPropertyName("includedItems")]
    public List<string>? IncludedItems
    {
        get;
        set;
    }

    [JsonPropertyName("excludedItems")]
    public List<string>? ExcludedItems
    {
        get;
        set;
    }
}
