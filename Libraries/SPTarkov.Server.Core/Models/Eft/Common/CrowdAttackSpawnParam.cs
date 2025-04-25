using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record CrowdAttackSpawnParam
{
    [JsonPropertyName("Difficulty")]
    public string? Difficulty
    {
        get;
        set;
    }

    [JsonPropertyName("Role")]
    public string? Role
    {
        get;
        set;
    }

    [JsonPropertyName("Weight")]
    public int? Weight
    {
        get;
        set;
    }
}
