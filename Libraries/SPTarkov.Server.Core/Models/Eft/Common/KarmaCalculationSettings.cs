using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record KarmaCalculationSettings
{
    [JsonPropertyName("defaultPveKarmaValue")]
    public double? DefaultPveKarmaValue
    {
        get;
        set;
    }

    [JsonPropertyName("enable")]
    public bool? Enable
    {
        get;
        set;
    }

    [JsonPropertyName("expireDaysAfterLastRaid")]
    public double? ExpireDaysAfterLastRaid
    {
        get;
        set;
    }

    [JsonPropertyName("maxKarmaThresholdPercentile")]
    public double? MaxKarmaThresholdPercentile
    {
        get;
        set;
    }

    [JsonPropertyName("minKarmaThresholdPercentile")]
    public double? MinKarmaThresholdPercentile
    {
        get;
        set;
    }

    [JsonPropertyName("minSurvivedRaidCount")]
    public double? MinSurvivedRaidCount
    {
        get;
        set;
    }
}
