using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HealthFactorSetting
{
    [JsonPropertyName("Minimum")]
    public double? Minimum
    {
        get;
        set;
    }

    [JsonPropertyName("Maximum")]
    public double? Maximum
    {
        get;
        set;
    }

    [JsonPropertyName("Default")]
    public double? Default
    {
        get;
        set;
    }
}
