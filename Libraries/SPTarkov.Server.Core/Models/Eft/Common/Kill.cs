using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Kill
{
    [JsonPropertyName("combo")]
    public Combo[] Combos
    {
        get;
        set;
    }

    [JsonPropertyName("victimLevelExp")]
    public double? VictimLevelExperience
    {
        get;
        set;
    }

    [JsonPropertyName("headShotMult")]
    public double? HeadShotMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("expOnDamageAllHealth")]
    public double? ExperienceOnDamageAllHealth
    {
        get;
        set;
    }

    [JsonPropertyName("longShotDistance")]
    public double? LongShotDistance
    {
        get;
        set;
    }

    [JsonPropertyName("bloodLossToLitre")]
    public double? BloodLossToLitre
    {
        get;
        set;
    }

    [JsonPropertyName("botExpOnDamageAllHealth")]
    public double? BotExperienceOnDamageAllHealth
    {
        get;
        set;
    }

    [JsonPropertyName("botHeadShotMult")]
    public double? BotHeadShotMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("victimBotLevelExp")]
    public double? VictimBotLevelExperience
    {
        get;
        set;
    }

    [JsonPropertyName("pmcExpOnDamageAllHealth")]
    public double? PmcExperienceOnDamageAllHealth
    {
        get;
        set;
    }

    [JsonPropertyName("pmcHeadShotMult")]
    public double? PmcHeadShotMultiplier
    {
        get;
        set;
    }
}
