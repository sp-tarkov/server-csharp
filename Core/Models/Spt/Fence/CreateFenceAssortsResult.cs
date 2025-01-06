using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Types.Models.Spt.Fence;

public class CreateFenceAssortsResult
{
    [JsonPropertyName("sptItems")]
    public List<List<Item>> SptItems { get; set; }

    [JsonPropertyName("barter_scheme")]
    public Dictionary<string, List<List<BarterScheme>>> BarterScheme { get; set; }

    [JsonPropertyName("loyal_level_items")]
    public Dictionary<string, int> LoyalLevelItems { get; set; }
}