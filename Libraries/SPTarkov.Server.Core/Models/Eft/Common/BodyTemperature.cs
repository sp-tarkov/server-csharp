using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BodyTemperature
{
    [JsonPropertyName("DefaultBuildUpTime")]
    public double? DefaultBuildUpTime
    {
        get;
        set;
    }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime
    {
        get;
        set;
    }

    [JsonPropertyName("LoopTime")]
    public double? LoopTime
    {
        get;
        set;
    }
}
