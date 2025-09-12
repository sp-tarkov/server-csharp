using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See GClass611 (To be remapped to BotGlobalsCoreSettingsClass) in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalCoreSettings
{
    [JsonPropertyName("VisibleAngle")]
    public float? VisibleAngle { get; set; }

    [JsonPropertyName("VisibleDistance")]
    public float? VisibleDistance { get; set; }

    [JsonPropertyName("ScatteringPerMeter")]
    public float? ScatteringPerMeter { get; set; }

    [JsonPropertyName("ScatteringClosePerMeter")]
    public float? ScatteringClosePerMeter { get; set; }

    [JsonPropertyName("DamageCoeff")]
    public float? DamageCoeff { get; set; }

    [JsonPropertyName("HearingSense")]
    public float? HearingSense { get; set; }

    [JsonPropertyName("CanRun")]
    public bool? CanRun { get; set; }

    [JsonPropertyName("CanGrenade")]
    public bool? CanGrenade { get; set; }

    [JsonPropertyName("AimingType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EAimingType? AimingType { get; set; }

    [JsonPropertyName("PistolFireDistancePref")]
    public float? PistolFireDistancePref { get; set; }

    [JsonPropertyName("ShotgunFireDistancePref")]
    public float? ShotgunFireDistancePref { get; set; }

    [JsonPropertyName("RifleFireDistancePref")]
    public float? RifleFireDistancePref { get; set; }

    [JsonPropertyName("AccuratySpeed")]
    public float? AccuratySpeed { get; set; }

    [JsonPropertyName("WaitInCoverBetweenShotsSec")]
    public float? WaitInCoverBetweenShotsSec { get; set; }

    public enum EAimingType
    {
        normal,
        regular,
    }
}
