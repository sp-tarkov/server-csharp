using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MovementRolloffMultiplier
{
    [JsonPropertyName("MovementState")]
    public string? MovementState
    {
        get;
        set;
    }

    [JsonPropertyName("RolloffMultiplier")]
    public double? RolloffMultiplier
    {
        get;
        set;
    }
}
