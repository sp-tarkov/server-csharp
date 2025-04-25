using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LowEdgeHealth
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay
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

    [JsonPropertyName("StartCommonHealth")]
    public double? StartCommonHealth
    {
        get;
        set;
    }
}
