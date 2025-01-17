using System.Text.Json.Serialization;

namespace Core.Models.Spt.Server;

public record SettingsBase
{
    [JsonPropertyName("config")]
    public Config? Configuration { get; set; }
}

public record Config
{
    [JsonPropertyName("AFKTimeoutSeconds")]
    public int? AFKTimeoutSeconds { get; set; }

    [JsonPropertyName("AdditionalRandomDelaySeconds")]
    public int? AdditionalRandomDelaySeconds { get; set; }

    [JsonPropertyName("ClientSendRateLimit")]
    public int? ClientSendRateLimit { get; set; }

    [JsonPropertyName("CriticalRetriesCount")]
    public int? CriticalRetriesCount { get; set; }

    [JsonPropertyName("DefaultRetriesCount")]
    public int? DefaultRetriesCount { get; set; }

    [JsonPropertyName("FirstCycleDelaySeconds")]
    public int? FirstCycleDelaySeconds { get; set; }

    [JsonPropertyName("FramerateLimit")]
    public FramerateLimit? FramerateLimit { get; set; }

    [JsonPropertyName("GroupStatusInterval")]
    public int? GroupStatusInterval { get; set; }

    [JsonPropertyName("GroupStatusButtonInterval")]
    public int? GroupStatusButtonInterval { get; set; }

    [JsonPropertyName("KeepAliveInterval")]
    public int? KeepAliveInterval { get; set; }

    [JsonPropertyName("LobbyKeepAliveInterval")]
    public int? LobbyKeepAliveInterval { get; set; }

    [JsonPropertyName("Mark502and504AsNonImportant")]
    public bool? Mark502and504AsNonImportant { get; set; }

    [JsonPropertyName("MemoryManagementSettings")]
    public MemoryManagementSettings? MemoryManagementSettings { get; set; }

    [JsonPropertyName("NVidiaHighlights")]
    public bool? NVidiaHighlights { get; set; }

    [JsonPropertyName("NextCycleDelaySeconds")]
    public int? NextCycleDelaySeconds { get; set; }

    [JsonPropertyName("PingServerResultSendInterval")]
    public int? PingServerResultSendInterval { get; set; }

    [JsonPropertyName("PingServersInterval")]
    public int? PingServersInterval { get; set; }

    [JsonPropertyName("ReleaseProfiler")]
    public ReleaseProfiler? ReleaseProfiler { get; set; }

    [JsonPropertyName("RequestConfirmationTimeouts")]
    public List<double>? RequestConfirmationTimeouts { get; set; }

    [JsonPropertyName("RequestsMadeThroughLobby")]
    public List<string>? RequestsMadeThroughLobby { get; set; }

    [JsonPropertyName("SecondCycleDelaySeconds")]
    public int? SecondCycleDelaySeconds { get; set; }

    [JsonPropertyName("ShouldEstablishLobbyConnection")]
    public bool? ShouldEstablishLobbyConnection { get; set; }

    [JsonPropertyName("TurnOffLogging")]
    public bool? TurnOffLogging { get; set; }

    [JsonPropertyName("WeaponOverlapDistanceCulling")]
    public int? WeaponOverlapDistanceCulling { get; set; }

    [JsonPropertyName("WebDiagnosticsEnabled")]
    public bool? WebDiagnosticsEnabled { get; set; }

    [JsonPropertyName("NetworkStateView")]
    public NetworkStateView? NetworkStateView { get; set; }

    [JsonPropertyName("WsReconnectionDelays")]
    public List<int>? WsReconnectionDelays { get; set; }
}

public record FramerateLimit
{
    [JsonPropertyName("MaxFramerateGameLimit")]
    public int? MaxFramerateGameLimit { get; set; }

    [JsonPropertyName("MaxFramerateLobbyLimit")]
    public int? MaxFramerateLobbyLimit { get; set; }

    [JsonPropertyName("MinFramerateLimit")]
    public int? MinFramerateLimit { get; set; }
}

public record MemoryManagementSettings
{
    [JsonPropertyName("AggressiveGC")]
    public bool? AggressiveGC { get; set; }

    [JsonPropertyName("GigabytesRequiredToDisableGCDuringRaid")]
    public int? GigabytesRequiredToDisableGCDuringRaid { get; set; }

    [JsonPropertyName("HeapPreAllocationEnabled")]
    public bool? HeapPreAllocationEnabled { get; set; }

    [JsonPropertyName("HeapPreAllocationMB")]
    public int? HeapPreAllocationMB { get; set; }

    [JsonPropertyName("OverrideRamCleanerSettings")]
    public bool? OverrideRamCleanerSettings { get; set; }

    [JsonPropertyName("RamCleanerEnabled")]
    public bool? RamCleanerEnabled { get; set; }
}

public record ReleaseProfiler
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("MaxRecords")]
    public int? MaxRecords { get; set; }

    [JsonPropertyName("RecordTriggerValue")]
    public int? RecordTriggerValue { get; set; }
}

public record NetworkStateView
{
    [JsonPropertyName("LossThreshold")]
    public int? LossThreshold { get; set; }

    [JsonPropertyName("RttThreshold")]
    public int? RttThreshold { get; set; }
}
