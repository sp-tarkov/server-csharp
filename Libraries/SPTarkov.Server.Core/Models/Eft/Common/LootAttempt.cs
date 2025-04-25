using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LootAttempt
{
    [JsonPropertyName("k_exp")]
    public double? ExperiencePoints
    {
        get;
        set;
    }
}
