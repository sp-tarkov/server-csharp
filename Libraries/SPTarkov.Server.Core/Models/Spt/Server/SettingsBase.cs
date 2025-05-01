using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record SettingsBase
{
    [JsonPropertyName("config")]
    public Config? Configuration
    {
        get;
        set;
    }
}

public record Config
{
    [JsonPropertyName("AFKTimeoutSeconds")]
    public int? AFKTimeoutSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("AdditionalRandomDelaySeconds")]
    public int? AdditionalRandomDelaySeconds
    {
        get;
        set;
    }

    [JsonPropertyName("AudioSettings")]
    public AudioSettings? AudioSettings
    {
        get;
        set;
    }

    [JsonPropertyName("ClientSendRateLimit")]
    public int? ClientSendRateLimit
    {
        get;
        set;
    }

    [JsonPropertyName("CriticalRetriesCount")]
    public int? CriticalRetriesCount
    {
        get;
        set;
    }

    [JsonPropertyName("DefaultRetriesCount")]
    public int? DefaultRetriesCount
    {
        get;
        set;
    }

    [JsonPropertyName("FirstCycleDelaySeconds")]
    public int? FirstCycleDelaySeconds
    {
        get;
        set;
    }

    [JsonPropertyName("FramerateLimit")]
    public FramerateLimit? FramerateLimit
    {
        get;
        set;
    }

    [JsonPropertyName("GroupStatusInterval")]
    public int? GroupStatusInterval
    {
        get;
        set;
    }

    [JsonPropertyName("GroupStatusButtonInterval")]
    public int? GroupStatusButtonInterval
    {
        get;
        set;
    }

    [JsonPropertyName("KeepAliveInterval")]
    public int? KeepAliveInterval
    {
        get;
        set;
    }

    [JsonPropertyName("LobbyKeepAliveInterval")]
    public int? LobbyKeepAliveInterval
    {
        get;
        set;
    }

    [JsonPropertyName("Mark502and504AsNonImportant")]
    public bool? Mark502and504AsNonImportant
    {
        get;
        set;
    }

    [JsonPropertyName("MemoryManagementSettings")]
    public MemoryManagementSettings? MemoryManagementSettings
    {
        get;
        set;
    }

    [JsonPropertyName("NVidiaHighlights")]
    public bool? NVidiaHighlights
    {
        get;
        set;
    }

    [JsonPropertyName("NextCycleDelaySeconds")]
    public int? NextCycleDelaySeconds
    {
        get;
        set;
    }

    [JsonPropertyName("PingServerResultSendInterval")]
    public int? PingServerResultSendInterval
    {
        get;
        set;
    }

    [JsonPropertyName("PingServersInterval")]
    public int? PingServersInterval
    {
        get;
        set;
    }

    [JsonPropertyName("ReleaseProfiler")]
    public ReleaseProfiler? ReleaseProfiler
    {
        get;
        set;
    }

    [JsonPropertyName("RequestConfirmationTimeouts")]
    public List<double>? RequestConfirmationTimeouts
    {
        get;
        set;
    }

    [JsonPropertyName("RequestsMadeThroughLobby")]
    public List<string>? RequestsMadeThroughLobby
    {
        get;
        set;
    }

    [JsonPropertyName("SecondCycleDelaySeconds")]
    public int? SecondCycleDelaySeconds
    {
        get;
        set;
    }

    [JsonPropertyName("ShouldEstablishLobbyConnection")]
    public bool? ShouldEstablishLobbyConnection
    {
        get;
        set;
    }

    [JsonPropertyName("TurnOffLogging")]
    public bool? TurnOffLogging
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponOverlapDistanceCulling")]
    public int? WeaponOverlapDistanceCulling
    {
        get;
        set;
    }

    [JsonPropertyName("WebDiagnosticsEnabled")]
    public bool? WebDiagnosticsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("NetworkStateView")]
    public NetworkStateView? NetworkStateView
    {
        get;
        set;
    }

    [JsonPropertyName("WsReconnectionDelays")]
    public List<int>? WsReconnectionDelays
    {
        get;
        set;
    }
}

public record AudioSettings
{
    [JsonPropertyName("AudioGroupPresets")]
    public List<AudioGroupPreset>? AudioGroupPresets
    {
        get;
        set;
    }

    [JsonPropertyName("EnvironmentSettings")]
    public EnvironmentSettings? EnvironmentSettings
    {
        get;
        set;
    }

    [JsonPropertyName("HeadphonesSettings")]
    public HeadphoneSettings HeadphonesSettings
    {
        get;
        set;
    }

    [JsonPropertyName("MetaXRAudioPluginSettings")]
    public MetaXRAudioPluginSettings? MetaXRAudioPluginSettings
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerSettings")]
    public PlayerSettings? PlayerSettings
    {
        get;
        set;
    }

    [JsonPropertyName("RadioBroadcastSettings")]
    public RadioBroadcastSettings? RadioBroadcastSettings
    {
        get;
        set;
    }
}

public record FramerateLimit
{
    [JsonPropertyName("MaxFramerateGameLimit")]
    public int? MaxFramerateGameLimit
    {
        get;
        set;
    }

    [JsonPropertyName("MaxFramerateLobbyLimit")]
    public int? MaxFramerateLobbyLimit
    {
        get;
        set;
    }

    [JsonPropertyName("MinFramerateLimit")]
    public int? MinFramerateLimit
    {
        get;
        set;
    }
}

public record MemoryManagementSettings
{
    [JsonPropertyName("AggressiveGC")]
    public bool? AggressiveGC
    {
        get;
        set;
    }

    [JsonPropertyName("GigabytesRequiredToDisableGCDuringRaid")]
    public int? GigabytesRequiredToDisableGCDuringRaid
    {
        get;
        set;
    }

    [JsonPropertyName("HeapPreAllocationEnabled")]
    public bool? HeapPreAllocationEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("HeapPreAllocationMB")]
    public int? HeapPreAllocationMB
    {
        get;
        set;
    }

    [JsonPropertyName("OverrideRamCleanerSettings")]
    public bool? OverrideRamCleanerSettings
    {
        get;
        set;
    }

    [JsonPropertyName("RamCleanerEnabled")]
    public bool? RamCleanerEnabled
    {
        get;
        set;
    }
}

public record ReleaseProfiler
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("MaxRecords")]
    public int? MaxRecords
    {
        get;
        set;
    }

    [JsonPropertyName("RecordTriggerValue")]
    public int? RecordTriggerValue
    {
        get;
        set;
    }
}

public record NetworkStateView
{
    [JsonPropertyName("LossThreshold")]
    public int? LossThreshold
    {
        get;
        set;
    }

    [JsonPropertyName("RttThreshold")]
    public int? RttThreshold
    {
        get;
        set;
    }
}

public record AudioGroupPreset
{
    [JsonPropertyName("AngleToAllowBinaural")]
    public double? AngleToAllowBinaural
    {
        get;
        set;
    }

    [JsonPropertyName("DisabledBinauralByDistance")]
    public bool? DisabledBinauralByDistance
    {
        get;
        set;
    }

    [JsonPropertyName("DistanceToAllowBinaural")]
    public double? DistanceToAllowBinaural
    {
        get;
        set;
    }

    [JsonPropertyName("GroupType")]
    public double? GroupType
    {
        get;
        set;
    }

    [JsonPropertyName("HeightToAllowBinaural")]
    public double? HeightToAllowBinaural
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

    [JsonPropertyName("OcclusionEnabled")]
    public bool? OcclusionEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("OcclusionIntensity")]
    public double? OcclusionIntensity
    {
        get;
        set;
    }

    [JsonPropertyName("OcclusionRolloffScale")]
    public double? OcclusionRolloffScale
    {
        get;
        set;
    }

    [JsonPropertyName("OverallVolume")]
    public double? OverallVolume
    {
        get;
        set;
    }
}

public record EnvironmentSettings
{
    [JsonPropertyName("AutumnLateSettings")]
    public SeasonEnvironmentSettings AutumnLateSettings
    {
        get;
        set;
    }

    [JsonPropertyName("AutumnSettings")]
    public SeasonEnvironmentSettings AutumnSettings
    {
        get;
        set;
    }

    [JsonPropertyName("SpringEarlySettings")]
    public SeasonEnvironmentSettings SpringEarlySettings
    {
        get;
        set;
    }

    [JsonPropertyName("SpringSettings")]
    public SeasonEnvironmentSettings SpringSettings
    {
        get;
        set;
    }

    [JsonPropertyName("StormSettings")]
    public SeasonEnvironmentSettings StormSettings
    {
        get;
        set;
    }

    [JsonPropertyName("SummerSettings")]
    public SeasonEnvironmentSettings SummerSettings
    {
        get;
        set;
    }

    [JsonPropertyName("WinterSettings")]
    public SeasonEnvironmentSettings WinterSettings
    {
        get;
        set;
    }

    [JsonPropertyName("SurfaceMultipliers")]
    public List<SurfaceMultiplier>? SurfaceMultipliers
    {
        get;
        set;
    }
}

public record SeasonEnvironmentSettings
{
    [JsonPropertyName("RainSettings")]
    public List<RainSetting> RainSettings
    {
        get;
        set;
    }

    [JsonPropertyName("StepsVolumeMultiplier")]
    public double StepsVolumeMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("WindMultipliers")]
    public List<WindMultiplier> WindMultipliers
    {
        get;
        set;
    }
}

public record SurfaceMultiplier
{
    public string SurfaceType
    {
        get;
        set;
    }

    public double VolumeMult
    {
        get;
        set;
    }
}

public record WindMultiplier
{
    [JsonPropertyName("VolumeMult")]
    public double VolumeMult
    {
        get;
        set;
    }

    [JsonPropertyName("WindSpeed")]
    public string WindSpeed
    {
        get;
        set;
    }
}

public record RainSetting
{
    [JsonPropertyName("IndoorVolumeMult")]
    public int IndoorVolumeMult
    {
        get;
        set;
    }

    [JsonPropertyName("OutdoorVolumeMult")]
    public double OutdoorVolumeMult
    {
        get;
        set;
    }

    [JsonPropertyName("RainIntensity")]
    public string RainIntensity
    {
        get;
        set;
    }
}

public record HeadphoneSettings
{
    public double FadeDuration
    {
        get;
        set;
    }

    public string FadeIn
    {
        get;
        set;
    }

    public string FadeOut
    {
        get;
        set;
    }
}

public record MetaXRAudioPluginSettings
{
    public bool? EnabledPluginErrorChecker
    {
        get;
        set;
    }

    public double? OutputVolumeCheckCooldown
    {
        get;
        set;
    }
}
