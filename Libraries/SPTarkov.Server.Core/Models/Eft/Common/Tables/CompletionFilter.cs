using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CompletionFilter
{
    [JsonPropertyName("itemsBlacklist")]
    public List<ItemsBlacklist>? ItemsBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("itemsWhitelist")]
    public List<ItemsWhitelist>? ItemsWhitelist
    {
        get;
        set;
    }
}
