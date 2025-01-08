using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class BotDurability
{
    [JsonPropertyName("default")]
    public DefaultDurability Default { get; set; }

    [JsonPropertyName("pmc")]
    public PmcDurability Pmc { get; set; }

    [JsonPropertyName("boss")]
    public BotDurability Boss { get; set; }

    [JsonPropertyName("follower")]
    public BotDurability Follower { get; set; }

    [JsonPropertyName("assault")]
    public BotDurability Assault { get; set; }

    [JsonPropertyName("cursedassault")]
    public BotDurability CursedAssault { get; set; }

    [JsonPropertyName("marksman")]
    public BotDurability Marksman { get; set; }

    [JsonPropertyName("pmcbot")]
    public BotDurability PmcBot { get; set; }

    [JsonPropertyName("arenafighterevent")]
    public BotDurability ArenaFighterEvent { get; set; }

    [JsonPropertyName("arenafighter")]
    public BotDurability ArenaFighter { get; set; }

    [JsonPropertyName("crazyassaultevent")]
    public BotDurability CrazyAssaultEvent { get; set; }

    [JsonPropertyName("exusec")]
    public BotDurability Exusec { get; set; }

    [JsonPropertyName("gifter")]
    public BotDurability Gifter { get; set; }

    [JsonPropertyName("sectantpriest")]
    public BotDurability SectantPriest { get; set; }

    [JsonPropertyName("sectantwarrior")]
    public BotDurability SectantWarrior { get; set; }
}

/** Durability values to be used when a more specific bot type can't be found */
public class DefaultDurability
{
    [JsonPropertyName("armor")]
    public ArmorDurability Armor { get; set; }

    [JsonPropertyName("weapon")]
    public WeaponDurability Weapon { get; set; }
}

public class PmcDurability
{
    [JsonPropertyName("armor")]
    public PmcDurabilityArmor Armor { get; set; }

    [JsonPropertyName("weapon")]
    public WeaponDurability Weapon { get; set; }
}

public class PmcDurabilityArmor
{
    [JsonPropertyName("lowestMaxPercent")]
    public double LowestMaxPercent { get; set; }

    [JsonPropertyName("highestMaxPercent")]
    public double HighestMaxPercent { get; set; }

    [JsonPropertyName("maxDelta")]
    public double MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public double MinDelta { get; set; }
}

public class ArmorDurability
{
    [JsonPropertyName("maxDelta")]
    public double MaxDelta { get; set; }

    [JsonPropertyName("minDelta")]
    public double MinDelta { get; set; }

    [JsonPropertyName("minLimitPercent")]
    public double MinLimitPercent { get; set; }
}

public class WeaponDurability
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