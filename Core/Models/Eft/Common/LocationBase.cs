using System.Text.Json.Serialization;
using Core.Models.Common;

namespace Core.Models.Eft.Common;

public class LocationBase
{
    [JsonPropertyName("AccessKeys")]
    public List<string>? AccessKeys { get; set; }

    [JsonPropertyName("AccessKeysPvE")]
    public List<string>? AccessKeysPvE { get; set; }

    [JsonPropertyName("AirdropParameters")]
    public List<AirdropParameter>? AirdropParameters { get; set; }

    [JsonPropertyName("NewSpawnForPlayers")]
    public bool? NewSpawnForPlayers { get; set; }

    [JsonPropertyName("OfflineNewSpawn")]
    public bool? OfflineNewSpawn { get; set; }

    [JsonPropertyName("OfflineOldSpawn")]
    public bool? OfflineOldSpawn { get; set; }

    [JsonPropertyName("Area")]
    public double? Area { get; set; }

    [JsonPropertyName("AveragePlayTime")]
    public double? AveragePlayTime { get; set; }

    [JsonPropertyName("AveragePlayerLevel")]
    public double? AveragePlayerLevel { get; set; }

    [JsonPropertyName("Banners")]
    public List<Banner>? Banners { get; set; }

    [JsonPropertyName("BossLocationSpawn")]
    public List<BossLocationSpawn>? BossLocationSpawn { get; set; }

    [JsonPropertyName("secretExits")]
    public List<Exit>? SecretExits { get; set; }

    [JsonPropertyName("BotStartPlayer")]
    public int? BotStartPlayer { get; set; }

    [JsonPropertyName("BotAssault")]
    public int? BotAssault { get; set; }

    /** Weighting on how likely a bot will be Easy difficulty */
    [JsonPropertyName("BotEasy")]
    public int? BotEasy { get; set; }

    /** Weighting on how likely a bot will be Hard difficulty */
    [JsonPropertyName("BotHard")]
    public int? BotHard { get; set; }

    /** Weighting on how likely a bot will be Impossible difficulty */
    [JsonPropertyName("BotImpossible")]
    public int? BotImpossible { get; set; }

    [JsonPropertyName("BotLocationModifier")]
    public BotLocationModifier? BotLocationModifier { get; set; }

    [JsonPropertyName("BotMarksman")]
    public int? BotMarksman { get; set; }

    /** Maximum Number of bots that are currently alive/loading/delayed */
    [JsonPropertyName("BotMax")]
    public int? BotMax { get; set; }

    /** Is not used in 33420 */
    [JsonPropertyName("BotMaxPlayer")]
    public int? BotMaxPlayer { get; set; }

    /** Is not used in 33420 */
    [JsonPropertyName("BotMaxTimePlayer")]
    public int? BotMaxTimePlayer { get; set; }

    /** Does not even exist in the client in 33420 */
    [JsonPropertyName("BotMaxPvE")]
    public int? BotMaxPvE { get; set; }

    /** Weighting on how likely a bot will be Normal difficulty */
    [JsonPropertyName("BotNormal")]
    public int? BotNormal { get; set; }

    /** How many bot slots that need to be open before trying to spawn new bots. */
    [JsonPropertyName("BotSpawnCountStep")]
    public int? BotSpawnCountStep { get; set; }

    /** How often to check if bots are spawn-able. In seconds */
    [JsonPropertyName("BotSpawnPeriodCheck")]
    public int? BotSpawnPeriodCheck { get; set; }

    /** The bot spawn will toggle on and off in intervals of Off(Min/Max) and On(Min/Max) */
    [JsonPropertyName("BotSpawnTimeOffMax")]
    public int? BotSpawnTimeOffMax { get; set; }

    [JsonPropertyName("BotSpawnTimeOffMin")]
    public int? BotSpawnTimeOffMin { get; set; }

    [JsonPropertyName("BotSpawnTimeOnMax")]
    public int? BotSpawnTimeOnMax { get; set; }

    [JsonPropertyName("BotSpawnTimeOnMin")]
    public int? BotSpawnTimeOnMin { get; set; }

    /** How soon bots will be allowed to spawn */
    [JsonPropertyName("BotStart")]
    public int? BotStart { get; set; }

    /** After this long bots will no longer spawn */
    [JsonPropertyName("BotStop")]
    public int? BotStop { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("DisabledForScav")]
    public bool? DisabledForScav { get; set; }

    [JsonPropertyName("DisabledScavExits")]
    public string? DisabledScavExits { get; set; }

    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("EnableCoop")]
    public bool? EnableCoop { get; set; }

    [JsonPropertyName("GlobalLootChanceModifier")]
    public double? GlobalLootChanceModifier { get; set; }

    [JsonPropertyName("GlobalLootChanceModifierPvE")]
    public double? GlobalLootChanceModifierPvE { get; set; }

    [JsonPropertyName("GlobalContainerChanceModifier")]
    public double? GlobalContainerChanceModifier { get; set; }

    [JsonPropertyName("IconX")]
    public double? IconX { get; set; }

    [JsonPropertyName("IconY")]
    public double? IconY { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Insurance")]
    public bool? Insurance { get; set; }

    [JsonPropertyName("IsSecret")]
    public bool? IsSecret { get; set; }

    [JsonPropertyName("Locked")]
    public bool? Locked { get; set; }

    [JsonPropertyName("Loot")]
    public List<SpawnpointTemplate>? Loot { get; set; }

    [JsonPropertyName("MatchMakerMinPlayersByWaitTime")]
    public List<MinPlayerWaitTime>? MatchMakerMinPlayersByWaitTime { get; set; }

    [JsonPropertyName("MaxBotPerZone")]
    public int? MaxBotPerZone { get; set; }

    [JsonPropertyName("MaxDistToFreePoint")]
    public int? MaxDistToFreePoint { get; set; }

    [JsonPropertyName("MaxPlayers")]
    public int? MaxPlayers { get; set; }

    [JsonPropertyName("MinDistToExitPoint")]
    public double? MinDistToExitPoint { get; set; }

    [JsonPropertyName("MinDistToFreePoint")]
    public double? MinDistToFreePoint { get; set; }

    [JsonPropertyName("MinMaxBots")]
    public List<MinMaxBot>? MinMaxBots { get; set; }

    [JsonPropertyName("MinPlayers")]
    public int? MinPlayers { get; set; }

    [JsonPropertyName("MaxCoopGroup")]
    public int? MaxCoopGroup { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("NonWaveGroupScenario")]
    public NonWaveGroupScenario? NonWaveGroupScenario { get; set; }

    [JsonPropertyName("NewSpawn")]
    public bool? NewSpawn { get; set; }

    [JsonPropertyName("OcculsionCullingEnabled")]
    public bool? OcculsionCullingEnabled { get; set; }

    [JsonPropertyName("OldSpawn")]
    public bool? OldSpawn { get; set; }

    [JsonPropertyName("OpenZones")]
    public string? OpenZones { get; set; }

    [JsonPropertyName("Preview")]
    public Preview? Preview { get; set; }

    [JsonPropertyName("PlayersRequestCount")]
    public int? PlayersRequestCount { get; set; }

    [JsonPropertyName("RequiredPlayerLevel")]
    public int? RequiredPlayerLevel { get; set; }

    [JsonPropertyName("RequiredPlayerLevelMin")]
    public int? RequiredPlayerLevelMin { get; set; }

    [JsonPropertyName("RequiredPlayerLevelMax")]
    public int? RequiredPlayerLevelMax { get; set; }

    [JsonPropertyName("MinPlayerLvlAccessKeys")]
    public int? MinPlayerLvlAccessKeys { get; set; }

    [JsonPropertyName("PmcMaxPlayersInGroup")]
    public int? PmcMaxPlayersInGroup { get; set; }

    [JsonPropertyName("ScavMaxPlayersInGroup")]
    public int? ScavMaxPlayersInGroup { get; set; }

    [JsonPropertyName("Rules")]
    public string? Rules { get; set; }

    [JsonPropertyName("SafeLocation")]
    public bool? SafeLocation { get; set; }

    [JsonPropertyName("Scene")]
    public Scene? Scene { get; set; }

    [JsonPropertyName("SpawnPointParams")]
    public List<SpawnPointParam>? SpawnPointParams { get; set; }

    [JsonPropertyName("UnixDateTime")]
    public long? UnixDateTime { get; set; }

    [JsonPropertyName("_Id")]
    public string? IdField { get; set; }

    [JsonPropertyName("doors")]
    public List<object>? Doors { get; set; }

    [JsonPropertyName("EscapeTimeLimit")]
    public int? EscapeTimeLimit { get; set; }

    // BSG fucked up another property name
    [JsonPropertyName("escape_time_limit")]
    public int Escape_Time_Limit_Do_Not_Use
    {
        set => EscapeTimeLimit = value;
    }

    [JsonPropertyName("EscapeTimeLimitCoop")]
    public int? EscapeTimeLimitCoop { get; set; }

    [JsonPropertyName("EscapeTimeLimitPVE")]
    public int? EscapeTimeLimitPVE { get; set; }

    [JsonPropertyName("Events")]
    public LocationEvents? Events { get; set; }

    [JsonPropertyName("exit_access_time")]
    public int? ExitAccessTime { get; set; }

    [JsonPropertyName("ForceOnlineRaidInPVE")]
    public bool? ForceOnlineRaidInPVE { get; set; }

    [JsonPropertyName("exit_count")]
    public int? ExitCount { get; set; }

    [JsonPropertyName("exit_time")]
    public int? ExitTime { get; set; }

    [JsonPropertyName("exits")]
    public List<Exit>? Exits { get; set; }

    [JsonPropertyName("filter_ex")]
    public List<string>? FilterEx { get; set; }

    [JsonPropertyName("limits")]
    public List<Limit>? Limits { get; set; }

    [JsonPropertyName("matching_min_seconds")]
    public int? MatchingMinSeconds { get; set; }

    [JsonPropertyName("GenerateLocalLootCache")]
    public bool? GenerateLocalLootCache { get; set; }

    [JsonPropertyName("maxItemCountInLocation")]
    public List<MaxItemCountInLocation>? MaxItemCountInLocation { get; set; }

    [JsonPropertyName("sav_summon_seconds")]
    public int? SavSummonSeconds { get; set; }

    [JsonPropertyName("tmp_location_field_remove_me")]
    public int? TmpLocationFieldRemoveMe { get; set; }

    [JsonPropertyName("transits")]
    public List<Transit>? Transits { get; set; }

    [JsonPropertyName("users_gather_seconds")]
    public int? UsersGatherSeconds { get; set; }

    [JsonPropertyName("users_spawn_seconds_n")]
    public int? UsersSpawnSecondsN { get; set; }

    [JsonPropertyName("users_spawn_seconds_n2")]
    public int? UsersSpawnSecondsN2 { get; set; }

    [JsonPropertyName("users_summon_seconds")]
    public int? UsersSummonSeconds { get; set; }

    [JsonPropertyName("waves")]
    public List<Wave>? Waves { get; set; }
}

public class Transit
{
    [JsonPropertyName("activateAfterSec")]
    public int ActivateAfterSeconds { get; set; } // TODO: Int in client

    [JsonPropertyName("active")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("events")]
    public bool? Events { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("conditions")]
    public string? Conditions { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("time")]
    public int? Time { get; set; }
}

public class NonWaveGroupScenario
{
    [JsonPropertyName("Chance")]
    public double? Chance { get; set; }

    [JsonPropertyName("Enabled")]
    public bool? IsEnabled { get; set; }

    [JsonPropertyName("MaxToBeGroup")]
    public int? MaximumToBeGrouped { get; set; }

    [JsonPropertyName("MinToBeGroup")]
    public int? MinimumToBeGrouped { get; set; }
}

public class Limit : MinMax
{
    [JsonPropertyName("items")]
    public object[] Items { get; set; } // TODO: was on TS any[] hmmm..

    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }
}

public class AirdropParameter
{
    [JsonPropertyName("AirdropPointDeactivateDistance")]
    public int? AirdropPointDeactivateDistance { get; set; }

    [JsonPropertyName("MinPlayersCountToSpawnAirdrop")]
    public int? MinimumPlayersCountToSpawnAirdrop { get; set; }

    [JsonPropertyName("PlaneAirdropChance")]
    public double? PlaneAirdropChance { get; set; }

    [JsonPropertyName("PlaneAirdropCooldownMax")]
    public int? PlaneAirdropCooldownMax { get; set; }

    [JsonPropertyName("PlaneAirdropCooldownMin")]
    public int? PlaneAirdropCooldownMin { get; set; }

    [JsonPropertyName("PlaneAirdropEnd")]
    public int? PlaneAirdropEnd { get; set; }

    [JsonPropertyName("PlaneAirdropMax")]
    public int? PlaneAirdropMax { get; set; }

    [JsonPropertyName("PlaneAirdropStartMax")]
    public int? PlaneAirdropStartMax { get; set; }

    [JsonPropertyName("PlaneAirdropStartMin")]
    public int? PlaneAirdropStartMin { get; set; }

    [JsonPropertyName("UnsuccessfulTryPenalty")]
    public int? UnsuccessfulTryPenalty { get; set; }
}

public class Banner
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("pic")]
    public Pic? Picture { get; set; }
}

public class Pic
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("rcid")]
    public string? Rcid { get; set; }
}

public class BossLocationSpawn
{
    [JsonPropertyName("BossChance")]
    public double? BossChance { get; set; }

    [JsonPropertyName("BossDifficult")]
    public string? BossDifficulty { get; set; }

    [JsonPropertyName("BossEscortAmount")]
    public string? BossEscortAmount { get; set; }

    [JsonPropertyName("BossEscortDifficult")]
    public string? BossEscortDifficulty { get; set; }

    [JsonPropertyName("BossEscortType")]
    public string? BossEscortType { get; set; }

    [JsonPropertyName("BossName")]
    public string? BossName { get; set; }

    [JsonPropertyName("BossPlayer")]
    public bool? IsBossPlayer { get; set; }

    [JsonPropertyName("BossZone")]
    public string? BossZone { get; set; }

    [JsonPropertyName("RandomTimeSpawn")]
    public bool? IsRandomTimeSpawn { get; set; }

    [JsonPropertyName("Time")]
    public double? Time { get; set; }

    [JsonPropertyName("TriggerId")]
    public string? TriggerId { get; set; }

    [JsonPropertyName("TriggerName")]
    public string? TriggerName { get; set; }

    [JsonPropertyName("Delay")]
    public double? Delay { get; set; }

    [JsonPropertyName("DependKarma")]
    public bool? DependKarma { get; set; }

    [JsonPropertyName("DependKarmaPVE")]
    public bool? DependKarmaPVE { get; set; }

    [JsonPropertyName("ForceSpawn")]
    public bool? ForceSpawn { get; set; }

    [JsonPropertyName("IgnoreMaxBots")]
    public bool? IgnoreMaxBots { get; set; }

    [JsonPropertyName("Supports")]
    public BossSupport[] Supports { get; set; }

    [JsonPropertyName("sptId")]
    public string? SptId { get; set; }

    [JsonPropertyName("SpawnMode")]
    public string[] SpawnMode { get; set; }
}

public class BossSupport
{
    [JsonPropertyName("BossEscortAmount")]
    public string? BossEscortAmount { get; set; }

    [JsonPropertyName("BossEscortDifficult")]
    public string[] BossEscortDifficulty { get; set; }

    [JsonPropertyName("BossEscortType")]
    public string? BossEscortType { get; set; }
}

public class BotLocationModifier
{
    [JsonPropertyName("AccuracySpeed")]
    public double? AccuracySpeed { get; set; }

    [JsonPropertyName("AdditionalHostilitySettings")]
    public AdditionalHostilitySettings[] AdditionalHostilitySettings { get; set; }

    [JsonPropertyName("DistToActivate")]
    public double? DistanceToActivate { get; set; }

    [JsonPropertyName("DistToActivatePvE")]
    public double? DistanceToActivatePvE { get; set; }

    [JsonPropertyName("DistToPersueAxemanCoef")]
    public double? DistanceToPursueAxemanCoefficient { get; set; }

    [JsonPropertyName("DistToSleep")]
    public double? DistanceToSleep { get; set; }

    [JsonPropertyName("DistToSleepPvE")]
    public double? DistanceToSleepPvE { get; set; }

    [JsonPropertyName("GainSight")]
    public double? GainSight { get; set; }

    [JsonPropertyName("KhorovodChance")]
    public double? KhorovodChance { get; set; }

    [JsonPropertyName("MagnetPower")]
    public double? MagnetPower { get; set; }

    [JsonPropertyName("MarksmanAccuratyCoef")]
    public double? MarksmanAccuracyCoefficient { get; set; }

    [JsonPropertyName("Scattering")]
    public double? Scattering { get; set; }

    [JsonPropertyName("VisibleDistance")]
    public double? VisibleDistance { get; set; }

    [JsonPropertyName("MaxExfiltrationTime")]
    public double? MaxExfiltrationTime { get; set; }

    [JsonPropertyName("MinExfiltrationTime")]
    public double? MinExfiltrationTime { get; set; }
}

public class AdditionalHostilitySettings
{
    [JsonPropertyName("AlwaysEnemies")]
    public List<string>? AlwaysEnemies { get; set; }

    [JsonPropertyName("AlwaysFriends")]
    public List<string>? AlwaysFriends { get; set; }

    [JsonPropertyName("BearEnemyChance")]
    public int? BearEnemyChance { get; set; }

    [JsonPropertyName("BearPlayerBehaviour")]
    public string? BearPlayerBehaviour { get; set; }

    [JsonPropertyName("BotRole")]
    public string? BotRole { get; set; }

    [JsonPropertyName("ChancedEnemies")]
    public List<ChancedEnemy>? ChancedEnemies { get; set; }

    [JsonPropertyName("Neutral")]
    public List<string>? Neutral { get; set; }

    [JsonPropertyName("SavagePlayerBehaviour")]
    public string? SavagePlayerBehaviour { get; set; }

    [JsonPropertyName("SavageEnemyChance")]
    public int? SavageEnemyChance { get; set; }

    [JsonPropertyName("UsecEnemyChance")]
    public int? UsecEnemyChance { get; set; }

    [JsonPropertyName("UsecPlayerBehaviour")]
    public string? UsecPlayerBehaviour { get; set; }

    [JsonPropertyName("Warn")]
    public List<string>? Warn { get; set; }
}

public class ChancedEnemy
{
    [JsonPropertyName("EnemyChance")]
    public int? EnemyChance { get; set; }

    [JsonPropertyName("Role")]
    public string? Role { get; set; }
}

public class MinMaxBot : MinMax
{
    [JsonPropertyName("WildSpawnType")]
    public object WildSpawnType { get; set; } // TODO: Could be WildSpawnType or string
}

public class MinPlayerWaitTime
{
    [JsonPropertyName("minPlayers")]
    public int? MinPlayers { get; set; }

    [JsonPropertyName("time")]
    public int? Time { get; set; }
}

public class Preview
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("rcid")]
    public string? Rcid { get; set; }
}

public class Scene
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("rcid")]
    public string? Rcid { get; set; }
}

public class SpawnPointParam
{
    [JsonPropertyName("BotZoneName")]
    public string? BotZoneName { get; set; }

    [JsonPropertyName("Categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("ColliderParams")]
    public ColliderParams? ColliderParams { get; set; }

    [JsonPropertyName("CorePointId")]
    public int? CorePointId { get; set; }

    [JsonPropertyName("DelayToCanSpawnSec")]
    public double? DelayToCanSpawnSec { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Infiltration")]
    public string? Infiltration { get; set; }

    [JsonPropertyName("Position")]
    public XYZ? Position { get; set; }

    [JsonPropertyName("Rotation")]
    public float? Rotation { get; set; }

    [JsonPropertyName("Sides")]
    public List<string>? Sides { get; set; }
}

public class ColliderParams
{
    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_props")]
    public Props? Props { get; set; }
}

public class Props
{
    [JsonPropertyName("Center")]
    public XYZ? Center { get; set; }

    [JsonPropertyName("Size")]
    public XYZ? Size { get; set; }

    [JsonPropertyName("Radius")]
    public float? Radius { get; set; }
}

public class Exit
{
    /** % Chance out of 100 exit will appear in raid */
    [JsonPropertyName("Chance")]
    public double? Chance { get; set; }

    [JsonPropertyName("ChancePVE")]
    public double? ChancePVE { get; set; }

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("CountPve")]
    public int? CountPve { get; set; }

    // Had to add this property as BSG sometimes names the properties with full PVE capitals
    // This property will just point the value to CountPve
    [JsonPropertyName("CountPVE")]
    public int CountPVE
    {
        set => CountPve = value;
    }

    [JsonPropertyName("EntryPoints")]
    public string? EntryPoints { get; set; }

    [JsonPropertyName("EventAvailable")]
    public bool? EventAvailable { get; set; }

    [JsonPropertyName("EligibleForPMC")]
    public bool? EligibleForPMC { get; set; }

    [JsonPropertyName("EligibleForScav")]
    public bool? EligibleForScav { get; set; }

    [JsonPropertyName("ExfiltrationTime")]
    public double? ExfiltrationTime { get; set; }

    [JsonPropertyName("ExfiltrationTimePVE")]
    public float? ExfiltrationTimePVE { get; set; }

    [JsonPropertyName("ExfiltrationType")]
    public string? ExfiltrationType { get; set; }

    [JsonPropertyName("RequiredSlot")]
    public string? RequiredSlot { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("MaxTime")]
    public double? MaxTime { get; set; }

    [JsonPropertyName("MaxTimePVE")]
    public double? MaxTimePVE { get; set; }

    [JsonPropertyName("MinTime")]
    public double? MinTime { get; set; }

    [JsonPropertyName("MinTimePVE")]
    public double? MinTimePVE { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("PassageRequirement")]
    public string? PassageRequirement { get; set; }

    [JsonPropertyName("PlayersCount")]
    public int? PlayersCount { get; set; }

    [JsonPropertyName("PlayersCountPVE")]
    public int? PlayersCountPVE { get; set; }

    [JsonPropertyName("RequirementTip")]
    public string? RequirementTip { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }
}

public class MaxItemCountInLocation
{
    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("Value")]
    public int? Value { get; set; }
}

public class Wave
{
    [JsonPropertyName("BotPreset")]
    public string? BotPreset { get; set; }

    [JsonPropertyName("BotSide")]
    public string? BotSide { get; set; }

    [JsonPropertyName("SpawnPoints")]
    public string? SpawnPoints { get; set; }

    [JsonPropertyName("WildSpawnType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WildSpawnType? WildSpawnType { get; set; }

    [JsonPropertyName("isPlayers")]
    public bool? IsPlayers { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("slots_max")]
    public int? SlotsMax { get; set; }

    [JsonPropertyName("slots_min")]
    public int? SlotsMin { get; set; }

    [JsonPropertyName("time_max")]
    public int? TimeMax { get; set; }

    [JsonPropertyName("time_min")]
    public int? TimeMin { get; set; }

    /** OPTIONAL - Needs to be unique - Used by custom wave service to ensure same wave isnt added multiple times */
    [JsonPropertyName("sptId")]
    public string? SptId { get; set; }

    [JsonPropertyName("ChanceGroup")]
    public int? ChanceGroup { get; set; }

    /** 'pve' and/or 'regular' */
    [JsonPropertyName("SpawnMode")]
    public List<string>? SpawnMode { get; set; }
}

public class LocationEvents
{
    [JsonPropertyName("Halloween2024")]
    public Halloween2024? Halloween2024 { get; set; }

    public Khorovod? Khorovod { get; set; }
}

public class Khorovod
{
    public double? Chance { get; set; }
}

public class Halloween2024
{
    [JsonPropertyName("CrowdAttackBlockRadius")]
    public int? CrowdAttackBlockRadius { get; set; }

    [JsonPropertyName("CrowdAttackSpawnParams")]
    public List<CrowdAttackSpawnParam>? CrowdAttackSpawnParams { get; set; }

    [JsonPropertyName("CrowdCooldownPerPlayerSec")]
    public int? CrowdCooldownPerPlayerSec { get; set; }

    [JsonPropertyName("CrowdsLimit")]
    public int? CrowdsLimit { get; set; }

    [JsonPropertyName("InfectedLookCoeff")]
    public double? InfectedLookCoeff { get; set; }

    [JsonPropertyName("MaxCrowdAttackSpawnLimit")]
    public int? MaxCrowdAttackSpawnLimit { get; set; }

    [JsonPropertyName("MinInfectionPercentage")]
    public double? MinInfectionPercentage { get; set; }

    [JsonPropertyName("MinSpawnDistToPlayer")]
    public double? MinSpawnDistToPlayer { get; set; }

    [JsonPropertyName("TargetPointSearchRadiusLimit")]
    public double? TargetPointSearchRadiusLimit { get; set; }

    [JsonPropertyName("ZombieCallDeltaRadius")]
    public double? ZombieCallDeltaRadius { get; set; }

    [JsonPropertyName("ZombieCallPeriodSec")]
    public int? ZombieCallPeriodSec { get; set; }

    [JsonPropertyName("ZombieCallRadiusLimit")]
    public double? ZombieCallRadiusLimit { get; set; }

    [JsonPropertyName("ZombieMultiplier")]
    public double? ZombieMultiplier { get; set; }

    [JsonPropertyName("InfectionPercentage")]
    public double? InfectionPercentage { get; set; }

    public Khorovod? Khorovod { get; set; }
}

public class CrowdAttackSpawnParam
{
    [JsonPropertyName("Difficulty")]
    public string? Difficulty { get; set; }

    [JsonPropertyName("Role")]
    public string? Role { get; set; }

    [JsonPropertyName("Weight")]
    public int? Weight { get; set; }
}

public enum WildSpawnType
{
    assault,
    marksman,
    pmcbot,
    bosskilla,
    bossknight
}