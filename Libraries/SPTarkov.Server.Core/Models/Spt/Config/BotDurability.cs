using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record BotDurability
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("default")]
    public required DefaultDurability Default
    {
        get;
        set;
    }

    [JsonPropertyName("botDurabilities")]
    public required Dictionary<string, DefaultDurability> BotDurabilities
    {
        get;
        set;
    }

    [JsonPropertyName("pmc")]
    public required PmcDurability Pmc
    {
        get;
        set;
    }
}

/// <summary>
///     Durability values to be used when a more specific bot type can't be found
/// </summary>
public record DefaultDurability
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("armor")]
    public required ArmorDurability Armor
    {
        get;
        set;
    }

    [JsonPropertyName("weapon")]
    public required WeaponDurability Weapon
    {
        get;
        set;
    }
}

public record PmcDurability
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("armor")]
    public required PmcDurabilityArmor Armor
    {
        get;
        set;
    }

    [JsonPropertyName("weapon")]
    public required WeaponDurability Weapon
    {
        get;
        set;
    }
}

public record PmcDurabilityArmor
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("lowestMaxPercent")]
    public int LowestMaxPercent
    {
        get;
        set;
    }

    [JsonPropertyName("highestMaxPercent")]
    public int HighestMaxPercent
    {
        get;
        set;
    }

    [JsonPropertyName("maxDelta")]
    public int MaxDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minDelta")]
    public int MinDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minLimitPercent")]
    public int MinLimitPercent
    {
        get;
        set;
    }
}

public record ArmorDurability
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("maxDelta")]
    public int MaxDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minDelta")]
    public int MinDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minLimitPercent")]
    public int MinLimitPercent
    {
        get;
        set;
    }

    [JsonPropertyName("lowestMaxPercent")]
    public int LowestMaxPercent
    {
        get;
        set;
    }

    [JsonPropertyName("highestMaxPercent")]
    public int HighestMaxPercent
    {
        get;
        set;
    }
}

public record WeaponDurability
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("lowestMax")]
    public int LowestMax
    {
        get;
        set;
    }

    [JsonPropertyName("highestMax")]
    public int HighestMax
    {
        get;
        set;
    }

    [JsonPropertyName("maxDelta")]
    public int MaxDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minDelta")]
    public int MinDelta
    {
        get;
        set;
    }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent
    {
        get;
        set;
    }
}
