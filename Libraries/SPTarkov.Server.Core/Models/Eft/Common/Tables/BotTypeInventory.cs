using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BotTypeInventory
{
    [JsonPropertyName("equipment")]
    public Dictionary<EquipmentSlots, Dictionary<string, double>>? Equipment
    {
        get;
        set;
    }

    public GlobalAmmo? Ammo
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public ItemPools? Items
    {
        get;
        set;
    }

    [JsonPropertyName("mods")]
    public GlobalMods? Mods
    {
        get;
        set;
    }
}
