using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record BotDurability
{
    [JsonPropertyName("default")]
    public DefaultDurability Default { get; set; }

    [JsonPropertyName("botDurabilities")]
    public Dictionary<string, DefaultDurability> BotDurabilities { get; set; }

    [JsonPropertyName("pmc")]
    public PmcDurability Pmc { get; set; }
}

/** Durability values to be used when a more specific bot type can't be found */
public record DefaultDurability
{
    [JsonPropertyName("armor")]
    public ArmorDurability Armor { get; set; }

    [JsonPropertyName("weapon")]
    public WeaponDurability Weapon { get; set; }
}

public record PmcDurability
{
    [JsonPropertyName("armor")]
    public PmcDurabilityArmor Armor { get; set; }

    [JsonPropertyName("weapon")]
    public WeaponDurability Weapon { get; set; }
}

public record PmcDurabilityArmor
{
    [JsonPropertyName("lowestMaxPercent")]
    public int LowestMaxPercent { get; set; }

    [JsonPropertyName("highestMaxPercent")]
    public int HighestMaxPercent { get; set; }

    [JsonPropertyName("maxDelta")]
    public int MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public int MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public int MinLimitPercent { get; set; }
}

public record ArmorDurability
{
    [JsonPropertyName("maxDelta")]
    public int MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public int MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public int MinLimitPercent { get; set; }

    [JsonPropertyName("lowestMaxPercent")]
    public int LowestMaxPercent { get; set; }

    [JsonPropertyName("highestMaxPercent")]
    public int HighestMaxPercent { get; set; }
}

public record WeaponDurability
{
    [JsonPropertyName("lowestMax")]
    public int LowestMax { get; set; }

    [JsonPropertyName("highestMax")]
    public int HighestMax { get; set; }

    [JsonPropertyName("maxDelta")]
    public int MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public int MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent { get; set; }
}
