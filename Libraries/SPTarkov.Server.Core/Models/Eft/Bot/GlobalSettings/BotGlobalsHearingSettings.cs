using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// See BotGlobasHearingSettings in the client (Yes, with the typo), this record should match that
/// </summary>
public record BotGlobalsHearingSettings
{
    [JsonPropertyName("BOT_CLOSE_PANIC_DIST")]
    public float BotClosePanicDist { get; set; }

    [JsonPropertyName("CHANCE_TO_HEAR_SIMPLE_SOUND_0_1")]
    public float ChanceToHearSimpleSound01 { get; set; }

    [JsonPropertyName("DISPERSION_COEF")]
    public float DispersionCoef { get; set; }

    [JsonPropertyName("DISPERSION_COEF_GUN")]
    public float DispersionCoefGun { get; set; }

    [JsonPropertyName("CLOSE_DIST")]
    public float CloseDist { get; set; }

    [JsonPropertyName("FAR_DIST")]
    public float FarDist { get; set; }

    [JsonPropertyName("SOUND_DIR_DEEFREE")]
    public float SoundDirDeefree { get; set; }

    [JsonPropertyName("DIST_PLACE_TO_FIND_POINT")]
    public float DistPlaceToFindPoint { get; set; }

    [JsonPropertyName("DEAD_BODY_SOUND_RAD")]
    public float DeadBodySoundRad { get; set; }

    [JsonPropertyName("LOOK_ONLY_DANGER")]
    public bool LookOnlyDanger { get; set; }

    [JsonPropertyName("RESET_TIMER_DIST")]
    public float ResetTimerDist { get; set; }

    [JsonPropertyName("HEAR_DELAY_WHEN_PEACE")]
    public float HearDelayWhenPeace { get; set; }

    [JsonPropertyName("HEAR_DELAY_WHEN_HAVE_SMT")]
    public float HearDelayWhenHaveSmt { get; set; }

    [JsonPropertyName("LOOK_ONLY_DANGER_DELTA")]
    public float LookOnlyDangerDelta { get; set; }

    [JsonPropertyName("ENEMY_SNIPER_SHOOT_DIST")]
    public float EnemySniperShootDist { get; set; }
}
