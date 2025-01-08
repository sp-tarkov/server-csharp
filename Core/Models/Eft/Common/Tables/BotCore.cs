using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class BotCore
{
    [JsonPropertyName("SAVAGE_KILL_DIST")]
    public double? SavageKillDistance { get; set; }

    [JsonPropertyName("SOUND_DOOR_BREACH_METERS")]
    public double? SoundDoorBreachMeters { get; set; }

    [JsonPropertyName("SOUND_DOOR_OPEN_METERS")]
    public double? SoundDoorOpenMeters { get; set; }

    [JsonPropertyName("STEP_NOISE_DELTA")]
    public double? StepNoiseDelta { get; set; }

    [JsonPropertyName("JUMP_NOISE_DELTA")]
    public double? JumpNoiseDelta { get; set; }

    [JsonPropertyName("GUNSHOT_SPREAD")]
    public double? GunshotSpread { get; set; }

    [JsonPropertyName("GUNSHOT_SPREAD_SILENCE")]
    public double? GunshotSpreadSilence { get; set; }

    [JsonPropertyName("BASE_WALK_SPEREAD2")]
    public double? BaseWalkSpread2 { get; set; }

    [JsonPropertyName("MOVE_SPEED_COEF_MAX")]
    public double? MoveSpeedCoefficientMax { get; set; }

    [JsonPropertyName("SPEED_SERV_SOUND_COEF_A")]
    public double? SpeedServiceSoundCoefficientA { get; set; }

    [JsonPropertyName("SPEED_SERV_SOUND_COEF_B")]
    public double? SpeedServiceSoundCoefficientB { get; set; }

    [JsonPropertyName("G")]
    public double? Gravity { get; set; }

    [JsonPropertyName("STAY_COEF")]
    public double? StayCoefficient { get; set; }

    [JsonPropertyName("SIT_COEF")]
    public double? SitCoefficient { get; set; }

    [JsonPropertyName("LAY_COEF")]
    public double? LayCoefficient { get; set; }

    [JsonPropertyName("MAX_ITERATIONS")]
    public double? MaxIterations { get; set; }

    [JsonPropertyName("START_DIST_TO_COV")]
    public double? StartDistanceToCover { get; set; }

    [JsonPropertyName("MAX_DIST_TO_COV")]
    public double? MaxDistanceToCover { get; set; }

    [JsonPropertyName("STAY_HEIGHT")]
    public double? StayHeight { get; set; }

    [JsonPropertyName("CLOSE_POINTS")]
    public double? ClosePoints { get; set; }

    [JsonPropertyName("COUNT_TURNS")]
    public double? CountTurns { get; set; }

    [JsonPropertyName("SIMPLE_POINT_LIFE_TIME_SEC")]
    public double? SimplePointLifetimeSeconds { get; set; }

    [JsonPropertyName("DANGER_POINT_LIFE_TIME_SEC")]
    public double? DangerPointLifetimeSeconds { get; set; }

    [JsonPropertyName("DANGER_POWER")]
    public double? DangerPower { get; set; }

    [JsonPropertyName("COVER_DIST_CLOSE")]
    public double? CoverDistanceClose { get; set; }

    [JsonPropertyName("GOOD_DIST_TO_POINT")]
    public double GoodDistanceToPoint { get; set; }

    [JsonPropertyName("COVER_TOOFAR_FROM_BOSS")]
    public double CoverTooFarFromBoss { get; set; }

    [JsonPropertyName("COVER_TOOFAR_FROM_BOSS_SQRT")]
    public double CoverTooFarFromBossSqrt { get; set; }

    [JsonPropertyName("MAX_Y_DIFF_TO_PROTECT")]
    public double MaxYDifferenceToProtect { get; set; }

    [JsonPropertyName("FLARE_POWER")]
    public double FlarePower { get; set; }

    [JsonPropertyName("MOVE_COEF")]
    public double MoveCoefficient { get; set; }

    [JsonPropertyName("PRONE_POSE")]
    public double PronePose { get; set; }

    [JsonPropertyName("LOWER_POSE")]
    public double LowerPose { get; set; }

    [JsonPropertyName("MAX_POSE")]
    public double MaxPose { get; set; }

    [JsonPropertyName("FLARE_TIME")]
    public double FlareTime { get; set; }

    [JsonPropertyName("MAX_REQUESTS__PER_GROUP")]
    public double MaxRequestsPerGroup { get; set; }

    [JsonPropertyName("UPDATE_GOAL_TIMER_SEC")]
    public double UpdateGoalTimerSeconds { get; set; }

    [JsonPropertyName("DIST_NOT_TO_GROUP")]
    public double DistanceNotToGroup { get; set; }

    [JsonPropertyName("DIST_NOT_TO_GROUP_SQR")]
    public double DistanceNotToGroupSquared { get; set; }

    [JsonPropertyName("LAST_SEEN_POS_LIFETIME")]
    public double LastSeenPositionLifetime { get; set; }

    [JsonPropertyName("DELTA_GRENADE_START_TIME")]
    public double DeltaGrenadeStartTime { get; set; }

    [JsonPropertyName("DELTA_GRENADE_END_TIME")]
    public double DeltaGrenadeEndTime { get; set; }

    [JsonPropertyName("DELTA_GRENADE_RUN_DIST")]
    public double DeltaGrenadeRunDistance { get; set; }

    [JsonPropertyName("DELTA_GRENADE_RUN_DIST_SQRT")]
    public double DeltaGrenadeRunDistanceSqrt { get; set; }

    [JsonPropertyName("PATROL_MIN_LIGHT_DIST")]
    public double PatrolMinimumLightDistance { get; set; }

    [JsonPropertyName("HOLD_MIN_LIGHT_DIST")]
    public double HoldMinimumLightDistance { get; set; }

    [JsonPropertyName("STANDART_BOT_PAUSE_DOOR")]
    public double StandardBotPauseDoor { get; set; }

    [JsonPropertyName("ARMOR_CLASS_COEF")]
    public double ArmorClassCoefficient { get; set; }

    [JsonPropertyName("SHOTGUN_POWER")]
    public double ShotgunPower { get; set; }

    [JsonPropertyName("RIFLE_POWER")]
    public double RiflePower { get; set; }

    [JsonPropertyName("PISTOL_POWER")]
    public double PistolPower { get; set; }

    [JsonPropertyName("SMG_POWER")]
    public double SMGPower { get; set; }

    [JsonPropertyName("SNIPE_POWER")]
    public double SniperPower { get; set; }

    [JsonPropertyName("GESTUS_PERIOD_SEC")]
    public double GestusPeriodSeconds { get; set; }

    [JsonPropertyName("GESTUS_AIMING_DELAY")]
    public double GestusAimingDelay { get; set; }

    [JsonPropertyName("GESTUS_REQUEST_LIFETIME")]
    public double GestusRequestLifetime { get; set; }

    [JsonPropertyName("GESTUS_FIRST_STAGE_MAX_TIME")]
    public double GestusFirstStageMaxTime { get; set; }

    [JsonPropertyName("GESTUS_SECOND_STAGE_MAX_TIME")]
    public double GestusSecondStageMaxTime { get; set; }

    [JsonPropertyName("GESTUS_MAX_ANSWERS")]
    public double GestusMaxAnswers { get; set; }

    [JsonPropertyName("GESTUS_FUCK_TO_SHOOT")]
    public double GestusFuckToShoot { get; set; }

    [JsonPropertyName("GESTUS_DIST_ANSWERS")]
    public double GestusDistanceAnswers { get; set; }

    [JsonPropertyName("GESTUS_DIST_ANSWERS_SQRT")]
    public double GestusDistanceAnswersSqrt { get; set; }

    [JsonPropertyName("GESTUS_ANYWAY_CHANCE")]
    public double GestusAnywayChance { get; set; }

    [JsonPropertyName("TALK_DELAY")]
    public double TalkDelay { get; set; }

    [JsonPropertyName("CAN_SHOOT_TO_HEAD")]
    public bool CanShootToHead { get; set; }

    [JsonPropertyName("CAN_TILT")]
    public bool CanTilt { get; set; }

    [JsonPropertyName("TILT_CHANCE")]
    public double TiltChance { get; set; }

    [JsonPropertyName("MIN_BLOCK_DIST")]
    public double MinimumBlockDistance { get; set; }

    [JsonPropertyName("MIN_BLOCK_TIME")]
    public double MinimumBlockTime { get; set; }

    [JsonPropertyName("COVER_SECONDS_AFTER_LOSE_VISION")]
    public double CoverSecondsAfterLoseVision { get; set; }

    [JsonPropertyName("MIN_ARG_COEF")]
    public double MinimumArgumentCoefficient { get; set; }

    [JsonPropertyName("MAX_ARG_COEF")]
    public double MaximumArgumentCoefficient { get; set; }

    [JsonPropertyName("DEAD_AGR_DIST")]
    public double DeadAgrDistance { get; set; }

    [JsonPropertyName("MAX_DANGER_CARE_DIST_SQRT")]
    public double MaxDangerCareDistanceSqrt { get; set; }

    [JsonPropertyName("MAX_DANGER_CARE_DIST")]
    public double MaxDangerCareDistance { get; set; }

    [JsonPropertyName("MIN_MAX_PERSON_SEARCH")]
    public double MinimumMaximumPersonSearch { get; set; }

    [JsonPropertyName("PERCENT_PERSON_SEARCH")]
    public double PercentPersonSearch { get; set; }

    [JsonPropertyName("LOOK_ANYSIDE_BY_WALL_SEC_OF_ENEMY")]
    public double LookAnySideByWallSecondsOfEnemy { get; set; }

    [JsonPropertyName("CLOSE_TO_WALL_ROTATE_BY_WALL_SQRT")]
    public double CloseToWallRotateByWallSqrt { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_MIN")]
    public double ShootToChangeRandomPartMinimum { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_MAX")]
    public double ShootToChangeRandomPartMaximum { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_DELTA")]
    public double ShootToChangeRandomPartDelta { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_DIST")]
    public double FormulaCoefficientDeltaDistance { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_SHOOT")]
    public double FormulaCoefficientDeltaShoot { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_FRIEND_COVER")]
    public double FormulaCoefficientDeltaFriendCover { get; set; }

    [JsonPropertyName("SUSPETION_POINT_DIST_CHECK")]
    public double SuspicionPointDistanceCheck { get; set; }

    [JsonPropertyName("MAX_BASE_REQUESTS_PER_PLAYER")]
    public double MaxBaseRequestsPerPlayer { get; set; }

    [JsonPropertyName("MAX_HOLD_REQUESTS_PER_PLAYER")]
    public double MaxHoldRequestsPerPlayer { get; set; }

    [JsonPropertyName("MAX_GO_TO_REQUESTS_PER_PLAYER")]
    public double MaxGoToRequestsPerPlayer { get; set; }

    [JsonPropertyName("MAX_COME_WITH_ME_REQUESTS_PER_PLAYER")]
    public double MaxComeWithMeRequestsPerPlayer { get; set; }

    [JsonPropertyName("CORE_POINT_MAX_VALUE")]
    public double CorePointMaxValue { get; set; }

    [JsonPropertyName("CORE_POINTS_MAX")]
    public double CorePointsMax { get; set; }

    [JsonPropertyName("CORE_POINTS_MIN")]
    public double CorePointsMin { get; set; }

    [JsonPropertyName("BORN_POISTS_FREE_ONLY_FAREST_BOT")]
    public bool BornPointsFreeOnlyFarthestBot { get; set; }

    [JsonPropertyName("BORN_POINSTS_FREE_ONLY_FAREST_PLAYER")]
    public bool BornPointsFreeOnlyFarthestPlayer { get; set; }

    [JsonPropertyName("SCAV_GROUPS_TOGETHER")]
    public bool ScavGroupsTogether { get; set; }

    [JsonPropertyName("LAY_DOWN_ANG_SHOOT")]
    public double LayDownAngleShoot { get; set; }

    [JsonPropertyName("HOLD_REQUEST_TIME_SEC")]
    public double HoldRequestTimeSeconds { get; set; }

    [JsonPropertyName("TRIGGERS_DOWN_TO_RUN_WHEN_MOVE")]
    public double TriggersDownToRunWhenMove { get; set; }

    [JsonPropertyName("MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING")]
    public double MinimumDistanceToRunWhileAttackingMoving { get; set; }

    [JsonPropertyName("MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING_OTHER_ENEMIS")]
    public double MinimumDistanceToRunWhileAttackingMovingOtherEnemies { get; set; }

    [JsonPropertyName("MIN_DIST_TO_STOP_RUN")]
    public double MinimumDistanceToStopRunning { get; set; }

    [JsonPropertyName("JUMP_SPREAD_DIST")]
    public double JumpSpreadDistance { get; set; }

    [JsonPropertyName("LOOK_TIMES_TO_KILL")]
    public double LookTimesToKill { get; set; }

    [JsonPropertyName("COME_INSIDE_TIMES")]
    public double ComeInsideTimes { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL")]
    public double TotalTimeKill { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL_AFTER_WARN")]
    public double TotalTimeKillAfterWarning { get; set; }

    [JsonPropertyName("MOVING_AIM_COEF")]
    public double MovingAimCoefficient { get; set; }

    [JsonPropertyName("VERTICAL_DIST_TO_IGNORE_SOUND")]
    public double VerticalDistanceToIgnoreSound { get; set; }

    [JsonPropertyName("DEFENCE_LEVEL_SHIFT")]
    public double DefenseLevelShift { get; set; }

    [JsonPropertyName("MIN_DIST_CLOSE_DEF")]
    public double MinimumDistanceCloseDefense { get; set; }

    [JsonPropertyName("USE_ID_PRIOR_WHO_GO")]
    public bool UseIdPriorWhoGoes { get; set; }

    [JsonPropertyName("SMOKE_GRENADE_RADIUS_COEF")]
    public double SmokeGrenadeRadiusCoefficient { get; set; }

    [JsonPropertyName("GRENADE_PRECISION")]
    public double GrenadePrecision { get; set; }

    [JsonPropertyName("MAX_WARNS_BEFORE_KILL")]
    public double MaxWarningsBeforeKill { get; set; }

    [JsonPropertyName("CARE_ENEMY_ONLY_TIME")]
    public double CareEnemyOnlyTime { get; set; }

    [JsonPropertyName("MIDDLE_POINT_COEF")]
    public double MiddlePointCoefficient { get; set; }

    [JsonPropertyName("MAIN_TACTIC_ONLY_ATTACK")]
    public bool MainTacticOnlyAttack { get; set; }

    [JsonPropertyName("LAST_DAMAGE_ACTIVE")]
    public double LastDamageActive { get; set; }

    [JsonPropertyName("SHALL_DIE_IF_NOT_INITED")]
    public bool ShallDieIfNotInitialized { get; set; }

    [JsonPropertyName("CHECK_BOT_INIT_TIME_SEC")]
    public double CheckBotInitializationTimeSeconds { get; set; }

    [JsonPropertyName("WEAPON_ROOT_Y_OFFSET")]
    public double WeaponRootYOffset { get; set; }

    [JsonPropertyName("DELTA_SUPRESS_DISTANCE_SQRT")]
    public double DeltaSuppressDistanceSqrt { get; set; }

    [JsonPropertyName("DELTA_SUPRESS_DISTANCE")]
    public double DeltaSuppressDistance { get; set; }

    [JsonPropertyName("WAVE_COEF_LOW")]
    public double WaveCoefficientLow { get; set; }

    [JsonPropertyName("WAVE_COEF_MID")]
    public double WaveCoefficientMid { get; set; }

    [JsonPropertyName("WAVE_COEF_HIGH")]
    public double WaveCoefficientHigh { get; set; }

    [JsonPropertyName("WAVE_COEF_HORDE")]
    public double WaveCoefficientHorde { get; set; }

    [JsonPropertyName("WAVE_ONLY_AS_ONLINE")]
    public bool WaveOnlyAsOnline { get; set; }

    [JsonPropertyName("LOCAL_BOTS_COUNT")]
    public double LocalBotsCount { get; set; }

    [JsonPropertyName("AXE_MAN_KILLS_END")]
    public double AxeManKillsEnd { get; set; }
}