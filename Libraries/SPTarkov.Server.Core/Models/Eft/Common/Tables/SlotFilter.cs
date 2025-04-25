using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record SlotFilter
{
    [JsonPropertyName("Shift")]
    public double? Shift
    {
        get;
        set;
    }

    [JsonPropertyName("locked")]
    public bool? Locked
    {
        get;
        set;
    }

    [JsonPropertyName("Plate")]
    public string? Plate
    {
        get;
        set;
    }

    [JsonPropertyName("armorColliders")]
    public List<string>? ArmorColliders
    {
        get;
        set;
    }

    [JsonPropertyName("armorPlateColliders")]
    public List<string>? ArmorPlateColliders
    {
        get;
        set;
    }

    [JsonPropertyName("Filter")]
    public HashSet<string>? Filter
    {
        get;
        set;
    }

    [JsonPropertyName("AnimationIndex")]
    public double? AnimationIndex
    {
        get;
        set;
    }

    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount
    {
        get;
        set;
    }

    [JsonPropertyName("bluntDamageReduceFromSoftArmor")]
    public bool? BluntDamageReduceFromSoftArmor
    {
        get;
        set;
    }
}
