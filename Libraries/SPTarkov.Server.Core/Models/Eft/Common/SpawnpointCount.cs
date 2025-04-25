using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SpawnpointCount
{
    [JsonPropertyName("mean")]
    public double? Mean
    {
        get;
        set;
    }

    [JsonPropertyName("std")]
    public double? Std
    {
        get;
        set;
    }
}
