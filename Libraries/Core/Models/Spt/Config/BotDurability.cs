using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record BotDurability
{
    [JsonPropertyName("default")]
    public DefaultDurability Default { get; set; }

    [JsonPropertyName("pmc")]
    public PmcDurability Pmc { get; set; }

    [JsonPropertyName("boss")]
    public PmcDurability Boss { get; set; }

    [JsonPropertyName("follower")]
    public PmcDurability Follower { get; set; }

    [JsonPropertyName("assault")]
    public PmcDurability Assault { get; set; }

    [JsonPropertyName("cursedassault")]
    public PmcDurability CursedAssault { get; set; }

    [JsonPropertyName("marksman")]
    public PmcDurability Marksman { get; set; }

    [JsonPropertyName("pmcbot")]
    public PmcDurability PmcBot { get; set; }

    [JsonPropertyName("arenafighterevent")]
    public PmcDurability ArenaFighterEvent { get; set; }

    [JsonPropertyName("arenafighter")]
    public PmcDurability ArenaFighter { get; set; }

    [JsonPropertyName("crazyassaultevent")]
    public PmcDurability CrazyAssaultEvent { get; set; }

    [JsonPropertyName("exusec")]
    public PmcDurability Exusec { get; set; }

    [JsonPropertyName("gifter")]
    public PmcDurability Gifter { get; set; }

    [JsonPropertyName("sectantpriest")]
    public PmcDurability SectantPriest { get; set; }

    [JsonPropertyName("sectantwarrior")]
    public PmcDurability SectantWarrior { get; set; }
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
    public double LowestMaxPercent { get; set; }

    [JsonPropertyName("highestMaxPercent")]
    public double HighestMaxPercent { get; set; }

    [JsonPropertyName("maxDelta")]
    public double MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public double MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent { get; set; }
}

public record ArmorDurability
{
    [JsonPropertyName("maxDelta")]
    public double MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public double MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent { get; set; }
}

public record WeaponDurability
{
    [JsonPropertyName("lowestMax")]
    public double LowestMax { get; set; }

    [JsonPropertyName("highestMax")]
    public double HighestMax { get; set; }

    [JsonPropertyName("maxDelta")]
    public double MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public double MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent { get; set; }
}
