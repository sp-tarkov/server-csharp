using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Spt.Services;

namespace SPTarkov.Server.Core.Models.Eft.Location;

public record AirdropLootResult
{
    [JsonPropertyName("dropType")]
    public string? DropType { get; set; }

    [JsonPropertyName("loot")]
    public List<LootItem>? Loot { get; set; }
}
