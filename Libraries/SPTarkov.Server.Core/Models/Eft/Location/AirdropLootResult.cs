using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Spt.Services;

namespace SPTarkov.Server.Core.Models.Eft.Location;

public record AirdropLootResult
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("dropType")]
    public string? DropType
    {
        get;
        set;
    }

    [JsonPropertyName("loot")]
    public List<LootItem>? Loot
    {
        get;
        set;
    }
}
