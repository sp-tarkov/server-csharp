using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BotPreset
{
    [JsonPropertyName("UseThis")]
    public bool? UseThis
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

    [JsonPropertyName("BotDifficulty")]
    public string? BotDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("VisibleAngle")]
    public double? VisibleAngle
    {
        get;
        set;
    }

    [JsonPropertyName("VisibleDistance")]
    public double? VisibleDistance
    {
        get;
        set;
    }

    [JsonPropertyName("ScatteringPerMeter")]
    public double? ScatteringPerMeter
    {
        get;
        set;
    }

    [JsonPropertyName("HearingSense")]
    public double? HearingSense
    {
        get;
        set;
    }

    [JsonPropertyName("SCATTERING_DIST_MODIF")]
    public double? ScatteringDistModif
    {
        get;
        set;
    }

    [JsonPropertyName("MAX_AIMING_UPGRADE_BY_TIME")]
    public double? MaxAimingUpgradeByTime
    {
        get;
        set;
    }

    [JsonPropertyName("FIRST_CONTACT_ADD_SEC")]
    public double? FirstContactAddSec
    {
        get;
        set;
    }

    [JsonPropertyName("COEF_IF_MOVE")]
    public double? CoefIfMove
    {
        get;
        set;
    }
}
