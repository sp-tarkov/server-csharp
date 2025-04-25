using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LocationBase
{
    [JsonPropertyName("AccessKeys")]
    public List<string>? AccessKeys
    {
        get;
        set;
    }

    [JsonPropertyName("AccessKeysPvE")]
    public List<string>? AccessKeysPvE
    {
        get;
        set;
    }

    [JsonPropertyName("AirdropParameters")]
    public List<AirdropParameter>? AirdropParameters
    {
        get;
        set;
    }

    [JsonPropertyName("NewSpawnForPlayers")]
    public bool? NewSpawnForPlayers
    {
        get;
        set;
    }

    [JsonPropertyName("OfflineNewSpawn")]
    public bool? OfflineNewSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("OfflineOldSpawn")]
    public bool? OfflineOldSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("Area")]
    public double? Area
    {
        get;
        set;
    }

    [JsonPropertyName("AveragePlayTime")]
    public double? AveragePlayTime
    {
        get;
        set;
    }

    [JsonPropertyName("AveragePlayerLevel")]
    public double? AveragePlayerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("Banners")]
    public List<Banner>? Banners
    {
        get;
        set;
    }

    [JsonPropertyName("BossLocationSpawn")]
    public List<BossLocationSpawn> BossLocationSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("secretExits")]
    public List<Exit>? SecretExits
    {
        get;
        set;
    }

    [JsonPropertyName("BotStartPlayer")]
    public int? BotStartPlayer
    {
        get;
        set;
    }

    [JsonPropertyName("BotAssault")]
    public int? BotAssault
    {
        get;
        set;
    }

    /// <summary>
    ///     Weighting on how likely a bot will be Easy difficulty
    /// </summary>
    [JsonPropertyName("BotEasy")]
    public int? BotEasy
    {
        get;
        set;
    }

    /// <summary>
    ///     Weighting on how likely a bot will be Hard difficulty
    /// </summary>
    [JsonPropertyName("BotHard")]
    public int? BotHard
    {
        get;
        set;
    }

    /// <summary>
    ///     Weighting on how likely a bot will be Impossible difficulty
    /// </summary>
    [JsonPropertyName("BotImpossible")]
    public int? BotImpossible
    {
        get;
        set;
    }

    [JsonPropertyName("BotLocationModifier")]
    public BotLocationModifier? BotLocationModifier
    {
        get;
        set;
    }

    [JsonPropertyName("BotMarksman")]
    public int? BotMarksman
    {
        get;
        set;
    }

    /// <summary>
    ///     Maximum Number of bots that are currently alive/loading/delayed
    /// </summary>
    [JsonPropertyName("BotMax")]
    public int? BotMax
    {
        get;
        set;
    }

    /// <summary>
    ///     Is not used in 33420 TODO: still needed?
    /// </summary>
    [JsonPropertyName("BotMaxPlayer")]
    public int? BotMaxPlayer
    {
        get;
        set;
    }

    /// <summary>
    ///     Is not used in 33420 TODO: still needed?
    /// </summary>
    [JsonPropertyName("BotMaxTimePlayer")]
    public int? BotMaxTimePlayer
    {
        get;
        set;
    }

    /// <summary>
    ///     Does not even exist in the client in 33420 TODO: still needed?
    /// </summary>
    [JsonPropertyName("BotMaxPvE")]
    public int? BotMaxPvE
    {
        get;
        set;
    }

    /// <summary>
    ///     Weighting on how likely a bot will be Normal difficulty
    /// </summary>
    [JsonPropertyName("BotNormal")]
    public int? BotNormal
    {
        get;
        set;
    }

    /// <summary>
    ///     How many bot slots that need to be open before trying to spawn new bots.
    /// </summary>
    [JsonPropertyName("BotSpawnCountStep")]
    public int? BotSpawnCountStep
    {
        get;
        set;
    }

    /// <summary>
    ///     How often to check if bots are spawn-able. In seconds
    /// </summary>
    [JsonPropertyName("BotSpawnPeriodCheck")]
    public int? BotSpawnPeriodCheck
    {
        get;
        set;
    }

    /// <summary>
    ///     The bot spawn will toggle on and off in intervals of Off(Min/Max) and On(Min/Max)
    /// </summary>
    [JsonPropertyName("BotSpawnTimeOffMax")]
    public int? BotSpawnTimeOffMax
    {
        get;
        set;
    }

    [JsonPropertyName("BotSpawnTimeOffMin")]
    public int? BotSpawnTimeOffMin
    {
        get;
        set;
    }

    [JsonPropertyName("BotSpawnTimeOnMax")]
    public int? BotSpawnTimeOnMax
    {
        get;
        set;
    }

    [JsonPropertyName("BotSpawnTimeOnMin")]
    public int? BotSpawnTimeOnMin
    {
        get;
        set;
    }

    /// <summary>
    ///     How soon bots will be allowed to spawn
    /// </summary>
    [JsonPropertyName("BotStart")]
    public int? BotStart
    {
        get;
        set;
    }

    /// <summary>
    ///     After this long bots will no longer spawn
    /// </summary>
    [JsonPropertyName("BotStop")]
    public int? BotStop
    {
        get;
        set;
    }

    [JsonPropertyName("Description")]
    public string? Description
    {
        get;
        set;
    }

    [JsonPropertyName("DisabledForScav")]
    public bool? DisabledForScav
    {
        get;
        set;
    }

    [JsonPropertyName("EventTrapsData")]
    public EventTrapsData? EventTrapsData
    {
        get;
        set;
    }

    [JsonPropertyName("DisabledScavExits")]
    public string? DisabledScavExits
    {
        get;
        set;
    }

    [JsonPropertyName("Enabled")]
    public bool? Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("EnableCoop")]
    public bool? EnableCoop
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalLootChanceModifier")]
    public double? GlobalLootChanceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalLootChanceModifierPvE")]
    public double? GlobalLootChanceModifierPvE
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalContainerChanceModifier")]
    public double? GlobalContainerChanceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("HeatmapCellSize")]
    public XYZ? HeatmapCellSize
    {
        get;
        set;
    }

    [JsonPropertyName("HeatmapLayers")]
    public List<string>? HeatmapLayers
    {
        get;
        set;
    }

    [JsonPropertyName("IconX")]
    public double? IconX
    {
        get;
        set;
    }

    [JsonPropertyName("IconY")]
    public double? IconY
    {
        get;
        set;
    }

    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Insurance")]
    public bool? Insurance
    {
        get;
        set;
    }

    [JsonPropertyName("IsSecret")]
    public bool? IsSecret
    {
        get;
        set;
    }

    [JsonPropertyName("Locked")]
    public bool? Locked
    {
        get;
        set;
    }

    [JsonPropertyName("Loot")]
    public List<SpawnpointTemplate>? Loot
    {
        get;
        set;
    }

    [JsonPropertyName("MatchMakerMinPlayersByWaitTime")]
    public List<MinPlayerWaitTime>? MatchMakerMinPlayersByWaitTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxBotPerZone")]
    public int? MaxBotPerZone
    {
        get;
        set;
    }

    [JsonPropertyName("MaxDistToFreePoint")]
    public int? MaxDistToFreePoint
    {
        get;
        set;
    }

    [JsonPropertyName("MaxPlayers")]
    public int? MaxPlayers
    {
        get;
        set;
    }

    [JsonPropertyName("MinDistToExitPoint")]
    public double? MinDistToExitPoint
    {
        get;
        set;
    }

    [JsonPropertyName("MinDistToFreePoint")]
    public double? MinDistToFreePoint
    {
        get;
        set;
    }

    [JsonPropertyName("MinMaxBots")]
    public List<MinMaxBot>? MinMaxBots
    {
        get;
        set;
    }

    [JsonPropertyName("MinPlayers")]
    public int? MinPlayers
    {
        get;
        set;
    }

    [JsonPropertyName("MaxCoopGroup")]
    public int? MaxCoopGroup
    {
        get;
        set;
    }

    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("NonWaveGroupScenario")]
    public NonWaveGroupScenario? NonWaveGroupScenario
    {
        get;
        set;
    }

    [JsonPropertyName("NewSpawn")]
    public bool? NewSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("OcculsionCullingEnabled")]
    public bool? OcculsionCullingEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("OldSpawn")]
    public bool? OldSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("OpenZones")]
    public string? OpenZones
    {
        get;
        set;
    }

    [JsonPropertyName("Preview")]
    public Preview? Preview
    {
        get;
        set;
    }

    [JsonPropertyName("PlayersRequestCount")]
    public int? PlayersRequestCount
    {
        get;
        set;
    }

    [JsonPropertyName("RequiredPlayerLevel")]
    public int? RequiredPlayerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("RequiredPlayerLevelMin")]
    public int? RequiredPlayerLevelMin
    {
        get;
        set;
    }

    [JsonPropertyName("RequiredPlayerLevelMax")]
    public int? RequiredPlayerLevelMax
    {
        get;
        set;
    }

    [JsonPropertyName("MinPlayerLvlAccessKeys")]
    public int? MinPlayerLvlAccessKeys
    {
        get;
        set;
    }

    [JsonPropertyName("PmcMaxPlayersInGroup")]
    public int? PmcMaxPlayersInGroup
    {
        get;
        set;
    }

    [JsonPropertyName("ScavMaxPlayersInGroup")]
    public int? ScavMaxPlayersInGroup
    {
        get;
        set;
    }

    [JsonPropertyName("Rules")]
    public string? Rules
    {
        get;
        set;
    }

    [JsonPropertyName("SafeLocation")]
    public bool? SafeLocation
    {
        get;
        set;
    }

    [JsonPropertyName("Scene")]
    public Scene? Scene
    {
        get;
        set;
    }

    [JsonPropertyName("NoGroupSpawn")]
    public bool? NoGroupSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("SpawnPointParams")]
    public List<SpawnPointParam>? SpawnPointParams
    {
        get;
        set;
    }

    [JsonPropertyName("areas")]
    public Dictionary<string, Area>? Areas
    {
        get;
        set;
    }

    [JsonPropertyName("UnixDateTime")]
    public long? UnixDateTime
    {
        get;
        set;
    }

    [JsonPropertyName("_Id")]
    public string? IdField
    {
        get;
        set;
    }

    [JsonPropertyName("doors")]
    public List<object>? Doors
    {
        get;
        set;
    }

    [JsonPropertyName("EscapeTimeLimit")]
    public double? EscapeTimeLimit
    {
        get;
        set;
    }

    // BSG fucked up another property name
    [JsonPropertyName("escape_time_limit")]
    public int Escape_Time_Limit_Do_Not_Use
    {
        set
        {
            EscapeTimeLimit = value;
        }
    }

    [JsonPropertyName("EscapeTimeLimitCoop")]
    public int? EscapeTimeLimitCoop
    {
        get;
        set;
    }

    [JsonPropertyName("EscapeTimeLimitPVE")]
    public int? EscapeTimeLimitPVE
    {
        get;
        set;
    }

    [JsonPropertyName("Events")]
    public LocationEvents? Events
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("exit_access_time")]
    public int? ExitAccessTime
    {
        get;
        set;
    }

    [JsonPropertyName("ForceOnlineRaidInPVE")]
    public bool? ForceOnlineRaidInPVE
    {
        get;
        set;
    }

    [JsonPropertyName("ExitZones")]
    public string? ExitZones
    {
        get;
        set;
    }

    [JsonPropertyName("exit_count")]
    public int? ExitCount
    {
        get;
        set;
    }

    [JsonPropertyName("exit_time")]
    public double? ExitTime
    {
        get;
        set;
    }

    [JsonPropertyName("SpawnSafeDistanceMeters")]
    public double? SpawnSafeDistanceMeters
    {
        get;
        set;
    }

    [JsonPropertyName("OneTimeSpawn")]
    public double? OneTimeSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("exits")]
    public List<Exit>? Exits
    {
        get;
        set;
    }

    [JsonPropertyName("filter_ex")]
    public List<string>? FilterEx
    {
        get;
        set;
    }

    [JsonPropertyName("limits")]
    public List<Limit>? Limits
    {
        get;
        set;
    }

    [JsonPropertyName("matching_min_seconds")]
    public int? MatchingMinSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("GenerateLocalLootCache")]
    public bool? GenerateLocalLootCache
    {
        get;
        set;
    }

    [JsonPropertyName("maxItemCountInLocation")]
    public List<MaxItemCountInLocation>? MaxItemCountInLocation
    {
        get;
        set;
    }

    [JsonPropertyName("sav_summon_seconds")]
    public int? SavSummonSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("tmp_location_field_remove_me")]
    public int? TmpLocationFieldRemoveMe
    {
        get;
        set;
    }

    [JsonPropertyName("transits")]
    public List<Transit>? Transits
    {
        get;
        set;
    }

    [JsonPropertyName("users_gather_seconds")]
    public int? UsersGatherSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("users_spawn_seconds_n")]
    public int? UsersSpawnSecondsN
    {
        get;
        set;
    }

    [JsonPropertyName("users_spawn_seconds_n2")]
    public int? UsersSpawnSecondsN2
    {
        get;
        set;
    }

    [JsonPropertyName("users_summon_seconds")]
    public int? UsersSummonSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("waves")]
    public List<Wave> Waves
    {
        get;
        set;
    }
}
