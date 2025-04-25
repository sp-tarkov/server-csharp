using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Wound
{
    [JsonPropertyName("WorkingTime")]
    public double? WorkingTime
    {
        get;
        set;
    }

    [JsonPropertyName("ThresholdMin")]
    public double? ThresholdMin
    {
        get;
        set;
    }

    [JsonPropertyName("ThresholdMax")]
    public double? ThresholdMax
    {
        get;
        set;
    }
}
