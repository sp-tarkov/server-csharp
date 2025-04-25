using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SkillCounter
{
    [JsonPropertyName("divisor")]
    public double? Divisor
    {
        get;
        set;
    }

    [JsonPropertyName("points")]
    public double? Points
    {
        get;
        set;
    }
}
