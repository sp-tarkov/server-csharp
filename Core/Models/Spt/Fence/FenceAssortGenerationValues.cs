using System.Text.Json.Serialization;

namespace Types.Models.Spt.Fence;

public class FenceAssortGenerationValues
{
    [JsonPropertyName("normal")]
    public GenerationAssortValues Normal { get; set; }

    [JsonPropertyName("discount")]
    public GenerationAssortValues Discount { get; set; }
}

public class GenerationAssortValues
{
    [JsonPropertyName("item")]
    public int Item { get; set; }

    [JsonPropertyName("weaponPreset")]
    public int WeaponPreset { get; set; }

    [JsonPropertyName("equipmentPreset")]
    public int EquipmentPreset { get; set; }
}