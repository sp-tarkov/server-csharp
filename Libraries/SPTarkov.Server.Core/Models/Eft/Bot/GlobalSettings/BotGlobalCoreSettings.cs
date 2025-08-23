using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// See GClass611 (To be remapped to BotGlobalsCoreSettingsClass) in the client, this record should match that
/// </summary>
public record BotGlobalCoreSettings
{
    [JsonPropertyName("VisibleAngle")]
    public float VisibleAngle { get; set; }

    [JsonPropertyName("VisibleDistance")]
    public float VisibleDistance { get; set; }

    [JsonPropertyName("GainSightCoef")]
    public float GainSightCoef { get; set; }

    [JsonPropertyName("ScatteringPerMeter")]
    public float ScatteringPerMeter { get; set; }

    [JsonPropertyName("ScatteringClosePerMeter")]
    public float ScatteringClosePerMeter { get; set; }

    [JsonPropertyName("DamageCoeff")]
    public float DamageCoeff { get; set; }

    [JsonPropertyName("HearingSense")]
    public float HearingSense { get; set; }

    [JsonPropertyName("CanRun")]
    public bool CanRun { get; set; }

    [JsonPropertyName("CanGrenade")]
    public bool CanGrenade { get; set; }

    [JsonPropertyName("AimingType")]
    public EAimingType AimingType { get; set; }

    [JsonPropertyName("PistolFireDistancePref")]
    public float PistolFireDistancePref { get; set; }

    [JsonPropertyName("ShotgunFireDistancePref")]
    public float ShotgunFireDistancePref { get; set; }

    [JsonPropertyName("RifleFireDistancePref")]
    public float RifleFireDistancePref { get; set; }

    [JsonPropertyName("AccuratySpeed")]
    public float AccuratySpeed { get; set; }

    [JsonPropertyName("WaitInCoverBetweenShotsSec")]
    public float WaitInCoverBetweenShotsSec { get; set; }

    public enum EAimingType
    {
        normal,
        regular,
    }
}
