using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalsGrenadeSettings in the client, this record should match that
/// </para>
/// 
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsGrenadeSettings
{
    [JsonPropertyName("DELTA_NEXT_ATTEMPT_FROM_COVER")]
    public float? DeltaNextAttemptFromCover { get; set; }

    [JsonPropertyName("DELTA_NEXT_ATTEMPT")]
    public float? DeltaNextAttempt { get; set; }

    [JsonPropertyName("MIN_DIST_NOT_TO_THROW")]
    public float? MinDistNotToThrow { get; set; }

    [JsonPropertyName("NEAR_DELTA_THROW_TIME_SEC")]
    public float? NearDeltaThrowTimeSec { get; set; }

    [JsonPropertyName("MIN_THROW_GRENADE_DIST")]
    public float? MinThrowGrenadeDist { get; set; }

    [JsonPropertyName("MIN_THROW_GRENADE_DIST_SQRT")]
    public float? MinThrowGrenadeDistSqrt { get; set; }

    [JsonPropertyName("MIN_DIST_NOT_TO_THROW_SQR")]
    public float? MinDistNotToThrowSqr { get; set; }

    [JsonPropertyName("RUN_AWAY")]
    public float? RunAway { get; set; }

    [JsonPropertyName("RUN_AWAY_SQR")]
    public float? RunAwaySqr { get; set; }

    [JsonPropertyName("ADD_GRENADE_AS_DANGER")]
    public float? AddGrenadeAsDanger { get; set; }

    [JsonPropertyName("ADD_GRENADE_AS_DANGER_SQR")]
    public float? AddGrenadeAsDangerSqr { get; set; }

    [JsonPropertyName("CHANCE_TO_NOTIFY_ENEMY_GR_100")]
    public float? ChanceToNotifyEnemyGr100 { get; set; }

    [JsonPropertyName("GrenadePerMeter")]
    public float? GrenadePerMeter { get; set; }

    [JsonPropertyName("REQUEST_DIST_MUST_THROW_SQRT")]
    public float? RequestDistMustThrowSqrt { get; set; }

    [JsonPropertyName("REQUEST_DIST_MUST_THROW")]
    public float? RequestDistMustThrow { get; set; }

    [JsonPropertyName("BEWARE_TYPE")]
    public int? BewareType { get; set; }

    [JsonPropertyName("SHOOT_TO_SMOKE_CHANCE_100")]
    public float? ShootToSmokeChance100 { get; set; }

    [JsonPropertyName("CHANCE_RUN_FLASHED_100")]
    public float? ChanceRunFlashed100 { get; set; }

    [JsonPropertyName("MAX_FLASHED_DIST_TO_SHOOT")]
    public float? MaxFlashedDistToShoot { get; set; }

    [JsonPropertyName("MAX_FLASHED_DIST_TO_SHOOT_SQRT")]
    public float? MaxFlashedDistToShootSqrt { get; set; }

    [JsonPropertyName("FLASH_GRENADE_TIME_COEF")]
    public float? FlashGrenadeTimeCoef { get; set; }

    [JsonPropertyName("SIZE_SPOTTED_COEF")]
    public float? SizeSpottedCoef { get; set; }

    [JsonPropertyName("BE_ATTENTION_COEF")]
    public float? BeAttentionCoef { get; set; }

    [JsonPropertyName("TIME_SHOOT_TO_FLASH")]
    public float? TimeShootToFlash { get; set; }

    [JsonPropertyName("CLOSE_TO_SMOKE_TO_SHOOT")]
    public float? CloseToSmokeToShoot { get; set; }

    [JsonPropertyName("CLOSE_TO_SMOKE_TO_SHOOT_SQRT")]
    public float? CloseToSmokeToShootSqrt { get; set; }

    [JsonPropertyName("CLOSE_TO_SMOKE_TIME_DELTA")]
    public float? CloseToSmokeTimeDelta { get; set; }

    [JsonPropertyName("SMOKE_CHECK_DELTA")]
    public float? SmokeCheckDelta { get; set; }

    [JsonPropertyName("DELTA_GRENADE_START_TIME")]
    public float? DeltaGrenadeStartTime { get; set; }

    [JsonPropertyName("AMBUSH_IF_SMOKE_IN_ZONE_100")]
    public float? AmbushIfSmokeInZone100 { get; set; }

    [JsonPropertyName("AMBUSH_IF_SMOKE_RETURN_TO_ATTACK_SEC")]
    public float? AmbushIfSmokeReturnToAttackSec { get; set; }

    [JsonPropertyName("NO_RUN_FROM_AI_GRENADES")]
    public bool? NoRunFromAiGrenades { get; set; }

    [JsonPropertyName("MAX_THROW_POWER")]
    public float? MaxThrowPower { get; set; }

    [JsonPropertyName("GrenadePrecision")]
    public float? GrenadePrecision { get; set; }

    [JsonPropertyName("GRENADE_PRECISION_PORTALS")]
    public float? GrenadePrecisionPortals { get; set; }

    [JsonPropertyName("STOP_WHEN_THROW_GRENADE")]
    public bool? StopWhenThrowGrenade { get; set; }

    [JsonPropertyName("WAIT_TIME_TURN_AWAY")]
    public float? WaitTimeTurnAway { get; set; }

    [JsonPropertyName("SMOKE_SUPPRESS_DELTA")]
    public float? SmokeSuppressDelta { get; set; }

    [JsonPropertyName("DAMAGE_GRENADE_SUPPRESS_DELTA")]
    public float? DamageGrenadeSuppressDelta { get; set; }

    [JsonPropertyName("STUN_SUPPRESS_DELTA")]
    public float? StunSuppressDelta { get; set; }

    [JsonPropertyName("CHEAT_START_GRENADE_PLACE")]
    public bool? CheatStartGrenadePlace { get; set; }

    [JsonPropertyName("CAN_THROW_STRAIGHT_CONTACT")]
    public bool? CanThrowStraightContact { get; set; }

    [JsonPropertyName("STRAIGHT_CONTACT_DELTA_SEC")]
    public float? StraightContactDeltaSec { get; set; }

    [JsonPropertyName("ANG_TYPE")]
    public int? AngType { get; set; }

    [JsonPropertyName("MIN_THROW_DIST_PERCENT_0_1")]
    public float? MinThrowDistPercent01 { get; set; }

    [JsonPropertyName("FLASH_MODIF_IS_NIGHTVISION")]
    public float? FlashModifIsNightvision { get; set; }

    [JsonPropertyName("FIRST_TIME_SEEN_DELTA_CAN_THROW")]
    public float? FirstTimeSeenDeltaCanThrow { get; set; }

    [JsonPropertyName("SHALL_GETUP")]
    public bool? ShallGetup { get; set; }

    [JsonPropertyName("CAN_LAY")]
    public bool? CanLay { get; set; }

    [JsonPropertyName("IGNORE_SMOKE_GRENADE")]
    public bool? IgnoreSmokeGrenade { get; set; }

    [JsonPropertyName("CAN_THROW_FROM_ANY_PLACE")]
    public bool? CanThrowFromAnyPlace { get; set; }
}
