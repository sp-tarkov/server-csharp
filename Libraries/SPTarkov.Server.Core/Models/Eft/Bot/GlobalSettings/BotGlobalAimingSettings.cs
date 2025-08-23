using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalAimingSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalAimingSettings
{
    [JsonPropertyName("MAX_AIM_PRECICING")]
    public float? MaxAimPrecicing { get; set; }

    [JsonPropertyName("BETTER_PRECICING_COEF")]
    public float? BetterPrecicingCoef { get; set; }

    [JsonPropertyName("RECLC_Y_DIST")]
    public float? ReclcYDist { get; set; }

    [JsonPropertyName("RECALC_DIST")]
    public float? RecalcDist { get; set; }

    [JsonPropertyName("RECALC_SQR_DIST")]
    public float? RecalcSqrDist { get; set; }

    [JsonPropertyName("COEF_FROM_COVER")]
    public float? CoefFromCover { get; set; }

    [JsonPropertyName("PANIC_COEF")]
    public float? PanicCoef { get; set; }

    [JsonPropertyName("PANIC_ACCURATY_COEF")]
    public float? PanicAccuratyCoef { get; set; }

    [JsonPropertyName("HARD_AIM")]
    public float? HardAim { get; set; }

    [JsonPropertyName("HARD_AIM_CHANCE_100")]
    public int? HardAimChance100 { get; set; }

    [JsonPropertyName("PANIC_TIME")]
    public float? PanicTime { get; set; }

    [JsonPropertyName("RECALC_MUST_TIME")]
    public int? RecalcMustTime { get; set; }

    [JsonPropertyName("RECALC_MUST_TIME_MIN")]
    public int? RecalcMustTimeMin { get; set; }

    [JsonPropertyName("RECALC_MUST_TIME_MAX")]
    public int? RecalcMustTimeMax { get; set; }

    [JsonPropertyName("DAMAGE_PANIC_TIME")]
    public float? DamagePanicTime { get; set; }

    [JsonPropertyName("DANGER_UP_POINT")]
    public float? DangerUpPoint { get; set; }

    [JsonPropertyName("MAX_AIMING_UPGRADE_BY_TIME")]
    public float? MaxAimingUpgradeByTime { get; set; }

    [JsonPropertyName("DAMAGE_TO_DISCARD_AIM_0_100")]
    public float? DamageToDiscardAim0100 { get; set; }

    [JsonPropertyName("MIN_TIME_DISCARD_AIM_SEC")]
    public float? MinTimeDiscardAimSec { get; set; }

    [JsonPropertyName("MAX_TIME_DISCARD_AIM_SEC")]
    public float? MaxTimeDiscardAimSec { get; set; }

    [JsonPropertyName("XZ_COEF")]
    public float? XzCoef { get; set; }

    [JsonPropertyName("XZ_COEF_STATIONARY_BULLET")]
    public float? XzCoefStationaryBullet { get; set; }

    [JsonPropertyName("XZ_COEF_STATIONARY_GRENADE")]
    public float? XzCoefStationaryGrenade { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_PRIORITY")]
    public int? ShootToChangePriority { get; set; }

    [JsonPropertyName("BOTTOM_COEF")]
    public float? BottomCoef { get; set; }

    [JsonPropertyName("FIRST_CONTACT_ADD_SEC")]
    public float? FirstContactAddSec { get; set; }

    [JsonPropertyName("FIRST_CONTACT_ADD_CHANCE_100")]
    public float? FirstContactAddChance100 { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_DELAY_SEC")]
    public float? BaseHitAffectionDelaySec { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_MIN_ANG")]
    public float? BaseHitAffectionMinAng { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_MAX_ANG")]
    public float? BaseHitAffectionMaxAng { get; set; }

    [JsonPropertyName("BASE_SHIEF")]
    public float? BaseShief { get; set; }

    [JsonPropertyName("BASE_SHIEF_STATIONARY_BULLET")]
    public float? BaseShiefStationaryBullet { get; set; }

    [JsonPropertyName("BASE_SHIEF_STATIONARY_GRENADE")]
    public float? BaseShiefStationaryGrenade { get; set; }

    [JsonPropertyName("SCATTERING_HAVE_DAMAGE_COEF")]
    public float? ScatteringHaveDamageCoef { get; set; }

    [JsonPropertyName("SCATTERING_DIST_MODIF")]
    public float? ScatteringDistModif { get; set; }

    [JsonPropertyName("SCATTERING_DIST_MODIF_CLOSE")]
    public float? ScatteringDistModifClose { get; set; }

    [JsonPropertyName("AIMING_TYPE")]
    public int? AimingType { get; set; }

    [JsonPropertyName("DIST_TO_SHOOT_TO_CENTER")]
    public float? DistToShootToCenter { get; set; }

    [JsonPropertyName("DIST_TO_SHOOT_NO_OFFSET")]
    public float? DistToShootNoOffset { get; set; }

    [JsonPropertyName("SHPERE_FRIENDY_FIRE_SIZE")]
    public float? ShpereFriendyFireSize { get; set; }

    [JsonPropertyName("COEF_IF_MOVE")]
    public float? CoefIfMove { get; set; }

    [JsonPropertyName("TIME_COEF_IF_MOVE")]
    public float? TimeCoefIfMove { get; set; }

    [JsonPropertyName("BOT_MOVE_IF_DELTA")]
    public float? BotMoveIfDelta { get; set; }

    [JsonPropertyName("NEXT_SHOT_MISS_CHANCE_100")]
    public float? NextShotMissChance100 { get; set; }

    [JsonPropertyName("NEXT_SHOT_MISS_Y_OFFSET")]
    public float? NextShotMissYOffset { get; set; }

    [JsonPropertyName("ANYTIME_LIGHT_WHEN_AIM_100")]
    public float? AnytimeLightWhenAim100 { get; set; }

    [JsonPropertyName("ANY_PART_SHOOT_TIME")]
    public float? AnyPartShootTime { get; set; }

    [JsonPropertyName("WEAPON_ROOT_OFFSET")]
    public float? WeaponRootOffset { get; set; }

    [JsonPropertyName("MIN_DAMAGE_TO_GET_HIT_AFFETS")]
    public float? MinDamageToGetHitAffets { get; set; }

    [JsonPropertyName("MAX_AIM_TIME")]
    public float? MaxAimTime { get; set; }

    [JsonPropertyName("OFFSET_RECAL_ANYWAY_TIME")]
    public float? OffsetRecalAnywayTime { get; set; }

    [JsonPropertyName("Y_TOP_OFFSET_COEF")]
    public float? YTopOffsetCoef { get; set; }

    [JsonPropertyName("Y_BOTTOM_OFFSET_COEF")]
    public float? YBottomOffsetCoef { get; set; }

    [JsonPropertyName("STATIONARY_LEAVE_HALF_DEGREE")]
    public float? StationaryLeaveHalfDegree { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MIN")]
    public int? BadShootsMin { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MAX")]
    public int? BadShootsMax { get; set; }

    [JsonPropertyName("BAD_SHOOTS_OFFSET")]
    public float? BadShootsOffset { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MAIN_COEF")]
    public float? BadShootsMainCoef { get; set; }

    [JsonPropertyName("START_TIME_COEF")]
    public float? StartTimeCoef { get; set; }

    [JsonPropertyName("AIMING_ON_WAY")]
    public float? AimingOnWay { get; set; }

    [JsonPropertyName("FIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_DISTANCE")]
    public float? FirstContactHardToSeeMissShootsDistance { get; set; }

    [JsonPropertyName("FIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_COUNT")]
    public int? FirstContactHardToSeeMissShootsCount { get; set; }

    [JsonPropertyName("MISS_FIRST_SOOTS")]
    public int? MissFirstSoots { get; set; }

    [JsonPropertyName("MISS_ON_START")]
    public int? MissOnStart { get; set; }

    [JsonPropertyName("MISS_DIST")]
    public float? MissDist { get; set; }

    [JsonPropertyName("UnderbarrelLauncherAiming")]
    public BotUnderbarrelLauncherAimingSettings? UnderbarrelLauncherAiming { get; set; }
}

/// <summary>
/// See BotUnderbarrelLauncherAimingSettings in the client, this record should match that
/// </summary>
public record BotUnderbarrelLauncherAimingSettings
{
    [JsonPropertyName("AIMING_ON_WAY")]
    public float? AimingOnWay { get; set; }

    [JsonPropertyName("ANYTIME_LIGHT_WHEN_AIM_100")]
    public float? AnytimeLightWhenAim100 { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MIN")]
    public int? BadShootsMin { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MAX")]
    public int? BadShootsMax { get; set; }

    [JsonPropertyName("START_TIME_COEF")]
    public float? StartTimeCoef { get; set; }

    [JsonPropertyName("DAMAGE_TO_DISCARD_AIM_0_100")]
    public float? DamageToDiscardAim0100 { get; set; }

    [JsonPropertyName("MIN_TIME_DISCARD_AIM_SEC")]
    public float? MinTimeDiscardAimSec { get; set; }

    [JsonPropertyName("MAX_TIME_DISCARD_AIM_SEC")]
    public float? MaxTimeDiscardAimSec { get; set; }

    [JsonPropertyName("MAX_AIM_PRECICING")]
    public float? MaxAimPrecicing { get; set; }

    [JsonPropertyName("MAX_AIMING_UPGRADE_BY_TIME")]
    public float? MaxAimingUpgradeByTime { get; set; }

    [JsonPropertyName("BOT_MOVE_IF_DELTA")]
    public float? BotMoveIfDelta { get; set; }

    [JsonPropertyName("PANIC_TIME")]
    public float? PanicTime { get; set; }

    [JsonPropertyName("RECALC_MUST_TIME_MIN")]
    public int? RecalcMustTimeMin { get; set; }

    [JsonPropertyName("RECALC_MUST_TIME_MAX")]
    public int? RecalcMustTimeMax { get; set; }

    [JsonPropertyName("RECLC_Y_DIST")]
    public float? ReclcYDist { get; set; }

    [JsonPropertyName("RECALC_SQR_DIST")]
    public float? RecalcSqrDist { get; set; }

    [JsonPropertyName("TIME_COEF_IF_MOVE")]
    public float? TimeCoefIfMove { get; set; }

    [JsonPropertyName("PANIC_COEF")]
    public float? PanicCoef { get; set; }

    [JsonPropertyName("COEF_FROM_COVER")]
    public float? CoefFromCover { get; set; }

    [JsonPropertyName("BOTTOM_COEF")]
    public float? BottomCoef { get; set; }

    [JsonPropertyName("MAX_AIM_TIME")]
    public float? MaxAimTime { get; set; }

    [JsonPropertyName("SCATTERING_DIST_MODIF")]
    public float? ScatteringDistModif { get; set; }

    [JsonPropertyName("SCATTERING_DIST_MODIF_CLOSE")]
    public float? ScatteringDistModifClose { get; set; }

    [JsonPropertyName("DIST_TO_SHOOT_NO_OFFSET")]
    public float? DistToShootNoOffset { get; set; }

    [JsonPropertyName("PANIC_ACCURATY_COEF")]
    public float? PanicAccuratyCoef { get; set; }

    [JsonPropertyName("HARD_AIM")]
    public float? HardAim { get; set; }

    [JsonPropertyName("COEF_IF_MOVE")]
    public float? CoefIfMove { get; set; }

    [JsonPropertyName("Y_TOP_OFFSET_COEF")]
    public float? YTopOffsetCoef { get; set; }

    [JsonPropertyName("Y_BOTTOM_OFFSET_COEF")]
    public float? YBottomOffsetCoef { get; set; }

    [JsonPropertyName("NEXT_SHOT_MISS_Y_OFFSET")]
    public float? NextShotMissYOffset { get; set; }

    [JsonPropertyName("BAD_SHOOTS_OFFSET")]
    public float? BadShootsOffset { get; set; }

    [JsonPropertyName("BAD_SHOOTS_MAIN_COEF")]
    public float? BadShootsMainCoef { get; set; }

    [JsonPropertyName("OFFSET_RECAL_ANYWAY_TIME")]
    public float? OffsetRecalAnywayTime { get; set; }
}
