using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Fence;

public record CreateFenceAssortsResult
{
    [JsonPropertyName("sptItems")]
    public List<List<Item>>? SptItems
    {
        get;
        set;
    }

    [JsonPropertyName("barter_scheme")]
    public Dictionary<string, List<List<BarterScheme>>>? BarterScheme
    {
        get;
        set;
    }

    [JsonPropertyName("loyal_level_items")]
    public Dictionary<string, int>? LoyalLevelItems
    {
        get;
        set;
    }
}
