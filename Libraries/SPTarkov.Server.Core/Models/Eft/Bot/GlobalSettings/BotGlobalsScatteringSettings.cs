using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

public record BotGlobalsScatteringSettings
{
    [JsonPropertyName("min_scatter")]
    public float MinScatter { get; set; }

    [JsonPropertyName("working_scatter")]
    public float WorkingScatter { get; set; }

    [JsonPropertyName("max_scatter")]
    public float MaxScatter { get; set; }

    [JsonPropertyName("scatter_speed_up")]
    public float SpeedUp { get; set; }

    [JsonPropertyName("scatter_speed_up_aim")]
    public float SpeedUpAim { get; set; }

    [JsonPropertyName("scatter_speed_down")]
    public float SpeedDown { get; set; }

    [JsonPropertyName("to_slow_bot_speed")]
    public float ToSlowBotSpeed { get; set; }

    [JsonPropertyName("to_low_bot_speed")]
    public float ToLowBotSpeed { get; set; }

    [JsonPropertyName("to_up_bot_speed")]
    public float ToUpBotSpeed { get; set; }

    [JsonPropertyName("moving_slow_coef")]
    public float MovingSlowCoef { get; set; }

    [JsonPropertyName("to_low_angular_speed")]
    public float ToLowBotAngularSpeed { get; set; }

    [JsonPropertyName("to_stop_angular_speed")]
    public float ToStopBotAngularSpeed { get; set; }

    [JsonPropertyName("scatter_from_shot")]
    public float FromShot { get; set; }

    [JsonPropertyName("tracer_coef")]
    public float TracerCoef { get; set; }

    [JsonPropertyName("hand_damage_scattering")]
    public float HandDamageScatteringMinMax { get; set; }

    [JsonPropertyName("hand_damage_accuracy_speed")]
    public float HandDamageAccuracySpeed { get; set; }

    [JsonPropertyName("blood_fall_coef")]
    public float BloodFall { get; set; }

    [JsonPropertyName("caution_threshold")]
    public float Caution { get; set; }

    [JsonPropertyName("to_caution_coef")]
    public float ToCaution { get; set; }

    [JsonPropertyName("recoil_control_coef_single")]
    public float RecoilControlCoefShootDone { get; set; }

    [JsonPropertyName("recoil_control_coef_auto")]
    public float RecoilControlCoefShootDoneAuto { get; set; }

    [JsonPropertyName("amplitude_factor")]
    public float AmplitudeFactor { get; set; }

    [JsonPropertyName("amplitude_speed")]
    public float AmplitudeSpeed { get; set; }

    [JsonPropertyName("dist_to_not_aim")]
    public float DistFromOldPointToNotAim { get; set; }

    [JsonPropertyName("dist_to_not_aim_sqrt")]
    public float DistFromOldPointToNotAimSqrt { get; set; }

    [JsonPropertyName("min_dist_to_shoot")]
    public float DistNotToShoot { get; set; }

    [JsonPropertyName("pose_change_coef")]
    public float PoseChangeCoef { get; set; }

    [JsonPropertyName("lay_factor")]
    public float LayFactor { get; set; }

    [JsonPropertyName("recoil_y_coef")]
    public float RecoilYCoef { get; set; }

    [JsonPropertyName("recoil_y_speed_down")]
    public float RecoilYCoefSpeedDown { get; set; }

    [JsonPropertyName("recoil_y_max")]
    public float RecoilYMax { get; set; }
}
