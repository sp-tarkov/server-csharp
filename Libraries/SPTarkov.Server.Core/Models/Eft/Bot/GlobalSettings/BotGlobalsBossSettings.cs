using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// See BotGlobalsBossSettings in the client, this record should match that
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsBossSettings
{
    [JsonPropertyName("BOSS_DIST_TO_WARNING")]
    public float? BossDistToWarning { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_ALLY")]
    public float? BossDistToWarningAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT")]
    public float? BossDistToWarningSqrt { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT_ALLY")]
    public float? BossDistToWarningSqrtAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_USEC")]
    public float? BossDistToWarningUsec { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_USEC_ALLY")]
    public float? BossDistToWarningUsecAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT_USEC")]
    public float? BossDistToWarningSqrtUsec { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT_USEC_ALLY")]
    public float? BossDistToWarningSqrtUsecAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_BEAR")]
    public float? BossDistToWarningBear { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_BEAR_ALLY")]
    public float? BossDistToWarningBearAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT_BEAR")]
    public float? BossDistToWarningSqrtBear { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_SQRT_BEAR_ALLY")]
    public float? BossDistToWarningSqrtBearAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT")]
    public float? BossDistToWarningOut { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT_ALLY")]
    public float? BossDistToWarningOutAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT_SQRT")]
    public float? BossDistToWarningOutSqrt { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT_SQRT_ALLY")]
    public float? BossDistToWarningOutSqrtAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_SHOOT")]
    public float? BossDistToShoot { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_SHOOT_ALLY")]
    public float? BossDistToShootAlly { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_SHOOT_SQRT")]
    public float? BossDistToShootSqrt { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_SHOOT_SQRT_ALLY")]
    public float? BossDistToShootSqrtAlly { get; set; }

    [JsonPropertyName("CHANCE_TO_SEND_GRENADE_100")]
    public float? ChanceToSendGrenade100 { get; set; }

    [JsonPropertyName("MAX_DIST_COVER_BOSS")]
    public float? MaxDistCoverBoss { get; set; }

    [JsonPropertyName("MAX_DIST_COVER_BOSS_SQRT")]
    public float? MaxDistCoverBossSqrt { get; set; }

    [JsonPropertyName("MAX_DIST_DECIDER_TO_SEND")]
    public float? MaxDistDeciderToSend { get; set; }

    [JsonPropertyName("MAX_DIST_DECIDER_TO_SEND_SQRT")]
    public float? MaxDistDeciderToSendSqrt { get; set; }

    [JsonPropertyName("TIME_AFTER_LOSE")]
    public float? TimeAfterLose { get; set; }

    [JsonPropertyName("TIME_AFTER_LOSE_DELTA")]
    public float? TimeAfterLoseDelta { get; set; }

    [JsonPropertyName("PERSONS_SEND")]
    public int? PersonsSend { get; set; }

    [JsonPropertyName("DELTA_SEARCH_TIME")]
    public float? DeltaSearchTime { get; set; }

    [JsonPropertyName("COVER_TO_SEND")]
    public bool? CoverToSend { get; set; }

    [JsonPropertyName("WAIT_NO_ATTACK_SAVAGE")]
    public float? WaitNoAttackSavage { get; set; }

    [JsonPropertyName("CHANCE_USE_RESERVE_PATROL_100")]
    public float? ChanceUseReservePatrol100 { get; set; }

    [JsonPropertyName("WARN_PLAYER_PERIOD")]
    public float? WarnPlayerPeriod { get; set; }

    [JsonPropertyName("EFFECT_REGENERATION_PER_MIN")]
    public float? EffectRegenerationPerMin { get; set; }

    [JsonPropertyName("EFFECT_PAINKILLER")]
    public bool? EffectPainkiller { get; set; }

    [JsonPropertyName("DISABLE_METABOLISM")]
    public bool? DisableMetabolism { get; set; }

    [JsonPropertyName("KILLA_Y_DELTA_TO_BE_ENEMY_BOSS")]
    public float? KillaYDeltaToBeEnemyBoss { get; set; }

    [JsonPropertyName("KILLA_DITANCE_TO_BE_ENEMY_BOSS")]
    public float? KillaDitanceToBeEnemyBoss { get; set; }

    [JsonPropertyName("KILLA_START_SEARCH_SEC")]
    public float? KillaStartSearchSec { get; set; }

    [JsonPropertyName("KILLA_CONTUTION_TIME")]
    public float? KillaContutionTime { get; set; }

    [JsonPropertyName("KILLA_CLOSE_ATTACK_DIST")]
    public float? KillaCloseAttackDist { get; set; }

    [JsonPropertyName("KILLA_MIDDLE_ATTACK_DIST")]
    public float? KillaMiddleAttackDist { get; set; }

    [JsonPropertyName("KILLA_LARGE_ATTACK_DIST")]
    public float? KillaLargeAttackDist { get; set; }

    [JsonPropertyName("KILLA_SEARCH_METERS")]
    public float? KillaSearchMeters { get; set; }

    [JsonPropertyName("KILLA_DEF_DIST_SQRT")]
    public float? KillaDefDistSqrt { get; set; }

    [JsonPropertyName("KILLA_SEARCH_SEC_STOP_AFTER_COMING")]
    public float? KillaSearchSecStopAfterComing { get; set; }

    [JsonPropertyName("KILLA_DIST_TO_GO_TO_SUPPRESS")]
    public float? KillaDistToGoToSuppress { get; set; }

    [JsonPropertyName("KILLA_AFTER_GRENADE_SUPPRESS_DELAY")]
    public float? KillaAfterGrenadeSuppressDelay { get; set; }

    [JsonPropertyName("KILLA_CLOSEATTACK_TIMES")]
    public int? KillaCloseattackTimes { get; set; }

    [JsonPropertyName("KILLA_CLOSEATTACK_DELAY")]
    public float? KillaCloseattackDelay { get; set; }

    [JsonPropertyName("KILLA_HOLD_DELAY")]
    public float? KillaHoldDelay { get; set; }

    [JsonPropertyName("KILLA_BULLET_TO_RELOAD")]
    public int? KillaBulletToReload { get; set; }

    [JsonPropertyName("PERCENT_BULLET_TO_RELOAD")]
    public float? PercentBulletToReload { get; set; }

    [JsonPropertyName("SHALL_WARN")]
    public bool? ShallWarn { get; set; }

    [JsonPropertyName("KILLA_ENEMIES_TO_ATTACK")]
    public int? KillaEnemiesToAttack { get; set; }

    [JsonPropertyName("KILLA_ONE_IS_CLOSE")]
    public float? KillaOneIsClose { get; set; }

    [JsonPropertyName("KILLA_TRIGGER_DOWN_DELAY")]
    public float? KillaTriggerDownDelay { get; set; }

    [JsonPropertyName("KILLA_WAIT_IN_COVER_COEF")]
    public float? KillaWaitInCoverCoef { get; set; }

    [JsonPropertyName("TAGILLA_Y_DELTA_TO_BE_ENEMY_BOSS")]
    public float? TagillaYDeltaToBeEnemyBoss { get; set; }

    [JsonPropertyName("TAGILLA_SAVAGE_HELP_SQR_DIST")]
    public float? TagillaSavageHelpSqrDist { get; set; }

    [JsonPropertyName("TAGILLA_FORCED_CLOSE_ATTACK_DIST")]
    public float? TagillaForcedCloseAttackDist { get; set; }

    [JsonPropertyName("TAGILLA_FIRST_ASSAULT_RADIUS")]
    public float? TagillaFirstAssaultRadius { get; set; }

    [JsonPropertyName("TAGILLA_FIRST_ASSAULT_CHANCE")]
    public float? TagillaFirstAssaultChance { get; set; }

    [JsonPropertyName("TAGILLA_SECOND_ASSAULT_RADIUS")]
    public float? TagillaSecondAssaultRadius { get; set; }

    [JsonPropertyName("TAGILLA_SECOND_ASSAULT_CHANCE")]
    public float? TagillaSecondAssaultChance { get; set; }

    [JsonPropertyName("TAGILLA_FEEL_ENEMY_DIST_SQR")]
    public float? TagillaFeelEnemyDistSqr { get; set; }

    [JsonPropertyName("TAGILLA_TIME_TO_PURSUIT_WITHOUT_HITS")]
    public float? TagillaTimeToPursuitWithoutHits { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_ATTACK_NEXT_DECISION_PERIOD")]
    public float? TagillaMeleeAttackNextDecisionPeriod { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_CHANCE_RELOAD")]
    public float? TagillaMeleeChanceReload { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_CHANCE_INTERACTION")]
    public float? TagillaMeleeChanceInteraction { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_CHANCE_INVENTORY")]
    public float? TagillaMeleeChanceInventory { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_CHANCE_MEDS")]
    public float? TagillaMeleeChanceMeds { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_CHANCE_FORCED")]
    public float? TagillaMeleeChanceForced { get; set; }

    [JsonPropertyName("TAGILLA_MIN_TIME_TO_REPEAT_MELEE_ASSAULT")]
    public float? TagillaMinTimeToRepeatMeleeAssault { get; set; }

    [JsonPropertyName("KOJANIY_DIST_WHEN_READY")]
    public float? KojaniyDistWhenReady { get; set; }

    /// <summary>
    /// to calculate the number of enemies, this radius is taken into account
    /// </summary>
    [JsonPropertyName("KOJANIY_DIST_TO_BE_ENEMY")]
    public float? KojaniyDistToBeEnemy { get; set; }

    [JsonPropertyName("KOJANIY_MIN_DIST_TO_LOOT")]
    public float? KojaniyMinDistToLoot { get; set; }

    [JsonPropertyName("KOJANIY_MIN_DIST_TO_LOOT_SQRT")]
    public float? KojaniyMinDistToLootSqrt { get; set; }

    [JsonPropertyName("KOJANIY_DIST_ENEMY_TOO_CLOSE")]
    public float? KojaniyDistEnemyTooClose { get; set; }

    [JsonPropertyName("KOJANIY_MANY_ENEMIES_COEF")]
    public float? KojaniyManyEnemiesCoef { get; set; }

    [JsonPropertyName("KOJANIY_FIGHT_CENTER_POS_ME")]
    public bool? KojaniyFightCenterPosMe { get; set; }

    [JsonPropertyName("KOJANIY_DIST_CORE_SPOS_RECALC")]
    public float? KojaniyDistCoreSposRecalc { get; set; }

    [JsonPropertyName("KOJANIY_DIST_CORE_SPOS_RECALC_SQRT")]
    public float? KojaniyDistCoreSposRecalcSqrt { get; set; }

    [JsonPropertyName("KOJANIY_START_SUPPERS_SHOOTS_SEC")]
    public float? KojaniyStartSuppersShootsSec { get; set; }

    [JsonPropertyName("KOJANIY_START_NEXT_SUPPERS_SHOOTS_SEC")]
    public float? KojaniyStartNextSuppersShootsSec { get; set; }

    [JsonPropertyName("KOJANIY_SAFE_ENEMIES")]
    public int? KojaniySafeEnemies { get; set; }

    [JsonPropertyName("KOJANIY_TAKE_CARE_ABOULT_ENEMY_DELTA")]
    public float? KojaniyTakeCareAboultEnemyDelta { get; set; }

    [JsonPropertyName("KOJANIY_WANNA_GO_TO_CLOSEST_COVER")]
    public float? KojaniyWannaGoToClosestCover { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWER_PATH_NAME")]
    public string? GluharFollowerPathName { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWER_SCOUT_DIST_START_ATTACK")]
    public float? GluharFollowerScoutDistStartAttack { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWER_SCOUT_DIST_END_ATTACK")]
    public float? GluharFollowerScoutDistEndAttack { get; set; }

    [JsonPropertyName("GLUHAR_BOSS_WANNA_ATTACK_CHANCE_0_100")]
    public float? GluharBossWannaAttackChance0100 { get; set; }

    [JsonPropertyName("GLUHAR_ASSAULT_ATTACK_DIST")]
    public float? GluharAssaultAttackDist { get; set; }

    [JsonPropertyName("GLUHAR_STOP_ASSAULT_ATTACK_DIST")]
    public float? GluharStopAssaultAttackDist { get; set; }

    [JsonPropertyName("GLUHAR_TIME_TO_ASSAULT")]
    public float? GluharTimeToAssault { get; set; }

    [JsonPropertyName("DIST_TO_PROTECT_BOSS")]
    public float? DistToProtectBoss { get; set; }

    [JsonPropertyName("GLUHAR_SEC_TO_REINFORSMENTS")]
    public float? GluharSecToReinforsments { get; set; }

    [JsonPropertyName("GLUHAR_REINFORSMENTS_BY_EXIT")]
    public bool? GluharReinforsmentsByExit { get; set; }

    [JsonPropertyName("GLUHAR_REINFORSMENTS_BY_EVENT")]
    public bool? GluharReinforsmentsByEvent { get; set; }

    [JsonPropertyName("GLUHAR_REINFORSMENTS_BY_PLAYER_COME_TO_ZONE")]
    public bool? GluharReinforsmentsByPlayerComeToZone { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWERS_TO_REINFORSMENTS")]
    public int? GluharFollowersToReinforsments { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWERS_SECURITY")]
    public int? GluharFollowersSecurity { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWERS_ASSAULT")]
    public int? GluharFollowersAssault { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWERS_SCOUT")]
    public int? GluharFollowersScout { get; set; }

    [JsonPropertyName("GLUHAR_FOLLOWERS_SNIPE")]
    public int? GluharFollowersSnipe { get; set; }

    [JsonPropertyName("GLUHAR_BOSS_DIST_TO_ENEMY_WANT_KILL")]
    public float? GluharBossDistToEnemyWantKill { get; set; }

    [JsonPropertyName("IF_I_HITTED_GO_AWAY_SEC_HIT")]
    public float? IfIHittedGoAwaySecHit { get; set; }

    [JsonPropertyName("DIST_TO_START_RUN_FOR_COVER_WITH_STOP")]
    public float? DistToStartRunForCoverWithStop { get; set; }

    [JsonPropertyName("DELTA_DIST_DEST_BOSS_START_RUN_FOR_COVER_WITH_STOP")]
    public float? DeltaDistDestBossStartRunForCoverWithStop { get; set; }

    [JsonPropertyName("SANITAR_ONLY_FIGHT_COVERS")]
    public bool? SanitarOnlyFightCovers { get; set; }

    [JsonPropertyName("SANITAR_TWO_COVER_TACTIC")]
    public bool? SanitarTwoCoverTactic { get; set; }

    [JsonPropertyName("COUNT_FOLLOWERS_TO_WARN")]
    public int? CountFollowersToWarn { get; set; }

    [JsonPropertyName("RUN_HIDE_CAN_USE_TREE_COVRES")]
    public bool? RunHideCanUseTreeCovres { get; set; }

    [JsonPropertyName("SECTANT_INDOOR_DIST_NOT_TO_ATTACK")]
    public float? SectantIndoorDistNotToAttack { get; set; }

    [JsonPropertyName("SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY")]
    public bool? SetCheatVisibleWhenAddToEnemy { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL")]
    public float? TotalTimeKill { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL_AFTER_WARN")]
    public float? TotalTimeKillAfterWarn { get; set; }

    [JsonPropertyName("COME_INSIDE_TIMES")]
    public int? ComeInsideTimes { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT_DELTA")]
    public float? BossDistToWarningOutDelta { get; set; }

    [JsonPropertyName("BOSS_DIST_TO_WARNING_OUT_DELTA_ALLY")]
    public float? BossDistToWarningOutDeltaAlly { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL_AFTER_START_WARN")]
    public float? TotalTimeKillAfterStartWarn { get; set; }

    [JsonPropertyName("BIG_PIPE_ARTILLERY_COUNT")]
    public int? BigPipeArtilleryCount { get; set; }

    [JsonPropertyName("BOSS_ZRYACHIY_TELEPORT_CHANCE")]
    public float? BossZryachiyTeleportChance { get; set; }

    [JsonPropertyName("BOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT")]
    public float? BossZryachiyMinDistToTeleport { get; set; }

    [JsonPropertyName("BOSS_ZRYACHIY_TELEPORT_CAN_SECONDS_FROM_START")]
    public float? BossZryachiyTeleportCanSecondsFromStart { get; set; }

    [JsonPropertyName("BOSS_ZRYACHIY_MIN_DIST_TO_NEXT_COVER")]
    public float? BossZryachiyMinDistToNextCover { get; set; }

    [JsonPropertyName("BOSS_ZRYACHIY_POSSIBLE_FOG")]
    public float? BossZryachiyPossibleFog { get; set; }

    [JsonPropertyName("NOT_ADD_TO_ENEMY_ON_KILLS")]
    public bool? NotAddToEnemyOnKills { get; set; }

    [JsonPropertyName("ALLOW_REQUEST_SELF")]
    public bool? AllowRequestSelf { get; set; }

    [JsonPropertyName("TAGILLA_MELEE_ATTACK_IF_NO_SHOOT")]
    public float? TagillaMeleeAttackIfNoShoot { get; set; }
}
