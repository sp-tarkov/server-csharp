using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Pain
{
    [JsonPropertyName("TremorDelay")]
    public double? TremorDelay
    {
        get;
        set;
    }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience
    {
        get;
        set;
    }
}
