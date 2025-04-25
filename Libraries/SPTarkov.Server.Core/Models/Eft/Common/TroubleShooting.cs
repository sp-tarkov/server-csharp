using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record TroubleShooting
{
    [JsonPropertyName("MalfRepairSpeedBonusPerLevel")]
    public double? MalfRepairSpeedBonusPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("SkillPointsPerMalfFix")]
    public double? SkillPointsPerMalfFix
    {
        get;
        set;
    }

    [JsonPropertyName("EliteDurabilityChanceReduceMult")]
    public double? EliteDurabilityChanceReduceMult
    {
        get;
        set;
    }

    [JsonPropertyName("EliteAmmoChanceReduceMult")]
    public double? EliteAmmoChanceReduceMult
    {
        get;
        set;
    }

    [JsonPropertyName("EliteMagChanceReduceMult")]
    public double? EliteMagChanceReduceMult
    {
        get;
        set;
    }
}
