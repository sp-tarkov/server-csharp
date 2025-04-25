using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record PrestigeSkillConfig
{
    [JsonPropertyName("transferMultiplier")]
    public double? TransferMultiplier
    {
        get;
        set;
    }
}
