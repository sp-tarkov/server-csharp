using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Fence;

public record FenceAssortGenerationValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("normal")]
    public GenerationAssortValues? Normal
    {
        get;
        set;
    }

    [JsonPropertyName("discount")]
    public GenerationAssortValues? Discount
    {
        get;
        set;
    }
}

public record GenerationAssortValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("item")]
    public int? Item
    {
        get;
        set;
    }

    [JsonPropertyName("weaponPreset")]
    public int? WeaponPreset
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentPreset")]
    public int? EquipmentPreset
    {
        get;
        set;
    }
}
