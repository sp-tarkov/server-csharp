using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ItemsBlacklist
{
    [JsonPropertyName("minPlayerLevel")]
    public int? MinPlayerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("itemIds")]
    public List<string>? ItemIds
    {
        get;
        set;
    }
}
