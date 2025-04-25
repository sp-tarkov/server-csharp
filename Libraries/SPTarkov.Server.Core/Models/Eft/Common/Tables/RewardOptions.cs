using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RewardOptions
{
    [JsonPropertyName("itemsBlacklist")]
    public List<string>? ItemsBlacklist
    {
        get;
        set;
    }
}
