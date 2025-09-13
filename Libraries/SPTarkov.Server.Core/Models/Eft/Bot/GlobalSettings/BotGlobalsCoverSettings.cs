using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalsCoverSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsCoverSettings
{
    [JsonPropertyName("RETURN_TO_ATTACK_AFTER_AMBUSH_MIN")]
    public float? ReturnToAttackAfterAmbushMin { get; set; }

    [JsonPropertyName("RETURN_TO_ATTACK_AFTER_AMBUSH_MAX")]
    public float? ReturnToAttackAfterAmbushMax { get; set; }

    [JsonPropertyName("SOUND_TO_GET_SPOTTED")]
    public float? SoundToGetSpotted { get; set; }

    [JsonPropertyName("TIME_TO_MOVE_TO_COVER")]
    public float? TimeToMoveToCover { get; set; }

    [JsonPropertyName("MAX_DIST_OF_COVER")]
    public float? MaxDistOfCover { get; set; }

    [JsonPropertyName("CHANGE_RUN_TO_COVER_SEC")]
    public float? ChangeRunToCoverSec { get; set; }

    [JsonPropertyName("CHANGE_RUN_TO_COVER_SEC_GREANDE")]
    public float? ChangeRunToCoverSecGreande { get; set; }

    [JsonPropertyName("MIN_DIST_TO_ENEMY")]
    public float? MinDistToEnemy { get; set; }

    [JsonPropertyName("DIST_CANT_CHANGE_WAY")]
    public float? DistCantChangeWay { get; set; }

    [JsonPropertyName("DIST_CHECK_SFETY")]
    public float? DistCheckSfety { get; set; }

    [JsonPropertyName("TIME_CHECK_SAFE")]
    public float? TimeCheckSafe { get; set; }

    [JsonPropertyName("HIDE_TO_COVER_TIME")]
    public float? HideToCoverTime { get; set; }

    [JsonPropertyName("MAX_DIST_OF_COVER_SQR")]
    public float? MaxDistOfCoverSqr { get; set; }

    [JsonPropertyName("DIST_CANT_CHANGE_WAY_SQR")]
    public float? DistCantChangeWaySqr { get; set; }

    [JsonPropertyName("SPOTTED_COVERS_RADIUS")]
    public float? SpottedCoversRadius { get; set; }

    [JsonPropertyName("LOOK_LAST_ENEMY_POS_MOVING")]
    public float? LookLastEnemyPosMoving { get; set; }

    [JsonPropertyName("LOOK_LAST_ENEMY_POS_LONG")]
    public float? LookLastEnemyPosLong { get; set; }

    [JsonPropertyName("LOOK_LAST_ENEMY_POS_DIST")]
    public float? LookLastEnemyPosDist { get; set; }

    [JsonPropertyName("LOOK_TO_HIT_POINT_IF_LAST_ENEMY")]
    public float? LookToHitPointIfLastEnemy { get; set; }

    [JsonPropertyName("LOOK_LAST_ENEMY_POS_LOOKAROUND")]
    public float? LookLastEnemyPosLookaround { get; set; }

    [JsonPropertyName("OFFSET_LOOK_ALONG_WALL_ANG")]
    public int? OffsetLookAlongWallAng { get; set; }

    [JsonPropertyName("SPOTTED_GRENADE_RADIUS")]
    public float? SpottedGrenadeRadius { get; set; }

    [JsonPropertyName("MAX_SPOTTED_TIME_SEC")]
    public float? MaxSpottedTimeSec { get; set; }

    [JsonPropertyName("WAIT_INT_COVER_FINDING_ENEMY")]
    public float? WaitIntCoverFindingEnemy { get; set; }

    [JsonPropertyName("CLOSE_DIST_POINT_SQRT")]
    public float? CloseDistPointSqrt { get; set; }

    [JsonPropertyName("DELTA_SEEN_FROM_COVE_LAST_POS")]
    public float? DeltaSeenFromCoveLastPos { get; set; }

    [JsonPropertyName("MOVE_TO_COVER_WHEN_TARGET")]
    public bool? MoveToCoverWhenTarget { get; set; }

    [JsonPropertyName("RUN_COVER_IF_CAN_AND_NO_ENEMIES")]
    public bool? RunCoverIfCanAndNoEnemies { get; set; }

    [JsonPropertyName("SPOTTED_GRENADE_TIME")]
    public float? SpottedGrenadeTime { get; set; }

    [JsonPropertyName("DEPENDS_Y_DIST_TO_BOT")]
    public bool? DependsYDistToBot { get; set; }

    /// <summary>
    /// The bot will run to cover if it is closer than X
    /// </summary>
    [JsonPropertyName("RUN_IF_FAR")]
    public float? RunIfFar { get; set; }

    [JsonPropertyName("RUN_IF_FAR_SQRT")]
    public float? RunIfFarSqrt { get; set; }

    /// <summary>
    /// The bot will go shooting into cover if it is closer than X but more than RUN_IF_FAR
    /// </summary>
    [JsonPropertyName("STAY_IF_FAR")]
    public float? StayIfFar { get; set; }

    [JsonPropertyName("STAY_IF_FAR_SQRT")]
    public float? StayIfFarSqrt { get; set; }

    [JsonPropertyName("CHECK_COVER_ENEMY_LOOK")]
    public bool? CheckCoverEnemyLook { get; set; }

    [JsonPropertyName("SHOOT_NEAR_TO_LEAVE")]
    public int? ShootNearToLeave { get; set; }

    [JsonPropertyName("SHOOT_NEAR_SEC_PERIOD")]
    public float? ShootNearSecPeriod { get; set; }

    [JsonPropertyName("HITS_TO_LEAVE_COVER")]
    public int? HitsToLeaveCover { get; set; }

    [JsonPropertyName("HITS_TO_LEAVE_COVER_UNKNOWN")]
    public int? HitsToLeaveCoverUnknown { get; set; }

    [JsonPropertyName("DOG_FIGHT_AFTER_LEAVE")]
    public float? DogFightAfterLeave { get; set; }

    [JsonPropertyName("NOT_LOOK_AT_WALL_IS_DANGER")]
    public bool? NotLookAtWallIsDanger { get; set; }

    [JsonPropertyName("MIN_DEFENCE_LEVEL")]
    public float? MinDefenceLevel { get; set; }

    [JsonPropertyName("REWORK_NOT_TO_SHOOT")]
    public bool? ReworkNotToShoot { get; set; }

    [JsonPropertyName("DELETE_POINTS_BEHIND_ENEMIES")]
    public bool? DeletePointsBehindEnemies { get; set; }

    [JsonPropertyName("GOOD_DIST_TO_POINT_COEF")]
    public float? GoodDistToPointCoef { get; set; }

    [JsonPropertyName("ENEMY_DIST_TO_GO_OUT")]
    public float? EnemyDistToGoOut { get; set; }

    [JsonPropertyName("CHECK_CLOSEST_FRIEND")]
    public bool? CheckClosestFriend { get; set; }

    [JsonPropertyName("MIN_TO_ENEMY_TO_BE_NOT_SAFE_SQRT")]
    public float? MinToEnemyToBeNotSafeSqrt { get; set; }

    /// <summary>
    /// If the enemy is closer than X to this point, the bot will consider that it is impossible to hide there.
    /// </summary>
    [JsonPropertyName("MIN_TO_ENEMY_TO_BE_NOT_SAFE")]
    public float? MinToEnemyToBeNotSafe { get; set; }

    [JsonPropertyName("SIT_DOWN_WHEN_HOLDING")]
    public bool? SitDownWhenHolding { get; set; }

    [JsonPropertyName("STATIONARY_WEAPON_NO_ENEMY_GETUP")]
    public float? StationaryWeaponNoEnemyGetup { get; set; }

    [JsonPropertyName("STATIONARY_WEAPON_MAX_DIST_TO_USE")]
    public float? StationaryWeaponMaxDistToUse { get; set; }

    [JsonPropertyName("STATIONARY_SPOTTED_TIMES_TO_LEAVE")]
    public int? StationarySpottedTimesToLeave { get; set; }

    [JsonPropertyName("STATIONARY_CAN_USE")]
    public bool? StationaryCanUse { get; set; }

    [JsonPropertyName("CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY")]
    public bool? CanEndShootFromCoverCauseStationary { get; set; }

    [JsonPropertyName("CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY_DELTA")]
    public float? CanEndShootFromCoverCauseStationaryDelta { get; set; }

    [JsonPropertyName("CAN_END_SHOOT_FROM_COVER_CAUSE_STATIONARY_RADIUS")]
    public float? CanEndShootFromCoverCauseStationaryRadius { get; set; }

    /// <summary>
    /// If the enemy is visible (we are not under fire) and closer than X meters, then we stop holding.
    /// default 15
    /// </summary>
    [JsonPropertyName("END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE")]
    public float? EndHoldIfEnemyCloseAndVisible { get; set; }

    [JsonPropertyName("DIST_MAX_REWORK_NOT_TO_SHOOT")]
    public float? DistMaxReworkNotToShoot { get; set; }

    [JsonPropertyName("SDIST_MAX_REWORK_NOT_TO_SHOOT")]
    public float? SdistMaxReworkNotToShoot { get; set; }

    [JsonPropertyName("USE_DANGER_AREAS")]
    public bool? UseDangerAreas { get; set; }

    [JsonPropertyName("MAX_ITERATIONS")]
    public int? MaxIterations { get; set; }

    [JsonPropertyName("CHANGE_COVER_IF_CANT_SHOOT_SEC")]
    public float? ChangeCoverIfCantShootSec { get; set; }

    [JsonPropertyName("SHALL_CHANGE_COVER_IF_CAN_SHOOT")]
    public bool? ShallChangeCoverIfCanShoot { get; set; }

    [JsonPropertyName("CHECK_CLOSEST_FRIEND_DIST")]
    public float? CheckClosestFriendDist { get; set; }

    [JsonPropertyName("CAN_LAY_TO_COVER_DIST_LOOK_TO_ENEMY")]
    public float? CanLayToCoverDistLookToEnemy { get; set; }

    /// <summary>
    /// Can it lie down in shelters if it is of the lying type?
    /// </summary>
    [JsonPropertyName("CAN_LAY_TO_COVER")]
    public bool? CanLayToCover { get; set; }
}
