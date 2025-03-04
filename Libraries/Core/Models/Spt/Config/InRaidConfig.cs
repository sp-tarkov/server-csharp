using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record InRaidConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-inraid";

    /// <summary>
    /// Overrides to apply to the pre-raid settings screen
    /// </summary>
    [JsonPropertyName("raidMenuSettings")]
    public RaidMenuSettings RaidMenuSettings
    {
        get;
        set;
    }

    /// <summary>
    /// What effects should be saved post-raid
    /// </summary>
    [JsonPropertyName("save")]
    public RaidSave Save
    {
        get;
        set;
    }

    /// <summary>
    /// Names of car extracts
    /// </summary>
    [JsonPropertyName("carExtracts")]
    public List<string> CarExtracts
    {
        get;
        set;
    }

    /// <summary>
    /// Names of coop extracts
    /// </summary>
    [JsonPropertyName("coopExtracts")]
    public List<string> CoopExtracts
    {
        get;
        set;
    }

    /// <summary>
    /// Fence rep gain from a single car extract
    /// </summary>
    [JsonPropertyName("carExtractBaseStandingGain")]
    public double CarExtractBaseStandingGain
    {
        get;
        set;
    }

    /// <summary>
    /// Fence rep gain from a single coop extract
    /// </summary>
    [JsonPropertyName("coopExtractBaseStandingGain")]
    public double CoopExtractBaseStandingGain
    {
        get;
        set;
    }

    /// <summary>
    /// Fence rep gain when successfully extracting as pscav
    /// </summary>
    [JsonPropertyName("scavExtractStandingGain")]
    public double ScavExtractStandingGain
    {
        get;
        set;
    }

    /// <summary>
    /// The likelihood of PMC eliminating a minimum of 2 scavs while you engage them as a pscav.
    /// </summary>
    [JsonPropertyName("pmcKillProbabilityForScavGain")]
    public double PmcKillProbabilityForScavGain
    {
        get;
        set;
    }

    /// <summary>
    /// On death should items in your secure keep their Find in raid status regardless of how you finished the raid
    /// </summary>
    [JsonPropertyName("keepFiRSecureContainerOnDeath")]
    public bool KeepFiRSecureContainerOnDeath
    {
        get;
        set;
    }

    /// <summary>
    /// If enabled always keep found in raid status on items
    /// </summary>
    [JsonPropertyName("alwaysKeepFoundInRaidOnRaidEnd")]
    public bool AlwaysKeepFoundInRaidOnRaidEnd
    {
        get;
        set;
    }

    /// <summary>
    /// Percentage chance a player scav hot is hostile to the player when scavving
    /// </summary>
    [JsonPropertyName("playerScavHostileChancePercent")]
    public double PlayerScavHostileChancePercent
    {
        get;
        set;
    }
}

public record RaidMenuSettings
{
    [JsonPropertyName("aiAmount")]
    public string AiAmount
    {
        get;
        set;
    }

    [JsonPropertyName("aiDifficulty")]
    public string AiDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("bossEnabled")]
    public bool BossEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("scavWars")]
    public bool ScavWars
    {
        get;
        set;
    }

    [JsonPropertyName("taggedAndCursed")]
    public bool TaggedAndCursed
    {
        get;
        set;
    }

    [JsonPropertyName("enablePve")]
    public bool EnablePve
    {
        get;
        set;
    }

    [JsonPropertyName("randomWeather")]
    public bool RandomWeather
    {
        get;
        set;
    }

    [JsonPropertyName("randomTime")]
    public bool RandomTime
    {
        get;
        set;
    }
}

public record RaidSave
{
    /// <summary>
    /// Should loot gained from raid be saved
    /// </summary>
    [JsonPropertyName("loot")]
    public bool Loot
    {
        get;
        set;
    }
}
