using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MatchEnd
{
    [JsonPropertyName("README")]
    public string? ReadMe
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("survived_exp_requirement")]
    public int? SurvivedExperienceRequirement
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("survived_seconds_requirement")]
    public int? SurvivedSecondsRequirement
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("survived_exp_reward")]
    public int? SurvivedExperienceReward
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("mia_exp_reward")]
    public int? MiaExperienceReward
    {
        get;
        set;
    }

    [JsonPropertyName("runner_exp_reward")]
    public int? RunnerExperienceReward
    {
        get;
        set;
    }

    [JsonPropertyName("leftMult")]
    public double? LeftMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("miaMult")]
    public double? MiaMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("survivedMult")]
    public double? SurvivedMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("runnerMult")]
    public double? RunnerMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("killedMult")]
    public double? KilledMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("transit_exp_reward")]
    public double? TransitExperienceReward
    {
        get;
        set;
    }

    [JsonPropertyName("transit_mult")]
    public List<Dictionary<string, double>>? TransitMultiplier
    {
        get;
        set;
    }
}
