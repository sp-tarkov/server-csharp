using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record InRaidConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-inraid";

    /** Overrides to apply to the pre-raid settings screen */
    [JsonPropertyName("raidMenuSettings")]
    public RaidMenuSettings RaidMenuSettings { get; set; }

    /** What effects should be saved post-raid */
    [JsonPropertyName("save")]
    public RaidSave Save { get; set; }

    /** Names of car extracts */
    [JsonPropertyName("carExtracts")]
    public List<string> CarExtracts { get; set; }

    /** Names of coop extracts */
    [JsonPropertyName("coopExtracts")]
    public List<string> CoopExtracts { get; set; }

    /** Fence rep gain from a single car extract */
    [JsonPropertyName("carExtractBaseStandingGain")]
    public double CarExtractBaseStandingGain { get; set; }

    /** Fence rep gain from a single coop extract */
    [JsonPropertyName("coopExtractBaseStandingGain")]
    public double CoopExtractBaseStandingGain { get; set; }

    /** Fence rep gain when successfully extracting as pscav */
    [JsonPropertyName("scavExtractStandingGain")]
    public double ScavExtractStandingGain { get; set; }

    /** The likelihood of PMC eliminating a minimum of 2 scavs while you engage them as a pscav. */
    [JsonPropertyName("pmcKillProbabilityForScavGain")]
    public double PmcKillProbabilityForScavGain { get; set; }

    /** On death should items in your secure keep their Find in raid status regardless of how you finished the raid */
    [JsonPropertyName("keepFiRSecureContainerOnDeath")]
    public bool KeepFiRSecureContainerOnDeath { get; set; }

    /** If enabled always keep found in raid status on items */
    [JsonPropertyName("alwaysKeepFoundInRaidonRaidEnd")]
    public bool AlwaysKeepFoundInRaidOnRaidEnd { get; set; }

    /** Percentage chance a player scav hot is hostile to the player when scavving */
    [JsonPropertyName("playerScavHostileChancePercent")]
    public double PlayerScavHostileChancePercent { get; set; }
}

public record RaidMenuSettings
{
    [JsonPropertyName("aiAmount")]
    public string AiAmount { get; set; }

    [JsonPropertyName("aiDifficulty")]
    public string AiDifficulty { get; set; }

    [JsonPropertyName("bossEnabled")]
    public bool BossEnabled { get; set; }

    [JsonPropertyName("scavWars")]
    public bool ScavWars { get; set; }

    [JsonPropertyName("taggedAndCursed")]
    public bool TaggedAndCursed { get; set; }

    [JsonPropertyName("enablePve")]
    public bool EnablePve { get; set; }

    [JsonPropertyName("randomWeather")]
    public bool RandomWeather { get; set; }

    [JsonPropertyName("randomTime")]
    public bool RandomTime { get; set; }
}

public record RaidSave
{
    /** Should loot gained from raid be saved */
    [JsonPropertyName("loot")]
    public bool Loot { get; set; }
}
