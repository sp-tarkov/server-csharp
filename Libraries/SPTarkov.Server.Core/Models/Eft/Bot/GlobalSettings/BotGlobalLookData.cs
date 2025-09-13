using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalLookData in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalLookData
{
    /// <summary>
    /// The lifetime of the point up to which the bot will pay attention to it with the standard inspection algorithm
    /// </summary>
    [JsonPropertyName("OLD_TIME_POINT")]
    public float? OldTimePoint { get; set; }

    [JsonPropertyName("WAIT_NEW_SENSOR")]
    public float? WaitNewSensor { get; set; }

    [JsonPropertyName("WAIT_NEW__LOOK_SENSOR")]
    public float? WaitNewLookSensor { get; set; }

    [JsonPropertyName("LOOK_AROUND_DELTA")]
    public float? LookAroundDelta { get; set; }

    [JsonPropertyName("MAX_VISION_GRASS_METERS")]
    public float? MaxVisionGrassMeters { get; set; }

    [JsonPropertyName("MAX_VISION_GRASS_METERS_FLARE")]
    public float? MaxVisionGrassMetersFlare { get; set; }

    [JsonPropertyName("MAX_VISION_GRASS_METERS_OPT")]
    public float? MaxVisionGrassMetersOpt { get; set; }

    [JsonPropertyName("MAX_VISION_GRASS_METERS_FLARE_OPT")]
    public float? MaxVisionGrassMetersFlareOpt { get; set; }

    [JsonPropertyName("LightOnVisionDistance")]
    public float? LightOnVisionDistance { get; set; }

    [JsonPropertyName("FAR_DISTANCE")]
    public float? FarDistance { get; set; }

    [JsonPropertyName("FarDeltaTimeSec")]
    public float? FarDeltaTimeSec { get; set; }

    [JsonPropertyName("MIDDLE_DIST")]
    public float? MiddleDist { get; set; }

    [JsonPropertyName("MiddleDeltaTimeSec")]
    public float? MiddleDeltaTimeSec { get; set; }

    [JsonPropertyName("CloseDeltaTimeSec")]
    public float? CloseDeltaTimeSec { get; set; }

    [JsonPropertyName("POSIBLE_VISION_SPACE")]
    public float? PosibleVisionSpace { get; set; }

    [JsonPropertyName("VISIBILITY_CHANGE_SPEED")]
    public float? VisibilityChangeSpeed { get; set; }

    [JsonPropertyName("VISIBILITY_CHANGE_DECREASE_SPEED")]
    public float? VisibilityChangeDecreaseSpeed { get; set; }

    [JsonPropertyName("BASE_RUNTIME_EFFECT_K")]
    public float? BaseRuntimeEffectK { get; set; }

    /// <summary>
    /// The maximum value of the coefficient of the speed of change of the visibility scale at a minimum distance. The higher it is, the faster the scale will increase near the enemy
    /// </summary>
    [JsonPropertyName("MIN_DISTANCE_VISIBILITY_CHANGE_SPEED_K")]
    public float? MinDistanceVisibilityChangeSpeedK { get; set; }

    [JsonPropertyName("MAX_DISTANCE_VISIBILITY_CHANGE_SPEED_K")]
    public float? MaxDistanceVisibilityChangeSpeedK { get; set; }

    [JsonPropertyName("GOAL_TO_FULL_DISSAPEAR")]
    public float? GoalToFullDissapear { get; set; }

    [JsonPropertyName("GOAL_TO_FULL_DISSAPEAR_GREEN")]
    public float? GoalToFullDissapearGreen { get; set; }

    [JsonPropertyName("GOAL_TO_FULL_DISSAPEAR_SHOOT")]
    public float? GoalToFullDissapearShoot { get; set; }

    [JsonPropertyName("BODY_DELTA_TIME_SEARCH_SEC")]
    public float? BodyDeltaTimeSearchSec { get; set; }

    [JsonPropertyName("COME_TO_BODY_DIST")]
    public float? ComeToBodyDist { get; set; }

    [JsonPropertyName("MARKSMAN_VISIBLE_DIST_COEF")]
    public float? MarksmanVisibleDistCoef { get; set; }

    [JsonPropertyName("VISIBLE_DISNACE_WITH_LIGHT")]
    public float? VisibleDisnaceWithLight { get; set; }

    [JsonPropertyName("ENEMY_LIGHT_ADD")]
    public float? EnemyLightAdd { get; set; }

    [JsonPropertyName("ENEMY_LIGHT_START_DIST")]
    public float? EnemyLightStartDist { get; set; }

    [JsonPropertyName("DIST_NOT_TO_IGNORE_WALL")]
    public float? DistNotToIgnoreWall { get; set; }

    [JsonPropertyName("DIST_CHECK_WALL")]
    public float? DistCheckWall { get; set; }

    [JsonPropertyName("LOOK_LAST_POSENEMY_IF_NO_DANGER_SEC")]
    public float? LookLastPosenemyIfNoDangerSec { get; set; }

    [JsonPropertyName("MIN_LOOK_AROUD_TIME")]
    public float? MinLookAroudTime { get; set; }

    [JsonPropertyName("LOOK_THROUGH_GRASS")]
    public bool? LookThroughGrass { get; set; }

    [JsonPropertyName("LOOK_THROUGH_GRASS_DIST_METERS")]
    public float? LookThroughGrassDistMeters { get; set; }

    [JsonPropertyName("SEC_REPEATED_SEEN")]
    public float? SecRepeatedSeen { get; set; }

    [JsonPropertyName("DIST_SQRT_REPEATED_SEEN")]
    public double? DistSqrtRepeatedSeen { get; set; }

    [JsonPropertyName("DIST_REPEATED_SEEN")]
    public double? DistRepeatedSeen { get; set; }

    [JsonPropertyName("COEF_REPEATED_SEEN")]
    public float? CoefRepeatedSeen { get; set; }

    [JsonPropertyName("MAX_DIST_CLAMP_TO_SEEN_SPEED")]
    public float? MaxDistClampToSeenSpeed { get; set; }

    [JsonPropertyName("NIGHT_VISION_ON")]
    public float? NightVisionOn { get; set; }

    [JsonPropertyName("NIGHT_VISION_OFF")]
    public float? NightVisionOff { get; set; }

    [JsonPropertyName("NIGHT_VISION_DIST")]
    public float? NightVisionDist { get; set; }

    [JsonPropertyName("VISIBLE_ANG_LIGHT")]
    public float? VisibleAngLight { get; set; }

    [JsonPropertyName("VISIBLE_ANG_NIGHTVISION")]
    public float? VisibleAngNightvision { get; set; }

    /// <summary>
    /// if the distance between players is less than X then grass and foliage are ignored at the Layers level
    /// </summary>
    [JsonPropertyName("NO_GREEN_DIST")]
    public float? NoGreenDist { get; set; }

    /// <summary>
    /// if the distance between players is less than X then grass is ignored at the Layers level
    /// </summary>
    [JsonPropertyName("NO_GRASS_DIST")]
    public float? NoGrassDist { get; set; }

    /// <summary>
    /// Coefficient applied to the speed of notice if the bot is inside the bush
    /// </summary>
    [JsonPropertyName("INSIDE_BUSH_COEF")]
    public float? InsideBushCoef { get; set; }

    /// <summary>
    /// what curve to use for vision range by time of day
    /// </summary>
    [JsonPropertyName("SELF_NIGHTVISION")]
    public bool? SelfNightvision { get; set; }

    [JsonPropertyName("FULL_SECTOR_VIEW")]
    public bool? FullSectorView { get; set; }

    /// <summary>
    /// How many seconds after being hit will you be able to see through the greenery?
    /// </summary>
    [JsonPropertyName("LOOK_THROUGH_PERIOD_BY_HIT")]
    public float? LookThroughPeriodByHit { get; set; }

    [JsonPropertyName("CHECK_HEAD_ANY_DIST")]
    public bool? CheckHeadAnyDist { get; set; }

    [JsonPropertyName("MIDDLE_DIST_CAN_SHOOT_HEAD")]
    public bool? MiddleDistCanShootHead { get; set; }

    [JsonPropertyName("CAN_USE_LIGHT")]
    public bool? CanUseLight { get; set; }

    [JsonPropertyName("RAIN_DEBUFF_MAXVISIBILITY_MULTIPLYER")]
    public float? RainDebuffMaxvisibilityMultiplyer { get; set; }

    [JsonPropertyName("FOG_DEBUFF_MAXVISIBILITY_MULTIPLYER")]
    public float? FogDebuffMaxvisibilityMultiplyer { get; set; }

    [JsonPropertyName("RAIN_DEBUFF_SEENCOEFF_MULTIPLYER")]
    public float? RainDebuffSeencoeffMultiplyer { get; set; }

    [JsonPropertyName("FOG_DEBUFF_SEENCOEFF_MULTIPLYER")]
    public float? FogDebuffSeencoeffMultiplyer { get; set; }

    [JsonPropertyName("SHOOT_FROM_EYES")]
    public bool? ShootFromEyes { get; set; }

    [JsonPropertyName("ANGLE_FOR_GETUP")]
    public float? AngleForGetup { get; set; }

    [JsonPropertyName("MINIMUM_VISIBLE_DIST")]
    public float? MinimumVisibleDist { get; set; }

    [JsonPropertyName("CAN_USE_STRIBOSCOPE")]
    public float? CanUseStriboscope { get; set; }

    [JsonPropertyName("LOOK_TO_ENEMY_TIME")]
    public float? LookToEnemyTime { get; set; }

    [JsonPropertyName("ANGLE_VISION_COEF_FILTER")]
    public float? AngleVisionCoefFilter { get; set; }

    [JsonPropertyName("HEAD_ROTATE_SPEED")]
    public float? HeadRotateSpeed { get; set; }

    [JsonPropertyName("VISIBILITY_LEVEL_TO_TURN_HEAD")]
    public float? VisibilityLevelToTurnHead { get; set; }
}
