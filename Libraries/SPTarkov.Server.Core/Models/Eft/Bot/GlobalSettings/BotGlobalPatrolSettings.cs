using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalPatrolSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public class BotGlobalPatrolSettings
{
    [JsonPropertyName("LOOK_TIME_BASE")]
    public float? LookTimeBase { get; set; }

    [JsonPropertyName("RESERVE_TIME_STAY")]
    public float? ReserveTimeStay { get; set; }

    [JsonPropertyName("RESERVE_LOOT_TIME_STAY")]
    public float? ReserveLootTimeStay { get; set; }

    [JsonPropertyName("FRIEND_SEARCH_SEC")]
    public float? FriendSearchSeconds { get; set; }

    [JsonPropertyName("TALK_DELAY")]
    public float? TalkDelay { get; set; }

    [JsonPropertyName("MIN_TALK_DELAY")]
    public float? MinTalkDelay { get; set; }

    [JsonPropertyName("TALK_DELAY_BIG")]
    public float? TalkDelayBig { get; set; }

    [JsonPropertyName("CHANGE_WAY_TIME")]
    public float? ChangeWayTime { get; set; }

    [JsonPropertyName("MIN_DIST_TO_CLOSE_TALK")]
    public float? MinDistToCloseTalk { get; set; }

    [JsonPropertyName("VISION_DIST_COEF_PEACE")]
    public float? VisionDistanceCoefPeace { get; set; }

    [JsonPropertyName("MIN_DIST_TO_CLOSE_TALK_SQR")]
    public float? MinDistToCloseTalkSqr { get; set; }

    [JsonPropertyName("CHANCE_TO_CUT_WAY_0_100")]
    public float? ChanceToCutWay { get; set; }

    [JsonPropertyName("CUT_WAY_MIN_0_1")]
    public float? CutWayMin { get; set; }

    [JsonPropertyName("CUT_WAY_MAX_0_1")]
    public float? CutWayMax { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_WAY_0_100")]
    public float? ChanceToChangeWay { get; set; }

    [JsonPropertyName("CHANCE_TO_SHOOT_DEADBODY")]
    public int? ChanceToShootDeadBody { get; set; }

    [JsonPropertyName("SUSPETION_PLACE_LIFETIME")]
    public float? SuspicionPlaceLifetime { get; set; }

    [JsonPropertyName("RESERVE_OUT_TIME")]
    public float? ReserveOutTime { get; set; }

    [JsonPropertyName("CLOSE_TO_SELECT_RESERV_WAY")]
    public float? CloseToSelectReserveWay { get; set; }

    [JsonPropertyName("MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER")]
    public float? MaxYDistToStartWarnRequest { get; set; }

    [JsonPropertyName("MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER_ALLY")]
    public float? MaxYDistToStartWarnRequestAlly { get; set; }

    [JsonPropertyName("CAN_CHOOSE_RESERV")]
    public bool? CanChooseReserve { get; set; }

    [JsonPropertyName("USE_ONLY_RESERV")]
    public bool? UseOnlyReserve { get; set; }

    [JsonPropertyName("HEAD_TURN_PERIOD_TIME")]
    public float? HeadTurnPeriodTime { get; set; }

    [JsonPropertyName("HEAD_FRONT_PERIOD_TIME")]
    public float? HeadFrontPeriodTime { get; set; }

    [JsonPropertyName("CHANCE_TO_PLAY_GESTURE_WHEN_CLOSE")]
    public float? ChanceToPlayGestureWhenClose { get; set; }

    [JsonPropertyName("HEAD_TURN_SPEED")]
    public float? HeadTurnSpeed { get; set; }

    [JsonPropertyName("HEAD_ANG_ROTATE")]
    public float? HeadAngleRotate { get; set; }

    [JsonPropertyName("LOOT_PATROL_POTENTIAL_CLUSTERS_AMOUNT")]
    public int? LootPatrolPotentialClustersAmount { get; set; }

    [JsonPropertyName("CHANCE_TO_PLAY_VOICE_WHEN_CLOSE")]
    public float? ChanceToPlayVoiceWhenClose { get; set; }

    [JsonPropertyName("GO_TO_NEXT_POINT_DELTA")]
    public float? GoToNextPointDelta { get; set; }

    [JsonPropertyName("GO_TO_NEXT_POINT_DELTA_RESERV_WAY")]
    public float? GoToNextPointDeltaReserveWay { get; set; }

    [JsonPropertyName("DEAD_BODY_SEE_DIST")]
    public float? DeadBodySeeDistance { get; set; }

    [JsonPropertyName("DEAD_BODY_LEAVE_DIST")]
    public float? DeadBodyLeaveDistance { get; set; }

    [JsonPropertyName("CAN_LOOK_TO_DEADBODIES")]
    public bool? CanLookToDeadBodies { get; set; }

    [JsonPropertyName("GESTURE_LENGTH")]
    public float? GestureLength { get; set; }

    [JsonPropertyName("SHALL_STOP_IN_PEACEFUL_ACTION")]
    public bool? ShallStopInPeacefulAction { get; set; }

    [JsonPropertyName("FORCE_OPPONENT_TO_PEAEFUL")]
    public bool? ForceOpponentToPeaceful { get; set; }

    [JsonPropertyName("RESERVE_USE_SURGE_TIME_STAY")]
    public float? ReserveUseSurgeTimeStay { get; set; }

    [JsonPropertyName("RESERV_CAN_USE_MEDS")]
    public bool? ReserveCanUseMeds { get; set; }

    [JsonPropertyName("USE_PATROL_POINT_ACTION_MOVE_BY_RESERVE_WAY")]
    public bool? UsePatrolPointActionMoveByReserveWay { get; set; }

    [JsonPropertyName("USE_SURGIAL_KIT_OVER_THE_BODY_CAHNCE_100")]
    public float? UseSurgicalKitChance { get; set; }

    [JsonPropertyName("USE_SURGIAL_KIT_OVER_THE_BODY_SECOND_CAHNCE_100")]
    public float? UseSurgicalKitSecondChance { get; set; }

    [JsonPropertyName("FOLLOWER_START_MOVE_DELAY")]
    public float? FollowerStartMoveDelay { get; set; }

    [JsonPropertyName("USE_CHACHE_WAYS")]
    public bool? UseCachedWays { get; set; }

    [JsonPropertyName("ITEMS_TO_DROP")]
    public string? ItemsToDrop { get; set; }

    [JsonPropertyName("SPRINT_BETWEEN_CACHED_POINTS")]
    public float? SprintBetweenCachedPoints { get; set; }

    [JsonPropertyName("CHECK_MAGAZIN_PERIOD")]
    public float? CheckMagazinePeriod { get; set; }

    [JsonPropertyName("EAT_DRINK_PERIOD")]
    public float? EatDrinkPeriod { get; set; }

    [JsonPropertyName("WATCH_SECOND_WEAPON_PERIOD")]
    public float? WatchSecondWeaponPeriod { get; set; }

    [JsonPropertyName("CAN_WATCH_SECOND_WEAPON")]
    public bool? CanWatchSecondWeapon { get; set; }

    [JsonPropertyName("DEAD_BODY_LOOK_PERIOD")]
    public float? DeadBodyLookPeriod { get; set; }

    [JsonPropertyName("CAN_HARD_AIM")]
    public bool? CanHardAim { get; set; }

    [JsonPropertyName("CAN_PEACEFUL_LOOK")]
    public bool? CanPeacefulLook { get; set; }

    [JsonPropertyName("CAN_FRIENDLY_TILT")]
    public bool? CanFriendlyTilt { get; set; }

    [JsonPropertyName("CAN_GESTUS")]
    public bool? CanGestus { get; set; }

    [JsonPropertyName("TRY_CHOOSE_RESERV_WAY_ON_START")]
    public bool? TryChooseReserveWayOnStart { get; set; }

    [JsonPropertyName("CAN_CHECK_MAGAZINE")]
    public bool? CanCheckMagazine { get; set; }

    [JsonPropertyName("PICKUP_ITEMS_TO_BACKPACK_OR_CONTAINER")]
    public bool? PickupItemsToBackpackOrContainer { get; set; }

    [JsonPropertyName("DO_RANDOM_DROP_ITEM")]
    public bool? DoRandomDropItem { get; set; }

    [JsonPropertyName("STAY_AFTER_LOOT_TIME_MIN")]
    public float? StayAfterLootTimeMin { get; set; }

    [JsonPropertyName("STAY_AFTER_LOOT_TIME_MAX")]
    public float? StayAfterLootTimeMax { get; set; }

    [JsonPropertyName("USE_REAL_LOOTING")]
    public bool? UseRealLooting { get; set; }

    [JsonPropertyName("DEAD_BODY_DROP_ITEM_PROBABILITY")]
    public float? DeadBodyDropItemProbability { get; set; }
}
