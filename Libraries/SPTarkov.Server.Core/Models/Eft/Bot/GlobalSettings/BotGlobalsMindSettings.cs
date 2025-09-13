using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalsMindSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsMindSettings
{
    /// <summary>
    /// Should the bot ignore damage from grenades
    /// </summary>
    [JsonPropertyName("GRENADE_DAMAGE_IGNORE")]
    public bool? GrenadeDamageIgnore { get; set; }

    /// <summary>
    /// Min. number of shots fired at random at the position from which fire was conducted at close range
    /// </summary>
    [JsonPropertyName("MIN_SHOOTS_TIME")]
    public int? MinShootsTime { get; set; }

    /// <summary>
    /// Max number of shots fired at random at the position from which the fire was conducted at close range
    /// </summary>
    [JsonPropertyName("MAX_SHOOTS_TIME")]
    public int? MaxShootsTime { get; set; }

    /// <summary>
    /// The bot can only punish after this amount of time has passed since the last enemy it saw disappeared from the field.
    /// </summary>
    [JsonPropertyName("TIME_TO_RUN_TO_COVER_CAUSE_SHOOT_SEC")]
    public float? TimeToRunToCoverCauseShootSec { get; set; }

    /// <summary>
    /// The time after which the bot will restore its characteristics after receiving damage
    /// </summary>
    [JsonPropertyName("DAMAGE_REDUCTION_TIME_SEC")]
    public float? DamageReductionTimeSec { get; set; }

    /// <summary>
    /// Minimum damage a bot must take to get a danger point
    /// default 20
    /// </summary>
    [JsonPropertyName("MIN_DAMAGE_SCARE")]
    public float? MinDamageScare { get; set; }

    /// <summary>
    /// The probability that a bot will run if it is hit while it is in cover and cannot/cannot see to shoot at the enemy
    /// </summary>
    [JsonPropertyName("CHANCE_TO_RUN_CAUSE_DAMAGE_0_100")]
    public float? ChanceToRunCauseDamage0100 { get; set; }

    /// <summary>
    /// After X seconds, the enemy stops being given out by the task distributor to bots
    /// </summary>
    [JsonPropertyName("TIME_TO_FORGOR_ABOUT_ENEMY_SEC")]
    public float? TimeToForgorAboutEnemySec { get; set; }

    /// <summary>
    /// After X seconds the bot will search for the enemy by coming to the place of his last vision! must be less than TIME_TO_FORGOR_ABOUT_ENEMY_SEC
    /// </summary>
    [JsonPropertyName("TIME_TO_FIND_ENEMY")]
    public float? TimeToFindEnemy { get; set; }

    [JsonPropertyName("MAX_AGGRO_BOT_DIST")]
    public float? MaxAggroBotDist { get; set; }

    /// <summary>
    /// The coefficient of accuracy of perception of the position from which the player was hit is higher - more accurate
    /// </summary>
    [JsonPropertyName("HIT_POINT_DETECTION")]
    public float? HitPointDetection { get; set; }

    /// <summary>
    /// Danger point coefficient when seeking shelter. Danger point
    /// </summary>
    [JsonPropertyName("DANGER_POINT_CHOOSE_COEF")]
    public float? DangerPointChooseCoef { get; set; }

    /// <summary>
    /// Danger point coefficient when seeking shelter. Simple point
    /// </summary>
    [JsonPropertyName("SIMPLE_POINT_CHOOSE_COEF")]
    public float? SimplePointChooseCoef { get; set; }

    [JsonPropertyName("LASTSEEN_POINT_CHOOSE_COEF")]
    public float? LastseenPointChooseCoef { get; set; }

    [JsonPropertyName("COVER_DIST_COEF")]
    public float? CoverDistCoef { get; set; }

    [JsonPropertyName("DIST_TO_FOUND_SQRT")]
    public float? DistToFoundSqrt { get; set; }

    /// <summary>
    /// Does the player search for the opponent when there is a GoalTarget?
    /// </summary>
    [JsonPropertyName("SEARCH_TARGET")]
    public bool? SearchTarget { get; set; }

    /// <summary>
    /// if the flag is set on at least one bot in a bot group, then the entire group becomes hostile, in which there is one hostile PMC player
    /// </summary>
    [JsonPropertyName("ENEMY_BY_GROUPS_PMC_PLAYERS")]
    public bool? EnemyByGroupsPmcPlayers { get; set; }

    /// <summary>
    /// if the flag is set on at least one bot in a bot group, then the entire group becomes hostile, which contains one hostile wild player
    /// </summary>
    [JsonPropertyName("ENEMY_BY_GROUPS_SAVAGE_PLAYERS")]
    public bool? EnemyByGroupsSavagePlayers { get; set; }

    /// <summary>
    /// If true then bosses do not change their behavior for a player with a high reputation as a fencer
    /// </summary>
    [JsonPropertyName("BOSS_IGNORE_LOYALTY")]
    public bool? BossIgnoreLoyalty { get; set; }

    [JsonPropertyName("DEFAULT_BEAR_BEHAVIOUR")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EWarnBehaviour? DefaultBearBehaviour { get; set; }

    [JsonPropertyName("DEFAULT_USEC_BEHAVIOUR")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EWarnBehaviour? DefaultUsecBehaviour { get; set; }

    [JsonPropertyName("DEFAULT_SAVAGE_BEHAVIOUR")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EWarnBehaviour? DefaultSavageBehaviour { get; set; }

    /// <summary>
    /// Must be sent to client as strings
    /// </summary>
    [JsonPropertyName("FRIENDLY_BOT_TYPES")]
    public List<WildSpawnType>? FriendlyBotTypes { get; set; }

    /// <summary>
    /// Must be sent to client as strings
    /// </summary>
    [JsonPropertyName("WARN_BOT_TYPES")]
    public List<WildSpawnType>? WarnBotTypes { get; set; }

    /// <summary>
    /// Must be sent to client as strings
    /// </summary>
    [JsonPropertyName("ENEMY_BOT_TYPES")]
    public List<WildSpawnType>? EnemyBotTypes { get; set; }

    /// <summary>
    /// Must be sent to client as strings
    /// </summary>
    [JsonPropertyName("REVENGE_BOT_TYPES")]
    public List<WildSpawnType>? RevengeBotTypes { get; set; }

    [JsonPropertyName("FOLLOWER_AND_BOSS_WARN_EQUAL_PRIORITY")]
    public bool? FollowerAndBossWarnEqualPriority { get; set; }

    [JsonPropertyName("MUTUAL_IGNORE_FRIENDLY_FIRE")]
    public bool? MutualIgnoreFriendlyFire { get; set; }

    [JsonPropertyName("MAX_AGGRO_BOT_DIST_UPPER_LIMIT")]
    public float? MaxAggroBotDistUpperLimit { get; set; }

    [JsonPropertyName("MAX_AGGRO_BOT_DIST_SQR_UPPER_LIMIT")]
    public float? MaxAggroBotDistSqrUpperLimit { get; set; }

    [JsonPropertyName("MAX_AGGRO_BOT_DIST_SQR")]
    public float? MaxAggroBotDistSqr { get; set; }

    [JsonPropertyName("DIST_TO_STOP_RUN_ENEMY")]
    public float? DistToStopRunEnemy { get; set; }

    /// <summary>
    /// The angle at which the bot understands that the enemy is looking at it
    /// </summary>
    [JsonPropertyName("ENEMY_LOOK_AT_ME_ANG")]
    public float? EnemyLookAtMeAng { get; set; }

    [JsonPropertyName("MIN_START_AGGRESION_COEF")]
    public float? MinStartAggresionCoef { get; set; }

    [JsonPropertyName("MAX_START_AGGRESION_COEF")]
    public float? MaxStartAggresionCoef { get; set; }

    /// <summary>
    /// Distance from which a bot can "feel" a bullet
    /// </summary>
    [JsonPropertyName("BULLET_FEEL_DIST")]
    public float? BulletFeelDist { get; set; }

    [JsonPropertyName("BULLET_FEEL_CLOSE_SDIST")]
    public float? BulletFeelCloseSdist { get; set; }

    /// <summary>
    /// Chance that after losing sight of an enemy and not having a new one, the bot will immediately go looking for an enemy.
    /// Only applicable for attack tactics. (Then there is a check for your own strength and the strength of the enemy.)
    /// </summary>
    [JsonPropertyName("ATTACK_IMMEDIATLY_CHANCE_0_100")]
    public float? AttackImmediatlyChance0100 { get; set; }

    /// <summary>
    /// Chance to show a faq when the bot sees the player
    /// </summary>
    [JsonPropertyName("CHANCE_FUCK_YOU_ON_CONTACT_100")]
    public float? ChanceFuckYouOnContact100 { get; set; }

    /// <summary>
    /// How much does a bot's aggression drop if someone in its group is killed?
    /// </summary>
    [JsonPropertyName("FRIEND_DEAD_AGR_LOW")]
    public float? FriendDeadAgrLow { get; set; }

    [JsonPropertyName("FRIEND_AGR_KILL")]
    public float? FriendAgrKill { get; set; }

    [JsonPropertyName("LAST_ENEMY_LOOK_TO")]
    public float? LastEnemyLookTo { get; set; }

    [JsonPropertyName("CAN_RECEIVE_PLAYER_REQUESTS_BEAR")]
    public bool? CanReceivePlayerRequestsBear { get; set; }

    [JsonPropertyName("CAN_RECEIVE_PLAYER_REQUESTS_USEC")]
    public bool? CanReceivePlayerRequestsUsec { get; set; }

    [JsonPropertyName("CAN_RECEIVE_PLAYER_REQUESTS_SAVAGE")]
    public bool? CanReceivePlayerRequestsSavage { get; set; }

    [JsonPropertyName("REVENGE_TO_GROUP")]
    public bool? RevengeToGroup { get; set; }

    [JsonPropertyName("REVENGE_FOR_SAVAGE_PLAYERS")]
    public bool? RevengeForSavagePlayers { get; set; }

    [JsonPropertyName("CAN_USE_MEDS")]
    public bool? CanUseMeds { get; set; }

    [JsonPropertyName("SUSPETION_POINT_CHANCE_ADD100")]
    public float? SuspetionPointChanceAdd100 { get; set; }

    [JsonPropertyName("AMBUSH_WHEN_UNDER_FIRE")]
    public bool? AmbushWhenUnderFire { get; set; }

    [JsonPropertyName("AMBUSH_WHEN_UNDER_FIRE_TIME_RESIST")]
    public float? AmbushWhenUnderFireTimeResist { get; set; }

    [JsonPropertyName("CAN_LOOT_BOSS_CLUSTER")]
    public bool? CanLootBossCluster { get; set; }

    [JsonPropertyName("ATTACK_ENEMY_IF_PROTECT_DELTA_LAST_TIME_SEEN")]
    public float? AttackEnemyIfProtectDeltaLastTimeSeen { get; set; }

    [JsonPropertyName("HOLD_IF_PROTECT_DELTA_LAST_TIME_SEEN")]
    public float? HoldIfProtectDeltaLastTimeSeen { get; set; }

    [JsonPropertyName("FIND_COVER_TO_GET_POSITION_WITH_SHOOT")]
    public float? FindCoverToGetPositionWithShoot { get; set; }

    [JsonPropertyName("PROTECT_TIME_REAL")]
    public bool? ProtectTimeReal { get; set; }

    [JsonPropertyName("CHANCE_SHOOT_WHEN_WARN_PLAYER_100")]
    public float? ChanceShootWhenWarnPlayer100 { get; set; }

    [JsonPropertyName("CAN_PANIC_IS_PROTECT")]
    public bool? CanPanicIsProtect { get; set; }

    [JsonPropertyName("NO_RUN_AWAY_FOR_SAFE")]
    public bool? NoRunAwayForSafe { get; set; }

    [JsonPropertyName("PART_PERCENT_TO_HEAL")]
    public float? PartPercentToHeal { get; set; }

    [JsonPropertyName("PROTECT_DELTA_HEAL_SEC")]
    public float? ProtectDeltaHealSec { get; set; }

    [JsonPropertyName("CAN_STAND_BY")]
    public bool? CanStandBy { get; set; }

    [JsonPropertyName("CAN_THROW_REQUESTS")]
    public bool? CanThrowRequests { get; set; }

    [JsonPropertyName("GROUP_ANY_PHRASE_DELAY")]
    public float? GroupAnyPhraseDelay { get; set; }

    [JsonPropertyName("GROUP_EXACTLY_PHRASE_DELAY")]
    public float? GroupExactlyPhraseDelay { get; set; }

    [JsonPropertyName("GROUP_EXACTLY_PHRASE_DELAY_MAX")]
    public float? GroupExactlyPhraseDelayMax { get; set; }

    [JsonPropertyName("DIST_TO_ENEMY_YO_CAN_HEAL")]
    public float? DistToEnemyYoCanHeal { get; set; }

    /// <summary>
    /// Chance that after the first 2 actions when warning the bot will stand and wait for the next 4 seconds
    /// </summary>
    [JsonPropertyName("CHANCE_TO_STAY_WHEN_WARN_PLAYER_100")]
    public float? ChanceToStayWhenWarnPlayer100 { get; set; }

    [JsonPropertyName("DOG_FIGHT_OUT")]
    public float? DogFightOut { get; set; }

    /// <summary>
    /// Will enter dogfight
    /// default 3
    /// </summary>
    [JsonPropertyName("DOG_FIGHT_IN")]
    public float? DogFightIn { get; set; }

    [JsonPropertyName("SHOOT_INSTEAD_DOG_FIGHT")]
    public float? ShootInsteadDogFight { get; set; }

    [JsonPropertyName("PISTOL_SHOTGUN_AMBUSH_DIST")]
    public float? PistolShotgunAmbushDist { get; set; }

    [JsonPropertyName("STANDART_AMBUSH_DIST")]
    public float? StandartAmbushDist { get; set; }

    [JsonPropertyName("AI_POWER_COEF")]
    public float? AiPowerCoef { get; set; }

    [JsonPropertyName("COVER_SECONDS_AFTER_LOSE_VISION")]
    public float? CoverSecondsAfterLoseVision { get; set; }

    [JsonPropertyName("COVER_SELF_ALWAYS_IF_DAMAGED")]
    public bool? CoverSelfAlwaysIfDamaged { get; set; }

    [JsonPropertyName("SEC_TO_MORE_DIST_TO_RUN")]
    public float? SecToMoreDistToRun { get; set; }

    [JsonPropertyName("HEAL_DELAY_SEC")]
    public float? HealDelaySec { get; set; }

    [JsonPropertyName("HIT_DELAY_WHEN_HAVE_SMT")]
    public float? HitDelayWhenHaveSmt { get; set; }

    [JsonPropertyName("HIT_DELAY_WHEN_PEACE")]
    public float? HitDelayWhenPeace { get; set; }

    [JsonPropertyName("TALK_WITH_QUERY")]
    public bool? TalkWithQuery { get; set; }

    [JsonPropertyName("DANGER_EXPIRE_TIME_MIN")]
    public float? DangerExpireTimeMin { get; set; }

    [JsonPropertyName("DANGER_EXPIRE_TIME_MAX")]
    public float? DangerExpireTimeMax { get; set; }

    [JsonPropertyName("PANIC_RUN_WEIGHT")]
    public float? PanicRunWeight { get; set; }

    [JsonPropertyName("PANIC_SIT_WEIGHT")]
    public float? PanicSitWeight { get; set; }

    [JsonPropertyName("PANIC_LAY_WEIGHT")]
    public float? PanicLayWeight { get; set; }

    [JsonPropertyName("PANIC_NONE_WEIGHT")]
    public float? PanicNoneWeight { get; set; }

    [JsonPropertyName("PANIC_SIT_WEIGHT_PEACE")]
    public float? PanicSitWeightPeace { get; set; }

    /// <summary>
    /// Can a bot execute requests?
    /// </summary>
    [JsonPropertyName("CAN_EXECUTE_REQUESTS")]
    public bool? CanExecuteRequests { get; set; }

    [JsonPropertyName("CAN_WARN_SELF")]
    public bool? CanWarnSelf { get; set; }

    [JsonPropertyName("DIST_TO_ENEMY_SPOTTED_ON_HIT")]
    public float? DistToEnemySpottedOnHit { get; set; }

    [JsonPropertyName("UNDER_FIRE_PERIOD")]
    public float? UnderFirePeriod { get; set; }

    [JsonPropertyName("MEDS_ONLY_SAFE_CONTAINER")]
    public bool? MedsOnlySafeContainer { get; set; }

    [JsonPropertyName("CAN_DROP_ITEMS")]
    public bool? CanDropItems { get; set; }

    [JsonPropertyName("CAN_TAKE_ITEMS")]
    public bool? CanTakeItems { get; set; }

    [JsonPropertyName("THROW_DIST_TO_SEE")]
    public float? ThrowDistToSee { get; set; }

    [JsonPropertyName("CAN_TAKE_ANY_ITEM")]
    public bool? CanTakeAnyItem { get; set; }

    [JsonPropertyName("WILL_PERSUE_AXEMAN")]
    public bool? WillPersueAxeman { get; set; }

    [JsonPropertyName("MAX_DIST_TO_RUN_PERSUE_AXEMAN")]
    public float? MaxDistToRunPersueAxeman { get; set; }

    [JsonPropertyName("MAX_DIST_TO_PERSUE_AXEMAN")]
    public float? MaxDistToPersueAxeman { get; set; }

    /// <summary>
    /// Use surgeon kit only from safe container
    /// </summary>
    [JsonPropertyName("SURGE_KIT_ONLY_SAFE_CONTAINER")]
    public bool? SurgeKitOnlySafeContainer { get; set; }

    [JsonPropertyName("CAN_USE_LONG_COVER_POINTS")]
    public bool? CanUseLongCoverPoints { get; set; }

    [JsonPropertyName("CAN_USE_FOOD_DRINK")]
    public bool? CanUseFoodDrink { get; set; }

    [JsonPropertyName("FOOD_DRINK_DELAY_SEC")]
    public float? FoodDrinkDelaySec { get; set; }

    /// <summary>
    /// What exactly to do when you come to a corpse 1 - use a medicine kit 2 - loot. Default - watch. public bool CAN_TALK = true;
    /// </summary>
    [JsonPropertyName("HOW_WORK_OVER_DEAD_BODY")]
    public int? HowWorkOverDeadBody { get; set; }

    [JsonPropertyName("DEADBODYWORK_INITIAL_DELAY")]
    public float? DeadbodyworkInitialDelay { get; set; }

    [JsonPropertyName("DEADBODYWORK_CHECK_ITEMS_DELAY")]
    public float? DeadbodyworkCheckItemsDelay { get; set; }

    [JsonPropertyName("DEADBODYWORK_MOVE_ITEMS_DELAY")]
    public float? DeadbodyworkMoveItemsDelay { get; set; }

    [JsonPropertyName("DEADBODYWORK_DROP_ITEMS_DELAY")]
    public float? DeadbodyworkDropItemsDelay { get; set; }

    [JsonPropertyName("CAN_TALK")]
    public bool? CanTalk { get; set; }

    [JsonPropertyName("ACTIVE_FORCE_ATTACK_EVENTS")]
    public bool? ActiveForceAttackEvents { get; set; }

    [JsonPropertyName("ACTIVE_FOLLOW_PLAYER_EVENTS")]
    public bool? ActiveFollowPlayerEvents { get; set; }

    /// <summary>
    /// The bot can be called to help by the boss (for example, Tagilla or Boar
    /// </summary>
    [JsonPropertyName("MAY_BE_CALLED_FOR_HELP")]
    public bool? MayBeCalledForHelp { get; set; }

    [JsonPropertyName("GIFTER_ADDITIONAL_GIFTS")]
    public int? GifterAdditionalGifts { get; set; }

    [JsonPropertyName("ANGLE_TO_SHOOT_BTR")]
    public float? AngleToShootBtr { get; set; }

    [JsonPropertyName("ROTATION_SPEED_BTR")]
    public float? RotationSpeedBtr { get; set; }

    [JsonPropertyName("IGNORE_ANOTHER_BOTS_BEING_HIT")]
    public bool? IgnoreAnotherBotsBeingHit { get; set; }

    [JsonPropertyName("AVOID_BTR_RADIUS_SQR")]
    public float? AvoidBtrRadiusSqr { get; set; }

    [JsonPropertyName("SNIPER_FIRE_IMMUNE")]
    public bool? SniperFireImmune { get; set; }

    /// <summary>
    /// If the flag is set, then enemies are added only for reasons from the VALID_REASONS_TO_ADD_ENEMY list.
    /// </summary>
    [JsonPropertyName("USE_ADD_TO_ENEMY_VALIDATION")]
    public bool? UseAddToEnemyValidation { get; set; }

    /// <summary>
    /// List of valid reasons for adding enemies, provided that the USE_ADD_TO_ENEMY_VALIDATION flag is set
    /// </summary>
    [JsonPropertyName("VALID_REASONS_TO_ADD_ENEMY")]
    public List<EBotEnemyCause>? ValidReasonsToAddEnemy { get; set; }

    [JsonPropertyName("CHECK_MARK_OF_UNKNOWS")]
    public bool? CheckMarkOfUnknows { get; set; }

    [JsonPropertyName("SDIST_TO_DELIVER_INFO_WHEN_ENEMY")]
    public float? SdistToDeliverInfoWhenEnemy { get; set; }

    [JsonPropertyName("TRIPWIRE_INERT_TIME")]
    public int? TripwireInertTime { get; set; }

    [JsonPropertyName("IGNORE_TRAP")]
    public bool? IgnoreTrap { get; set; }

    [JsonPropertyName("CHANCE_TO_IGNORE_TRIPWIRE")]
    public float? ChanceToIgnoreTripwire { get; set; }

    [JsonPropertyName("CHACE_TO_DEACTIVATE")]
    public float? ChaceToDeactivate { get; set; }

    [JsonPropertyName("REACT_ADD_DRUNK_ENEMY")]
    public bool? ReactAddDrunkEnemy { get; set; }

    [JsonPropertyName("DIST_TO_HIDE_ASSAULT")]
    public float? DistToHideAssault { get; set; }

    [JsonPropertyName("KEEP_ZONE_ON_SPAWN_TIME_SEC")]
    public float? KeepZoneOnSpawnTimeSec { get; set; }

    [JsonPropertyName("IGNORE_DANGER_PLACES")]
    public bool? IgnoreDangerPlaces { get; set; }

    [JsonPropertyName("PUSH_AND_SUPPRESS_HIDE")]
    public float? PushAndSuppressHide { get; set; }

    /// <summary>
    /// For the PushAndSuppressLayer layer, the distance from which the bot will push HARD
    /// </summary>
    [JsonPropertyName("PUSH_AND_SUPPRESS_PUSH")]
    public float? PushAndSuppressPush { get; set; }

    public enum EWarnBehaviour
    {
        Default = 1,
        Neutral = 2,
        Warn = 4,
        AlwaysEnemies = 8,
        AlwaysFriends = 16,
        ChancedEnemies = 32,
    }

    public enum EBotEnemyCause
    {
        pairLogic,
        initial,
        zryachiyLogic,
        addPlayerToBoss,
        addPlayer,
        addCauseGroup,
        initCauseEnemy,
        checkAddTODO,
        addBotAtGroup,
        addBotNoGroup,
        AddNewMember,
        byKill,
        AddEnemyToAllGroupsInBotZone,
        AddEnemyToAllGroups,
        warn,
        callBot,
        followGetHit,
        gifterKill,
        bossKillArena,
        KillaSyncTagilla,
        tagillaFindENemy,
        fuckGestus,
        pmcBossKill,
        rndWanrRequest,
        christmas,
        synWithKilla,
        death,
        doFollow2,
        doFollow,
        callForHelp2,
        callForHelp1,
        ravangeZryachiy,
        lighthouseKeeperServices,
        lighthouseKeeperServicesTarget,
        partisanBadKarma,
        attackBTR,
        serviceBTR,
        tagillaAlarm,
        drunk,
        Unknown,
        MarkOfUnknowsDist,
    }
}
