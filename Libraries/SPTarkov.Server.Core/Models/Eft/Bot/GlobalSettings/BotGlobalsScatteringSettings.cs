using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalsScatteringSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsScatteringSettings
{
    [JsonPropertyName("MinScatter")]
    public float? MinScatter { get; set; }

    [JsonPropertyName("WorkingScatter")]
    public float? WorkingScatter { get; set; }

    [JsonPropertyName("MaxScatter")]
    public float? MaxScatter { get; set; }

    [JsonPropertyName("SpeedUp")]
    public float? SpeedUp { get; set; }

    [JsonPropertyName("SpeedUpAim")]
    public float? SpeedUpAim { get; set; }

    [JsonPropertyName("SpeedDown")]
    public float? SpeedDown { get; set; }

    [JsonPropertyName("ToSlowBotSpeed")]
    public float? ToSlowBotSpeed { get; set; }

    [JsonPropertyName("ToLowBotSpeed")]
    public float? ToLowBotSpeed { get; set; }

    [JsonPropertyName("ToUpBotSpeed")]
    public float? ToUpBotSpeed { get; set; }

    [JsonPropertyName("MovingSlowCoef")]
    public float? MovingSlowCoef { get; set; }

    [JsonPropertyName("ToLowBotAngularSpeed")]
    public float? ToLowBotAngularSpeed { get; set; }

    [JsonPropertyName("ToStopBotAngularSpeed")]
    public float? ToStopBotAngularSpeed { get; set; }

    /// <summary>
    /// Degrees\How much the bot's spread angle diverges when it is hit, multiplied by the damage
    /// </summary>
    [JsonPropertyName("FromShot")]
    public float? FromShot { get; set; }

    [JsonPropertyName("TracerCoef")]
    public float? TracerCoef { get; set; }

    [JsonPropertyName("HandDamageScatteringMinMax")]
    public float? HandDamageScatteringMinMax { get; set; }

    [JsonPropertyName("HandDamageAccuracySpeed")]
    public float? HandDamageAccuracySpeed { get; set; }

    /// <summary>
    /// Float\to Coefficient of change in working circle of accuracy during bleeding
    /// </summary>
    [JsonPropertyName("BloodFall")]
    public float? BloodFall { get; set; }

    [JsonPropertyName("Caution")]
    public float? Caution { get; set; }

    [JsonPropertyName("ToCaution")]
    public float? ToCaution { get; set; }

    [JsonPropertyName("RecoilControlCoefShootDone")]
    public float? RecoilControlCoefShootDone { get; set; }

    [JsonPropertyName("RecoilControlCoefShootDoneAuto")]
    public float? RecoilControlCoefShootDoneAuto { get; set; }

    [JsonPropertyName("AMPLITUDE_FACTOR")]
    public float? AmplitudeFactor { get; set; }

    [JsonPropertyName("AMPLITUDE_SPEED")]
    public float? AmplitudeSpeed { get; set; }

    [JsonPropertyName("DIST_FROM_OLD_POINT_TO_NOT_AIM")]
    public float? DistFromOldPointToNotAim { get; set; }

    [JsonPropertyName("DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT")]
    public float? DistFromOldPointToNotAimSqrt { get; set; }

    [JsonPropertyName("DIST_NOT_TO_SHOOT")]
    public float? DistNotToShoot { get; set; }

    [JsonPropertyName("PoseChnageCoef")]
    public float? PoseChangeCoef { get; set; }

    /// <summary>
    /// At the moment of changing the position to prone/non-prone, the current circle of convergence will increase by X
    /// </summary>
    [JsonPropertyName("LayFactor")]
    public float? LayFactor { get; set; }

    [JsonPropertyName("RecoilYCoef")]
    public float? RecoilYCoef { get; set; }

    [JsonPropertyName("RecoilYCoefSppedDown")]
    public float? RecoilYCoefSpeedDown { get; set; }

    [JsonPropertyName("RecoilYMax")]
    public float? RecoilYMax { get; set; }
}
