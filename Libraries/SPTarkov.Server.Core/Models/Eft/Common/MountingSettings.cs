using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MountingSettings
{
    [JsonPropertyName("MovementSettings")]
    public MountingMovementSettings? MovementSettings
    {
        get;
        set;
    }

    [JsonPropertyName("PointDetectionSettings")]
    public MountingPointDetectionSettings? PointDetectionSettings
    {
        get;
        set;
    }
}
