using System.Text.Json.Serialization;
using Core.Models.Spt.Services;

namespace Core.Models.Eft.Location;

public class AirdropLootResult
{
    [JsonPropertyName("dropType")]
    public string? DropType { get; set; }

    [JsonPropertyName("loot")]
    public List<LootItem>? Loot { get; set; }
}
