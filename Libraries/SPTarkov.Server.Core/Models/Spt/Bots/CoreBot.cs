using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record CoreBot
{
    [JsonPropertyName("SAVAGE_KILL_DIST")]
    public required int SAVAGEKILLDIST { get; set; }

    [JsonPropertyName("SOUND_DOOR_BREACH_METERS")]
    public required double SOUNDDOORBREACHMETERS { get; set; }

    [JsonPropertyName("SOUND_DOOR_OPEN_METERS")]
    public required double SOUNDDOOROPENMETERS { get; set; }

    [JsonPropertyName("STEP_NOISE_DELTA")]
    public required double STEPNOISEDELTA { get; set; }

    [JsonPropertyName("JUMP_NOISE_DELTA")]
    public required double JUMPNOISEDELTA { get; set; }

    [JsonPropertyName("GUNSHOT_SPREAD")]
    public required int GUNSHOTSPREAD { get; set; }

    [JsonPropertyName("GUNSHOT_SPREAD_SILENCE")]
    public required int GUNSHOTSPREADSILENCE { get; set; }

    [JsonPropertyName("BASE_WALK_SPEREAD2")]
    public required double BASEWALKSPEREAD2 { get; set; }

    [JsonPropertyName("MOVE_SPEED_COEF_MAX")]
    public required double MOVESPEEDCOEFMAX { get; set; }

    [JsonPropertyName("SPEED_SERV_SOUND_COEF_A")]
    public required double SPEEDSERVSOUNDCOEFA { get; set; }

    [JsonPropertyName("SPEED_SERV_SOUND_COEF_B")]
    public required double SPEEDSERVSOUNDCOEFB { get; set; }

    [JsonPropertyName("G")]
    public required double G { get; set; }

    [JsonPropertyName("STAY_COEF")]
    public required double STAYCOEF { get; set; }

    [JsonPropertyName("SIT_COEF")]
    public required double SITCOEF { get; set; }

    [JsonPropertyName("LAY_COEF")]
    public required double LAYCOEF { get; set; }

    [JsonPropertyName("MAX_ITERATIONS")]
    public required int MAXITERATIONS { get; set; }

    [JsonPropertyName("START_DIST_TO_COV")]
    public required double STARTDISTTOCOV { get; set; }

    [JsonPropertyName("MAX_DIST_TO_COV")]
    public required double MAXDISTTOCOV { get; set; }

    [JsonPropertyName("STAY_HEIGHT")]
    public required double STAYHEIGHT { get; set; }

    [JsonPropertyName("CLOSE_POINTS")]
    public required double CLOSEPOINTS { get; set; }

    [JsonPropertyName("COUNT_TURNS")]
    public required int COUNTTURNS { get; set; }

    [JsonPropertyName("SIMPLE_POINT_LIFE_TIME_SEC")]
    public required double SIMPLEPOINTLIFETIMESEC { get; set; }

    [JsonPropertyName("DANGER_POINT_LIFE_TIME_SEC")]
    public required double DANGERPOINTLIFETIMESEC { get; set; }

    [JsonPropertyName("DANGER_POWER")]
    public required double DANGERPOWER { get; set; }

    [JsonPropertyName("COVER_DIST_CLOSE")]
    public required double COVERDISTCLOSE { get; set; }

    [JsonPropertyName("GOOD_DIST_TO_POINT")]
    public required double GOODDISTTOPOINT { get; set; }

    [JsonPropertyName("COVER_TOOFAR_FROM_BOSS")]
    public required double COVERTOOFARFROMBOSS { get; set; }

    [JsonPropertyName("COVER_TOOFAR_FROM_BOSS_SQRT")]
    public required double COVERTOOFARFROMBOSSSQRT { get; set; }

    [JsonPropertyName("MAX_Y_DIFF_TO_PROTECT")]
    public required double MAXYDIFFTOPROTECT { get; set; }

    [JsonPropertyName("FLARE_POWER")]
    public required double FLAREPOWER { get; set; }

    [JsonPropertyName("MOVE_COEF")]
    public required double MOVECOEF { get; set; }

    [JsonPropertyName("PRONE_POSE")]
    public required double PRONEPOSE { get; set; }

    [JsonPropertyName("LOWER_POSE")]
    public required int LOWERPOSE { get; set; }

    [JsonPropertyName("MAX_POSE")]
    public required double MAXPOSE { get; set; }

    [JsonPropertyName("FLARE_TIME")]
    public required double FLARETIME { get; set; }

    [JsonPropertyName("MAX_REQUESTS__PER_GROUP")]
    public required int MAXREQUESTSPERGROUP { get; set; }

    [JsonPropertyName("UPDATE_GOAL_TIMER_SEC")]
    public required double UPDATEGOALTIMERSEC { get; set; }

    [JsonPropertyName("DIST_NOT_TO_GROUP")]
    public required double DISTNOTTOGROUP { get; set; }

    [JsonPropertyName("DIST_NOT_TO_GROUP_SQR")]
    public required double DISTNOTTOGROUPSQR { get; set; }

    [JsonPropertyName("LAST_SEEN_POS_LIFETIME")]
    public required double LASTSEENPOSLIFETIME { get; set; }

    [JsonPropertyName("DELTA_GRENADE_START_TIME")]
    public required double DELTAGRENADESTARTTIME { get; set; }

    [JsonPropertyName("DELTA_GRENADE_END_TIME")]
    public required double DELTAGRENADEENDTIME { get; set; }

    [JsonPropertyName("DELTA_GRENADE_RUN_DIST")]
    public required int DELTAGRENADERUNDIST { get; set; }

    [JsonPropertyName("DELTA_GRENADE_RUN_DIST_SQRT")]
    public required double DELTAGRENADERUNDISTSQRT { get; set; }

    [JsonPropertyName("PATROL_MIN_LIGHT_DIST")]
    public required double PATROLMINLIGHTDIST { get; set; }

    [JsonPropertyName("HOLD_MIN_LIGHT_DIST")]
    public required double HOLDMINLIGHTDIST { get; set; }

    [JsonPropertyName("STANDART_BOT_PAUSE_DOOR")]
    public required double STANDARTBOTPAUSEDOOR { get; set; }

    [JsonPropertyName("ARMOR_CLASS_COEF")]
    public required double ARMORCLASSCOEF { get; set; }

    [JsonPropertyName("SHOTGUN_POWER")]
    public required double SHOTGUNPOWER { get; set; }

    [JsonPropertyName("RIFLE_POWER")]
    public required double RIFLEPOWER { get; set; }

    [JsonPropertyName("PISTOL_POWER")]
    public required double PISTOLPOWER { get; set; }

    [JsonPropertyName("SMG_POWER")]
    public required double SMGPOWER { get; set; }

    [JsonPropertyName("SNIPE_POWER")]
    public required double SNIPEPOWER { get; set; }

    [JsonPropertyName("GESTUS_PERIOD_SEC")]
    public required double GESTUSPERIODSEC { get; set; }

    [JsonPropertyName("GESTUS_AIMING_DELAY")]
    public required double GESTUSAIMINGDELAY { get; set; }

    [JsonPropertyName("GESTUS_REQUEST_LIFETIME")]
    public required double GESTUSREQUESTLIFETIME { get; set; }

    [JsonPropertyName("GESTUS_FIRST_STAGE_MAX_TIME")]
    public required double GESTUSFIRSTSTAGEMAXTIME { get; set; }

    [JsonPropertyName("GESTUS_SECOND_STAGE_MAX_TIME")]
    public required double GESTUSSECONDSTAGEMAXTIME { get; set; }

    [JsonPropertyName("GESTUS_MAX_ANSWERS")]
    public required int GESTUSMAXANSWERS { get; set; }

    [JsonPropertyName("GESTUS_FUCK_TO_SHOOT")]
    public required int GESTUSFUCKTOSHOOT { get; set; }

    [JsonPropertyName("GESTUS_DIST_ANSWERS")]
    public required double GESTUSDISTANSWERS { get; set; }

    [JsonPropertyName("GESTUS_DIST_ANSWERS_SQRT")]
    public required double GESTUSDISTANSWERSSQRT { get; set; }

    [JsonPropertyName("GESTUS_ANYWAY_CHANCE")]
    public required double GESTUSANYWAYCHANCE { get; set; }

    [JsonPropertyName("TALK_DELAY")]
    public required double TALKDELAY { get; set; }

    [JsonPropertyName("CAN_SHOOT_TO_HEAD")]
    public required bool CANSHOOTTOHEAD { get; set; }

    [JsonPropertyName("CAN_TILT")]
    public required bool CANTILT { get; set; }

    [JsonPropertyName("TILT_CHANCE")]
    public required double TILTCHANCE { get; set; }

    [JsonPropertyName("MIN_BLOCK_DIST")]
    public required double MINBLOCKDIST { get; set; }

    [JsonPropertyName("MIN_BLOCK_TIME")]
    public required double MINBLOCKTIME { get; set; }

    [JsonPropertyName("COVER_SECONDS_AFTER_LOSE_VISION")]
    public required double COVERSECONDSAFTERLOSEVISION { get; set; }

    [JsonPropertyName("MIN_ARG_COEF")]
    public required double MINARGCOEF { get; set; }

    [JsonPropertyName("MAX_ARG_COEF")]
    public required double MAXARGCOEF { get; set; }

    [JsonPropertyName("DEAD_AGR_DIST")]
    public required double DEADAGRDIST { get; set; }

    [JsonPropertyName("MAX_DANGER_CARE_DIST_SQRT")]
    public required double MAXDANGERCAREDISTSQRT { get; set; }

    [JsonPropertyName("MAX_DANGER_CARE_DIST")]
    public required double MAXDANGERCAREDIST { get; set; }

    [JsonPropertyName("MIN_MAX_PERSON_SEARCH")]
    public required int MINMAXPERSONSEARCH { get; set; }

    [JsonPropertyName("PERCENT_PERSON_SEARCH")]
    public required double PERCENTPERSONSEARCH { get; set; }

    [JsonPropertyName("LOOK_ANYSIDE_BY_WALL_SEC_OF_ENEMY")]
    public required double LOOKANYSIDEBYWALLSECOFENEMY { get; set; }

    [JsonPropertyName("CLOSE_TO_WALL_ROTATE_BY_WALL_SQRT")]
    public required double CLOSETOWALLROTATEBYWALLSQRT { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_MIN")]
    public required int SHOOTTOCHANGERNDPARTMIN { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_MAX")]
    public required int SHOOTTOCHANGERNDPARTMAX { get; set; }

    [JsonPropertyName("SHOOT_TO_CHANGE_RND_PART_DELTA")]
    public required double SHOOTTOCHANGERNDPARTDELTA { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_DIST")]
    public required double FORMULCOEFDELTADIST { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_SHOOT")]
    public required double FORMULCOEFDELTASHOOT { get; set; }

    [JsonPropertyName("FORMUL_COEF_DELTA_FRIEND_COVER")]
    public required double FORMULCOEFDELTAFRIENDCOVER { get; set; }

    [JsonPropertyName("SUSPETION_POINT_DIST_CHECK")]
    public required double SUSPETIONPOINTDISTCHECK { get; set; }

    [JsonPropertyName("MAX_BASE_REQUESTS_PER_PLAYER")]
    public required int MAXBASEREQUESTSPERPLAYER { get; set; }

    [JsonPropertyName("MAX_HOLD_REQUESTS_PER_PLAYER")]
    public required int MAXHOLDREQUESTSPERPLAYER { get; set; }

    [JsonPropertyName("MAX_GO_TO_REQUESTS_PER_PLAYER")]
    public required int MAXGOTOREQUESTSPERPLAYER { get; set; }

    [JsonPropertyName("MAX_COME_WITH_ME_REQUESTS_PER_PLAYER")]
    public required int MAXCOMEWITHMEREQUESTSPERPLAYER { get; set; }

    [JsonPropertyName("CORE_POINT_MAX_VALUE")]
    public required double COREPOINTMAXVALUE { get; set; }

    [JsonPropertyName("CORE_POINTS_MAX")]
    public required int COREPOINTSMAX { get; set; }

    [JsonPropertyName("CORE_POINTS_MIN")]
    public required int COREPOINTSMIN { get; set; }

    [JsonPropertyName("BORN_POISTS_FREE_ONLY_FAREST_BOT")]
    public required bool BORNPOISTSFREEONLYFARESTBOT { get; set; }

    [JsonPropertyName("BORN_POINSTS_FREE_ONLY_FAREST_PLAYER")]
    public required bool BORNPOINSTSFREEONLYFARESTPLAYER { get; set; }

    [JsonPropertyName("SCAV_GROUPS_TOGETHER")]
    public required bool SCAVGROUPSTOGETHER { get; set; }

    [JsonPropertyName("LAY_DOWN_ANG_SHOOT")]
    public required double LAYDOWNANGSHOOT { get; set; }

    [JsonPropertyName("HOLD_REQUEST_TIME_SEC")]
    public required double HOLDREQUESTTIMESEC { get; set; }

    [JsonPropertyName("TRIGGERS_DOWN_TO_RUN_WHEN_MOVE")]
    public required int TRIGGERSDOWNTORUNWHENMOVE { get; set; }

    [JsonPropertyName("MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING")]
    public required double MINDISTTORUNWHILEATTACKMOVING { get; set; }

    [JsonPropertyName("MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING_OTHER_ENEMIS")]
    public required double MINDISTTORUNWHILEATTACKMOVINGOTHERENEMIS { get; set; }

    [JsonPropertyName("MIN_DIST_TO_STOP_RUN")]
    public required double MINDISTTOSTOPRUN { get; set; }

    [JsonPropertyName("JUMP_SPREAD_DIST")]
    public required double JUMPSPREADDIST { get; set; }

    [JsonPropertyName("LOOK_TIMES_TO_KILL")]
    public required int LOOKTIMESTOKILL { get; set; }

    [JsonPropertyName("COME_INSIDE_TIMES")]
    public required int COMEINSIDETIMES { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL")]
    public required double TOTALTIMEKILL { get; set; }

    [JsonPropertyName("TOTAL_TIME_KILL_AFTER_WARN")]
    public required double TOTALTIMEKILLAFTERWARN { get; set; }

    [JsonPropertyName("MOVING_AIM_COEF")]
    public required double MOVINGAIMCOEF { get; set; }

    [JsonPropertyName("VERTICAL_DIST_TO_IGNORE_SOUND")]
    public required double VERTICALDISTTOIGNORESOUND { get; set; }

    [JsonPropertyName("DEFENCE_LEVEL_SHIFT")]
    public required double DEFENCELEVELSHIFT { get; set; }

    [JsonPropertyName("MIN_DIST_CLOSE_DEF")]
    public required double MINDISTCLOSEDEF { get; set; }

    [JsonPropertyName("USE_ID_PRIOR_WHO_GO")]
    public required bool USEIDPRIORWHOGO { get; set; }

    [JsonPropertyName("START_ACTIVE_FOLLOW_PLAYER_EVENT")]
    public required bool STARTACTIVEFOLLOWPLAYEREVENT { get; set; }

    [JsonPropertyName("START_ACTIVE_FORCE_ATTACK_PLAYER_EVENT")]
    public required bool STARTACTIVEFORCEATTACKPLAYEREVENT { get; set; }

    [JsonPropertyName("SMOKE_GRENADE_RADIUS_COEF")]
    public required double SMOKEGRENADERADIUSCOEF { get; set; }

    [JsonPropertyName("GRENADE_PRECISION")]
    public required int GRENADEPRECISION { get; set; }

    [JsonPropertyName("MAX_WARNS_BEFORE_KILL")]
    public required int MAXWARNSBEFOREKILL { get; set; }

    [JsonPropertyName("CARE_ENEMY_ONLY_TIME")]
    public required double CAREENEMYONLYTIME { get; set; }

    [JsonPropertyName("MIDDLE_POINT_COEF")]
    public required double MIDDLEPOINTCOEF { get; set; }

    [JsonPropertyName("MAIN_TACTIC_ONLY_ATTACK")]
    public required bool MAINTACTICONLYATTACK { get; set; }

    [JsonPropertyName("LAST_DAMAGE_ACTIVE")]
    public required double LASTDAMAGEACTIVE { get; set; }

    [JsonPropertyName("SHALL_DIE_IF_NOT_INITED")]
    public required bool SHALLDIEIFNOTINITED { get; set; }

    [JsonPropertyName("CHECK_BOT_INIT_TIME_SEC")]
    public required double CHECKBOTINITTIMESEC { get; set; }

    [JsonPropertyName("WEAPON_ROOT_Y_OFFSET")]
    public required double WEAPONROOTYOFFSET { get; set; }

    [JsonPropertyName("DELTA_SUPRESS_DISTANCE_SQRT")]
    public required double DELTASUPRESSDISTANCESQRT { get; set; }

    [JsonPropertyName("DELTA_SUPRESS_DISTANCE")]
    public required double DELTASUPRESSDISTANCE { get; set; }

    [JsonPropertyName("WAVE_COEF_LOW")]
    public required double WAVECOEFLOW { get; set; }

    [JsonPropertyName("WAVE_COEF_MID")]
    public required double WAVECOEFMID { get; set; }

    [JsonPropertyName("WAVE_COEF_HIGH")]
    public required double WAVECOEFHIGH { get; set; }

    [JsonPropertyName("WAVE_COEF_HORDE")]
    public required double WAVECOEFHORDE { get; set; }

    [JsonPropertyName("WAVE_ONLY_AS_ONLINE")]
    public required bool WAVEONLYASONLINE { get; set; }

    [JsonPropertyName("LOCAL_BOTS_COUNT")]
    public required int LOCALBOTSCOUNT { get; set; }

    /// <summary>
    /// Default = 4
    /// </summary>
    [JsonPropertyName("AXE_MAN_KILLS_END")]
    public required int AXEMANKILLSEND { get; set; }

    [JsonPropertyName("ACTIVE_HALLOWEEN_ZOMBIES_EVENT")]
    public bool? ActiveHalloweenZombiesEvent { get; set; }

    /// <summary>
    /// Christmas/rudans related
    /// </summary>
    [JsonPropertyName("ACTIVE_PATROL_GENERATOR_EVENT")]
    public bool? ActivePatrolGeneratorEvent { get; set; }

    /// <summary>
    /// Weather related?
    /// </summary>
    [JsonPropertyName("ACTIVE_FORCE_ATTACK_EVENTS")]
    public bool? ActiveForceAttackEvents { get; set; }

    [JsonPropertyName("ACTIVE_FORCE_ATTACK_EVENTS_HOUR_START")]
    public int? ACTIVE_FORCE_ATTACK_EVENTS_HOUR_START { get; set; }
}
