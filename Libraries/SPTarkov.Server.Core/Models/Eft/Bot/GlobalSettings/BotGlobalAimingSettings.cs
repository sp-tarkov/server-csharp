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
    /// <summary>
    /// Time for Maximum shooting improvement depending on how long the bot aims
    /// </summary>
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

    /// <summary>
    /// increased aiming when peeking out from behind cover
    /// </summary>
    [JsonPropertyName("COEF_FROM_COVER")]
    public float? CoefFromCover { get; set; }

    [JsonPropertyName("PANIC_COEF")]
    public float? PanicCoef { get; set; }

    [JsonPropertyName("PANIC_ACCURATY_COEF")]
    public float? PanicAccuratyCoef { get; set; }

    /// <summary>
    /// Improved Aiming Factor
    /// </summary>
    [JsonPropertyName("HARD_AIM")]
    public float? HardAim { get; set; }

    /// <summary>
    /// aim chance while shooting [0;100]
    /// </summary>
    [JsonPropertyName("HARD_AIM_CHANCE_100")]
    public int? HardAimChance100 { get; set; }

    /// <summary>
    /// Panic time is normal
    /// </summary>
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

    /// <summary>
    /// danger point firing level
    /// </summary>
    [JsonPropertyName("DANGER_UP_POINT")]
    public float? DangerUpPoint { get; set; }

    /// <summary>
    /// how much better can shooting be from zeroing in - 0.15 == 85%. 0.5 == 50% . 1 == 0%
    /// </summary>
    [JsonPropertyName("MAX_AIMING_UPGRADE_BY_TIME")]
    public float? MaxAimingUpgradeByTime { get; set; }

    /// <summary>
    /// this is the probability that the bot will mow down the shot when hit. The alternative is to worsen the aiming time
    /// </summary>
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

    /// <summary>
    /// How many shots on target are needed approximately to change the priority to shooting at legs
    /// </summary>
    [JsonPropertyName("SHOOT_TO_CHANGE_PRIORITY")]
    public int? ShootToChangePriority { get; set; }

    [JsonPropertyName("BOTTOM_COEF")]
    public float? BottomCoef { get; set; }

    /// <summary>
    /// Added to the first time a bot aims at a player
    /// </summary>
    [JsonPropertyName("FIRST_CONTACT_ADD_SEC")]
    public float? FirstContactAddSec { get; set; }

    /// <summary>
    /// Chance of triggering the delay specified in FIRST_CONTACT_ADD_SEC
    /// </summary>
    [JsonPropertyName("FIRST_CONTACT_ADD_CHANCE_100")]
    public float? FirstContactAddChance100 { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_DELAY_SEC")]
    public float? BaseHitAffectionDelaySec { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_MIN_ANG")]
    public float? BaseHitAffectionMinAng { get; set; }

    [JsonPropertyName("BASE_HIT_AFFECTION_MAX_ANG")]
    public float? BaseHitAffectionMaxAng { get; set; }

    /// <summary>
    /// Base shift in meters for aiming (example: BASE_SHIEF=5 => means at a distance of 20 meters the aiming will be as at 20+5=25)
    /// </summary>
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

    /// <summary>
    /// Chance that the bot will turn on the flashlight when aiming
    /// </summary>
    [JsonPropertyName("ANYTIME_LIGHT_WHEN_AIM_100")]
    public float? AnytimeLightWhenAim100 { get; set; }

    /// <summary>
    /// How many seconds after first spotting an enemy will it be possible to shoot at any part of the body?
    /// default 900
    /// </summary>
    [JsonPropertyName("ANY_PART_SHOOT_TIME")]
    public float? AnyPartShootTime { get; set; }

    [JsonPropertyName("WEAPON_ROOT_OFFSET")]
    public float? WeaponRootOffset { get; set; }

    [JsonPropertyName("MIN_DAMAGE_TO_GET_HIT_AFFETS")]
    public float? MinDamageToGetHitAffets { get; set; }

    /// <summary>
    /// Maximum aiming time
    /// </summary>
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

    /// <summary>
    /// Base number of hits past MIN
    /// </summary>
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

    /// <summary>
    /// The distance to the target, if exceeded, the bot misses on first contact if visibility is obstructed
    /// </summary>
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

    /// <summary>
    /// The bot is considered to be moving if it has passed more than X frame
    /// </summary>
    [JsonPropertyName("BOT_MOVE_IF_DELTA")]
    public float? BotMoveIfDelta { get; set; }

    /// <summary>
    /// Panic time is normal
    /// default 6
    /// </summary>
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

    /// <summary>
    /// Base coefficient from the formula == Y*ln(x/5+1.2)
    /// </summary>
    [JsonPropertyName("BAD_SHOOTS_MAIN_COEF")]
    public float? BadShootsMainCoef { get; set; }

    [JsonPropertyName("OFFSET_RECAL_ANYWAY_TIME")]
    public float? OffsetRecalAnywayTime { get; set; }
}
