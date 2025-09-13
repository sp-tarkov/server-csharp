using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalShootData in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalShootData
{
    [JsonPropertyName("SWITCH_TO_UNDERBARREL_WEAPON_COOLDOWN")]
    public float? SwitchToUnderbarrelWeaponCooldown { get; set; }

    [JsonPropertyName("MIN_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER")]
    public float? MinTimeToCheckForceSwitchToGrenadeLauncher { get; set; }

    [JsonPropertyName("MAX_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER")]
    public float? MaxTimeToCheckForceSwitchToGrenadeLauncher { get; set; }

    [JsonPropertyName("MAX_TIME_SEEN_TO_SWITCH_TO_GRENADE_LAUNCHER")]
    public float? MaxTimeSeenToSwitchToGrenadeLauncher { get; set; }

    [JsonPropertyName("MAX_SUCCESS_GRENADE_LAUNCHER_SHOOT_ATTEMPTS")]
    public uint MaxSuccessGrenadeLauncherShootAttempts { get; set; }

    [JsonPropertyName("SHOOT_PROBABILITY_GRENADE_LAUNCHER")]
    public float? ShootProbabilityGrenadeLauncher { get; set; }

    [JsonPropertyName("LOW_DIST_K_FOR_GRENADE_LAUNCHER")]
    public float? LowDistKForGrenadeLauncher { get; set; }

    [JsonPropertyName("DEFAULT_LOW_DIST_TO_USE_GRENADE_LAUNCHER")]
    public float? DefaultLowDistToUseGrenadeLauncher { get; set; }

    [JsonPropertyName("DISTANCE_TO_TARGET_NEAR_ENEMY_TRESHOLD")]
    public float? DistanceToTargetNearEnemyTreshold { get; set; }

    [JsonPropertyName("DISTANCE_TO_TARGET_NEAR_ENEMY_DEVIATION")]
    public float? DistanceToTargetNearEnemyDeviation { get; set; }

    [JsonPropertyName("RECOIL_TIME_NORMALIZE")]
    public float? RecoilTimeNormalize { get; set; }

    [JsonPropertyName("RECOIL_PER_METER")]
    public float? RecoilPerMeter { get; set; }

    [JsonPropertyName("MAX_RECOIL_PER_METER")]
    public float? MaxRecoilPerMeter { get; set; }

    [JsonPropertyName("HORIZONT_RECOIL_COEF")]
    public float? HorizontRecoilCoef { get; set; }

    [JsonPropertyName("WAIT_NEXT_SINGLE_SHOT")]
    public float? WaitNextSingleShot { get; set; }

    [JsonPropertyName("WAIT_NEXT_STATIONARY_BULLET")]
    public float? WaitNextStationaryBullet { get; set; }

    [JsonPropertyName("WAIT_NEXT_STATIONARY_GRENADE")]
    public float? WaitNextStationaryGrenade { get; set; }

    [JsonPropertyName("WAIT_NEXT_SINGLE_SHOT_LONG_MAX")]
    public float? WaitNextSingleShotLongMax { get; set; }

    [JsonPropertyName("WAIT_NEXT_SINGLE_SHOT_LONG_MIN")]
    public float? WaitNextSingleShotLongMin { get; set; }

    [JsonPropertyName("NEXT_SINGLE_SHOT_PAUSE")]
    public float? NextSingleShotPause { get; set; }

    [JsonPropertyName("SINGLE_SHOT_SERIES_TIME_MIN")]
    public float? SingleShotSeriesTimeMin { get; set; }

    [JsonPropertyName("SINGLE_SHOT_SERIES_TIME_MAX")]
    public float? SingleShotSeriesTimeMax { get; set; }

    [JsonPropertyName("USE_SINGLE_SHOT_SERIES")]
    public bool? UseSingleShotSeries { get; set; }

    [JsonPropertyName("MARKSMAN_DIST_SEK_COEF")]
    public float? MarksmanDistSekCoef { get; set; }

    [JsonPropertyName("FINGER_HOLD_SINGLE_SHOT")]
    public float? FingerHoldSingleShot { get; set; }

    [JsonPropertyName("FINGER_HOLD_STATIONARY_BULLET")]
    public float? FingerHoldStationaryBullet { get; set; }

    [JsonPropertyName("FINGER_HOLD_STATIONARY_GRENADE")]
    public float? FingerHoldStationaryGrenade { get; set; }

    [JsonPropertyName("BASE_AUTOMATIC_TIME")]
    public float? BaseAutomaticTime { get; set; }

    [JsonPropertyName("AUTOMATIC_FIRE_SCATTERING_COEF")]
    public float? AutomaticFireScatteringCoef { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100")]
    public float? ChanceToChangeToAutomaticFire100 { get; set; }

    [JsonPropertyName("FAR_DIST_ENEMY")]
    public float? FarDistEnemy { get; set; }

    [JsonPropertyName("SHOOT_FROM_COVER")]
    public int? ShootFromCover { get; set; }

    [JsonPropertyName("FAR_DIST_ENEMY_SQR")]
    public float? FarDistEnemySqr { get; set; }

    [JsonPropertyName("MAX_DIST_COEF")]
    public float? MaxDistCoef { get; set; }

    [JsonPropertyName("RECOIL_DELTA_PRESS")]
    public float? RecoilDeltaPress { get; set; }

    [JsonPropertyName("RUN_DIST_NO_AMMO")]
    public float? RunDistNoAmmo { get; set; }

    [JsonPropertyName("RUN_DIST_NO_AMMO_SQRT")]
    public float? RunDistNoAmmoSqrt { get; set; }

    [JsonPropertyName("CAN_SHOOTS_TIME_TO_AMBUSH")]
    public int? CanShootsTimeToAmbush { get; set; }

    [JsonPropertyName("NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_PERCENT")]
    public float? NotToSeeEnemyToWantReloadPercent { get; set; }

    [JsonPropertyName("NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_SEC")]
    public float? NotToSeeEnemyToWantReloadSec { get; set; }

    [JsonPropertyName("RELOAD_PECNET_NO_ENEMY")]
    public float? ReloadPecnetNoEnemy { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_WEAPON")]
    public float? ChanceToChangeWeapon { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_WEAPON_WITH_HELMET")]
    public float? ChanceToChangeWeaponWithHelmet { get; set; }

    [JsonPropertyName("LOW_DIST_TO_CHANGE_WEAPON")]
    public float? LowDistToChangeWeapon { get; set; }

    [JsonPropertyName("FAR_DIST_TO_CHANGE_WEAPON")]
    public float? FarDistToChangeWeapon { get; set; }

    [JsonPropertyName("SUPPRESS_BY_SHOOT_TIME")]
    public float? SuppressByShootTime { get; set; }

    [JsonPropertyName("SUPPRESS_TRIGGERS_DOWN")]
    public int? SuppressTriggersDown { get; set; }

    [JsonPropertyName("SUPPRESS_TRIGGERS_DOWN_AS_LIST")]
    public int? SuppressTriggersDownAsList { get; set; }

    [JsonPropertyName("DIST_TO_CHANGE_TO_MAIN")]
    public float? DistToChangeToMain { get; set; }

    [JsonPropertyName("AGS_17_DIST_TO_LEAVE")]
    public float? Ags17DistToLeave { get; set; }

    [JsonPropertyName("DIST_TO_HIT_MELEE")]
    public float? DistToHitMelee { get; set; }

    [JsonPropertyName("DIST_TO_HIT_MELEE_CONTINUE_COMBO")]
    public float? DistToHitMeleeContinueCombo { get; set; }

    [JsonPropertyName("DIST_TO_STOP_SPRINT_MELEE")]
    public float? DistToStopSprintMelee { get; set; }

    [JsonPropertyName("TRY_HIT_PERIOD_MELEE")]
    public float? TryHitPeriodMelee { get; set; }

    [JsonPropertyName("BLOCK_PERIOD_WHEN_LAY")]
    public float? BlockPeriodWhenLay { get; set; }

    [JsonPropertyName("CHANGE_WEAPON_PERIOD")]
    public float? ChangeWeaponPeriod { get; set; }

    [JsonPropertyName("USE_MELEE_COMBOS")]
    public bool? UseMeleeCombos { get; set; }

    [JsonPropertyName("MELEE_RESET_HIT_TIME")]
    public float? MeleeResetHitTime { get; set; }

    [JsonPropertyName("MELEE_STOP_MOVE_DISTANCE")]
    public float? MeleeStopMoveDistance { get; set; }

    [JsonPropertyName("VALIDATE_MALFUNCTION_CHANCE")]
    public int? ValidateMalfunctionChance { get; set; }

    [JsonPropertyName("REPAIR_MALFUNCTION_IMMEDIATE_CHANCE")]
    public int? RepairMalfunctionImmediateChance { get; set; }

    [JsonPropertyName("DELAY_BEFORE_EXAMINE_MALFUNCTION")]
    public float? DelayBeforeExamineMalfunction { get; set; }

    [JsonPropertyName("DELAY_BEFORE_FIX_MALFUNCTION")]
    public float? DelayBeforeFixMalfunction { get; set; }

    [JsonPropertyName("TRY_CHANGE_WEAPON_INSTEAD_RELOAD")]
    public bool? TryChangeWeaponInsteadReload { get; set; }

    [JsonPropertyName("MELEE_ATTACK_ZIG_ZAG")]
    public bool? MeleeAttackZigZag { get; set; }

    [JsonPropertyName("MIN_DIST_TO_ENEMY_TO_CHANGE_WEAPON_INSTEAD_RELOAD")]
    public float? MinDistToEnemyToChangeWeaponInsteadReload { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_WEAPON_INSTEAD_RELOAD")]
    public float? ChanceToChangeWeaponInsteadReload { get; set; }

    [JsonPropertyName("CHANCE_TO_CHANGE_WEAPON_INSTEAD_RELOAD_ENEMY_WITHOUT_HELM")]
    public float? ChanceToChangeWeaponInsteadReloadEnemyWithoutHelm { get; set; }

    [JsonPropertyName("MELEE_STOP_DIST")]
    public float? MeleeStopDist { get; set; }

    [JsonPropertyName("BLOCK_STEERING")]
    public bool? BlockSteering { get; set; }

    [JsonPropertyName("USE_BTR_CANSHOOT")]
    public bool? UseBtrCanshoot { get; set; }

    [JsonPropertyName("FAR_DISTANCE_ALL_WEAPONS")]
    public float? FarDistanceAllWeapons { get; set; }

    [JsonPropertyName("FAR_DISTANCE_PISTOLS")]
    public float? FarDistancePistols { get; set; }

    [JsonPropertyName("FAR_DISTANCE_SHOTGUNS")]
    public float? FarDistanceShotguns { get; set; }

    [JsonPropertyName("FAR_DIST_EYE_CONTACT_TIME_TO_CHANGE_COVER")]
    public float? FarDistEyeContactTimeToChangeCover { get; set; }

    [JsonPropertyName("CHANGE_TO_MAIN_WEAPON_WHEN_PATROL")]
    public bool? ChangeToMainWeaponWhenPatrol { get; set; }

    [JsonPropertyName("SHOOT_IMMEDIATELY_DIST")]
    public float? ShootImmediatelyDist { get; set; }

    [JsonPropertyName("CAN_STOP_SHOOT_CAUSE_ANIMATOR")]
    public bool? CanStopShootCauseAnimator { get; set; }

    [JsonPropertyName("TRY_CHANGE_WEAPON_WHEN_RELOAD")]
    public bool? TryChangeWeaponWhenReload { get; set; }

    [JsonPropertyName("CHANGE_TO_MAIN_WHEN_SUPPORT_NO_AMMO")]
    public bool? ChangeToMainWhenSupportNoAmmo { get; set; }

    [JsonPropertyName("LAST_SEEN_TIME_TO_START_SUPPRESS_STATIONARY_AGS")]
    public float? LastSeenTimeToStartSuppressStationaryAgs { get; set; }

    [JsonPropertyName("STATIONARY_GRENADE_MIN_DIST_TO_TAKE")]
    public float? StationaryGrenadeMinDistToTake { get; set; }

    [JsonPropertyName("STATIONARY_SIMPLE_MIN_DIST_TO_TAKE")]
    public double? StationarySimpleMinDistToTake { get; set; }

    [JsonPropertyName("NO_OFFSET_SHOOTING_FROM_PLAYER")]
    public bool? NoOffsetShootingFromPlayer { get; set; }

    [JsonPropertyName("ALTERNATIVE_KNIFE_KICK")]
    public bool? AlternativeKnifeKick { get; set; }

    [JsonPropertyName("DITANCE_TO_OFF_AUTO_FIRE")]
    public float? DistanceToOffAutoFire { get; set; }

    [JsonPropertyName("DITANCE_TO_ON_AUTO_FIRE")]
    public float? DistanceToOnAutoFire { get; set; }

    [JsonPropertyName("MISS_ON_CRITICAL_DIST")]
    public float? MissOnCriticalDist { get; set; }

    [JsonPropertyName("MISS_AFTER_SPRINT")]
    public bool? MissAfterSprint { get; set; }

    /// <summary>
    /// The bot misses the head
    /// </summary>
    [JsonPropertyName("MISS_TO_HEAD")]
    public bool? MissToHead { get; set; }

    /// <summary>
    /// The bot misses while moving
    /// </summary>
    [JsonPropertyName("MISS_ON_MOVE")]
    public bool? MissOnMove { get; set; }

    /// <summary>
    /// Bot misses during transition animations
    /// </summary>
    [JsonPropertyName("MISS_ON_TRANSITION")]
    public bool? MissOnTransition { get; set; }
}
