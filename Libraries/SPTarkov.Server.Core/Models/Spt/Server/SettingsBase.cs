using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record SettingsBase
{
    [JsonPropertyName("config")]
    public required Config Configuration { get; init; }
}

public record Config
{
    [JsonPropertyName("AFKTimeoutSeconds")]
    public int AFKTimeoutSeconds { get; set; }

    [JsonPropertyName("AdditionalRandomDelaySeconds")]
    public int AdditionalRandomDelaySeconds { get; set; }

    [JsonPropertyName("AudioSettings")]
    public AudioSettings AudioSettings { get; set; }

    [JsonPropertyName("ClientSendRateLimit")]
    public int ClientSendRateLimit { get; set; }

    [JsonPropertyName("CriticalRetriesCount")]
    public int CriticalRetriesCount { get; set; }

    [JsonPropertyName("DefaultRetriesCount")]
    public int DefaultRetriesCount { get; set; }

    [JsonPropertyName("FirstCycleDelaySeconds")]
    public int FirstCycleDelaySeconds { get; set; }

    [JsonPropertyName("FramerateLimit")]
    public FramerateLimit FramerateLimit { get; set; }

    [JsonPropertyName("GroupStatusInterval")]
    public int GroupStatusInterval { get; set; }

    [JsonPropertyName("GroupStatusButtonInterval")]
    public int GroupStatusButtonInterval { get; set; }

    [JsonPropertyName("KeepAliveInterval")]
    public int KeepAliveInterval { get; set; }

    [JsonPropertyName("LobbyKeepAliveInterval")]
    public int LobbyKeepAliveInterval { get; set; }

    [JsonPropertyName("Mark502and504AsNonImportant")]
    public bool Mark502and504AsNonImportant { get; set; }

    [JsonPropertyName("MemoryManagementSettings")]
    public MemoryManagementSettings MemoryManagementSettings { get; set; }

    [JsonPropertyName("NVidiaHighlights")]
    public bool NVidiaHighlights { get; set; }

    [JsonPropertyName("NextCycleDelaySeconds")]
    public int NextCycleDelaySeconds { get; set; }

    // TODO: this property currently is an empty array on json
    [JsonPropertyName("NotifierLobbyAidsForce")]
    public object[] NotifierLobbyAidsForce { get; set; }

    [JsonPropertyName("NotifierLobbyPercentage")]
    public int NotifierLobbyPercentage { get; set; }

    [JsonPropertyName("NotifierUseLobby")]
    public bool NotifierUseLobby { get; set; }

    [JsonPropertyName("PingServerResultSendInterval")]
    public int PingServerResultSendInterval { get; set; }

    [JsonPropertyName("PingServersInterval")]
    public int PingServersInterval { get; set; }

    [JsonPropertyName("ReleaseProfiler")]
    public ReleaseProfiler ReleaseProfiler { get; set; }

    [JsonPropertyName("RequestConfirmationTimeouts")]
    public List<double> RequestConfirmationTimeouts { get; set; }

    [JsonPropertyName("RequestsMadeThroughLobby")]
    public List<string> RequestsMadeThroughLobby { get; set; }

    [JsonPropertyName("SecondCycleDelaySeconds")]
    public int SecondCycleDelaySeconds { get; set; }

    [JsonPropertyName("ShouldEstablishLobbyConnection")]
    public bool ShouldEstablishLobbyConnection { get; set; }

    [JsonPropertyName("TurnOffLogging")]
    public bool TurnOffLogging { get; set; }

    [JsonPropertyName("WeaponOverlapDistanceCulling")]
    public int WeaponOverlapDistanceCulling { get; set; }

    [JsonPropertyName("WebDiagnosticsEnabled")]
    public bool WebDiagnosticsEnabled { get; set; }

    [JsonPropertyName("NetworkStateView")]
    public NetworkStateView NetworkStateView { get; set; }

    [JsonPropertyName("WsReconnectionDelays")]
    public List<int> WsReconnectionDelays { get; set; }
}

public record AudioSettings
{
    [JsonPropertyName("AudioGroupPresets")]
    public List<AudioGroupPreset> AudioGroupPresets { get; set; }

    [JsonPropertyName("EnvironmentSettings")]
    public EnvironmentSettings EnvironmentSettings { get; set; }

    [JsonPropertyName("HeadphonesSettings")]
    public HeadphoneSettings HeadphonesSettings { get; set; }

    [JsonPropertyName("MetaXRAudioPluginSettings")]
    public MetaXRAudioPluginSettings MetaXRAudioPluginSettings { get; set; }

    [JsonPropertyName("OcclusionSettings")]
    public AudioOcclusionSettings OcclusionSettings { get; set; }

    [JsonPropertyName("PlayerSettings")]
    public PlayerSettings PlayerSettings { get; set; }
}

public record FramerateLimit
{
    [JsonPropertyName("MaxFramerateGameLimit")]
    public int MaxFramerateGameLimit { get; set; }

    [JsonPropertyName("MaxFramerateLobbyLimit")]
    public int MaxFramerateLobbyLimit { get; set; }

    [JsonPropertyName("MinFramerateLimit")]
    public int MinFramerateLimit { get; set; }
}

public record MemoryManagementSettings
{
    [JsonPropertyName("AggressiveGC")]
    public bool AggressiveGC { get; set; }

    [JsonPropertyName("GigabytesRequiredToDisableGCDuringRaid")]
    public int GigabytesRequiredToDisableGCDuringRaid { get; set; }

    [JsonPropertyName("HeapPreAllocationEnabled")]
    public bool HeapPreAllocationEnabled { get; set; }

    [JsonPropertyName("HeapPreAllocationMB")]
    public int HeapPreAllocationMB { get; set; }

    [JsonPropertyName("OverrideRamCleanerSettings")]
    public bool OverrideRamCleanerSettings { get; set; }

    [JsonPropertyName("RamCleanerEnabled")]
    public bool RamCleanerEnabled { get; set; }
}

public record ReleaseProfiler
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("MaxRecords")]
    public int MaxRecords { get; set; }

    [JsonPropertyName("RecordTriggerValue")]
    public int RecordTriggerValue { get; set; }
}

public record NetworkStateView
{
    [JsonPropertyName("LossThreshold")]
    public int LossThreshold { get; set; }

    [JsonPropertyName("RttThreshold")]
    public int RttThreshold { get; set; }
}

public record AudioGroupPreset
{
    [JsonPropertyName("AngleToAllowBinaural")]
    public double? AngleToAllowBinaural { get; set; }

    [JsonPropertyName("DisabledBinauralByDistance")]
    public bool? DisabledBinauralByDistance { get; set; }

    [JsonPropertyName("DistanceToAllowBinaural")]
    public double? DistanceToAllowBinaural { get; set; }

    [JsonPropertyName("GroupType")]
    public double? GroupType { get; set; }

    [JsonPropertyName("HeightToAllowBinaural")]
    public double? HeightToAllowBinaural { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("OcclusionEnabled")]
    public bool? OcclusionEnabled { get; set; }

    [JsonPropertyName("OcclusionIntensity")]
    public double? OcclusionIntensity { get; set; }

    [JsonPropertyName("OcclusionRolloffScale")]
    public double? OcclusionRolloffScale { get; set; }

    [JsonPropertyName("OverallVolume")]
    public double? OverallVolume { get; set; }
}

public record EnvironmentSettings
{
    [JsonPropertyName("AutumnLateSettings")]
    public SeasonEnvironmentSettings AutumnLateSettings { get; set; }

    [JsonPropertyName("AutumnSettings")]
    public SeasonEnvironmentSettings AutumnSettings { get; set; }

    [JsonPropertyName("SpringEarlySettings")]
    public SeasonEnvironmentSettings SpringEarlySettings { get; set; }

    [JsonPropertyName("SpringSettings")]
    public SeasonEnvironmentSettings SpringSettings { get; set; }

    [JsonPropertyName("StormSettings")]
    public SeasonEnvironmentSettings StormSettings { get; set; }

    [JsonPropertyName("SummerSettings")]
    public SeasonEnvironmentSettings SummerSettings { get; set; }

    [JsonPropertyName("WinterSettings")]
    public SeasonEnvironmentSettings WinterSettings { get; set; }

    [JsonPropertyName("SurfaceMultipliers")]
    public List<SurfaceMultiplier>? SurfaceMultipliers { get; set; }
}

public record SeasonEnvironmentSettings
{
    [JsonPropertyName("RainSettings")]
    public List<RainSetting> RainSettings { get; set; }

    [JsonPropertyName("StepsVolumeMultiplier")]
    public double StepsVolumeMultiplier { get; set; }

    [JsonPropertyName("WindMultipliers")]
    public List<WindMultiplier> WindMultipliers { get; set; }
}

public record SurfaceMultiplier
{
    public string SurfaceType { get; set; }

    public double VolumeMult { get; set; }
}

public record WindMultiplier
{
    [JsonPropertyName("VolumeMult")]
    public double VolumeMult { get; set; }

    [JsonPropertyName("WindSpeed")]
    public string WindSpeed { get; set; }
}

public record RainSetting
{
    [JsonPropertyName("IndoorVolumeMult")]
    public double IndoorVolumeMult { get; set; }

    [JsonPropertyName("OutdoorVolumeMult")]
    public double OutdoorVolumeMult { get; set; }

    [JsonPropertyName("RainIntensity")]
    public string RainIntensity { get; set; }
}

public record HeadphoneSettings
{
    public double FadeDuration { get; set; }

    public string FadeIn { get; set; }

    public string FadeOut { get; set; }
}

public record MetaXRAudioPluginSettings
{
    public bool EnabledPluginErrorChecker { get; set; }

    public double OutputVolumeCheckCooldown { get; set; }

    [JsonPropertyName("audioGroupAcousticSettings")]
    public List<AudioGroupAcousticSetting> AudioGroupAcousticSettings { get; set; }
}

public record AudioGroupAcousticSetting
{
    [JsonPropertyName("acousticSettings")]
    public AcousticSettings AcousticSettings { get; set; }

    [JsonPropertyName("groupType")]
    public string GroupType { get; set; }
}

public record AcousticSettings
{
    [JsonPropertyName("enabledPrewarm")]
    public bool enabledPrewarm { get; set; }

    [JsonPropertyName("mono")]
    public AudioProperties Mono { get; set; }

    [JsonPropertyName("stereo")]
    public AudioProperties Stereo { get; set; }
}

public record AudioProperties
{
    [JsonPropertyName("earlyReflectionsSendDb")]
    public int EarlyReflectionsSendDb { get; set; }

    [JsonPropertyName("enabledReverb")]
    public bool EnabledReverb { get; set; }

    [JsonPropertyName("reverbReach")]
    public double ReverbReach { get; set; }

    [JsonPropertyName("reverbSendDb")]
    public int ReverbSendDb { get; set; }
}

public record AudioOcclusionSettings
{
    [JsonPropertyName("audioGroupOcclusionSettings")]
    public List<AudioGroupOcclusionSetting> AudioGroupOcclusionSettings { get; set; }

    [JsonPropertyName("locationOcclusionSettings")]
    public LocationOcclusionSettings LocationOcclusionSettings { get; set; }
}

public record AudioGroupOcclusionSetting
{
    [JsonPropertyName("groupType")]
    public string GroupType { get; set; }

    [JsonPropertyName("occlusionSettings")]
    public OcclusionSettings OcclusionSettings { get; set; }
}

public record OcclusionSettings
{
    [JsonPropertyName("indoorToOutdoorFactor")]
    public double IndoorToOutdoorFactor { get; set; }

    [JsonPropertyName("maxQualityFactor")]
    public double MaxQualityFactor { get; set; }

    [JsonPropertyName("obstructionEQPreset")]
    public EQPreset ObstructionEQPreset { get; set; }

    [JsonPropertyName("occlusionEnabled")]
    public bool OcclusionEnabled { get; set; }

    [JsonPropertyName("occlusionIntensity")]
    public int OcclusionIntensity { get; set; }

    [JsonPropertyName("outdoorToIndoorFactor")]
    public double OutdoorToIndoorFactor { get; set; }

    [JsonPropertyName("propagationEQPreset")]
    public EQPreset PropagationEQPreset { get; set; }

    [JsonPropertyName("rolloffScale")]
    public double RolloffScale { get; set; }

    [JsonPropertyName("stairsHeightCurve")]
    public VolumeCurve StairsHeightCurve { get; set; }

    [JsonPropertyName("useQualityCompression")]
    public bool UseQualityCompression { get; set; }
}

public record EQPreset
{
    [JsonPropertyName("distanceCoefficient")]
    public double DistanceCoefficient { get; set; }

    [JsonPropertyName("environmentVolumeThresholds")]
    public EnvironmentVolumeThresholds EnvironmentVolumeThresholds { get; set; }

    [JsonPropertyName("heightVolumeCurve")]
    public VolumeCurve HeightVolumeCurve { get; set; }

    [JsonPropertyName("hpfSettings")]
    public PfSettings HpfSettings { get; set; }

    [JsonPropertyName("lpfSettings")]
    public PfSettings LpfSettings { get; set; }

    [JsonPropertyName("rotationCoefficient")]
    public double RotationCoefficient { get; set; }

    [JsonPropertyName("volumeCurve")]
    public VolumeCurve VolumeCurve { get; set; }
}

public record EnvironmentVolumeThresholds
{
    [JsonPropertyName("baseValue")]
    public double BaseValue { get; set; }

    [JsonPropertyName("diffEnvironmentIsolated")]
    public double DiffEnvironmentIsolated { get; set; }

    [JsonPropertyName("diffRoomsTypeIsolated")]
    public double DiffRoomsTypeIsolated { get; set; }

    [JsonPropertyName("indoorIsolated")]
    public double IndoorIsolated { get; set; }

    [JsonPropertyName("indoorToOutdoor")]
    public double IndoorToOutdoor { get; set; }

    [JsonPropertyName("outdoorToIndoor")]
    public double OutdoorToIndoor { get; set; }
}

public record PfSettings
{
    [JsonPropertyName("distanceCurve")]
    public VolumeCurve DistanceCurve { get; set; }

    [JsonPropertyName("environmentEqThresholds")]
    public EnvironmentEqThresholds EnvironmentEqThresholds { get; set; }

    [JsonPropertyName("frequencyCurve")]
    public VolumeCurve FrequencyCurve { get; set; }

    [JsonPropertyName("heightCurve")]
    public VolumeCurve HeightCurve { get; set; }

    [JsonPropertyName("positionEqThresholds")]
    public PositionEqThresholds PositionEqThresholds { get; set; }

    [JsonPropertyName("resonanceCurve")]
    public VolumeCurve ResonanceCurve { get; set; }
}

public record VolumeCurve
{
    [JsonPropertyName("m_Curve")]
    public List<MCurve> MCurve { get; set; }

    [JsonPropertyName("m_PostInfinity")]
    public int MPostInfinity { get; set; }

    [JsonPropertyName("m_PreInfinity")]
    public int MPreInfinity { get; set; }

    [JsonPropertyName("m_RotationOrder")]
    public int MRotationOrder { get; set; }

    [JsonPropertyName("serializedVersion")]
    public string SerializedVersion { get; set; }
}

public class EnvironmentEqThresholds
{
    [JsonPropertyName("baseValue")]
    public double BaseValue { get; set; }

    [JsonPropertyName("diffEnvironmentIsolated")]
    public int DiffEnvironmentIsolated { get; set; }

    [JsonPropertyName("diffRoomsTypeIsolated")]
    public double DiffRoomsTypeIsolated { get; set; }

    [JsonPropertyName("indoorIsolated")]
    public double IndoorIsolated { get; set; }

    [JsonPropertyName("indoorToOutdoor")]
    public double IndoorToOutdoor { get; set; }

    [JsonPropertyName("outdoorToIndoor")]
    public double OutdoorToIndoor { get; set; }
}

public class PositionEqThresholds
{
    [JsonPropertyName("aboveFreq")]
    public int AboveFreq { get; set; }

    [JsonPropertyName("behindFreq")]
    public int BehindFreq { get; set; }

    [JsonPropertyName("belowFreq")]
    public int BelowFreq { get; set; }

    [JsonPropertyName("levelFreq")]
    public int LevelFreq { get; set; }
}

public class MCurve
{
    [JsonPropertyName("inSlope")]
    public double InSlope { get; set; }

    [JsonPropertyName("inWeight")]
    public double InWeight { get; set; }

    [JsonPropertyName("outSlope")]
    public double OutSlope { get; set; }

    [JsonPropertyName("outWeight")]
    public double OutWeight { get; set; }

    [JsonPropertyName("serializedVersion")]
    public string SerializedVersion { get; set; }

    [JsonPropertyName("tangentMode")]
    public int TangentMode { get; set; }

    [JsonPropertyName("time")]
    public double Time { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("weightedMode")]
    public int WeightedMode { get; set; }
}

public record LocationOcclusionSettings
{
    [JsonPropertyName("commonSettings")]
    public CommonSettings CommonSettings { get; set; }

    [JsonPropertyName("diffractionSettings")]
    public DiffractionSettings DiffractionSettings { get; set; }

    [JsonPropertyName("propagationSettings")]
    public PropagationSettings PropagationSettings { get; set; }

    [JsonPropertyName("reflectionSettings")]
    public ReflectionSettings ReflectionSettings { get; set; }

    [JsonPropertyName("transmissionSettings")]
    public TransmissionSettings TransmissionSettings { get; set; }
}

public class CommonSettings
{
    [JsonPropertyName("diffractionThreshold")]
    public double DiffractionThreshold { get; set; }

    [JsonPropertyName("effectChangeThreshold")]
    public double EffectChangeThreshold { get; set; }

    [JsonPropertyName("floorHeight")]
    public double FloorHeight { get; set; }

    [JsonPropertyName("maxDistance")]
    public int MaxDistance { get; set; }

    [JsonPropertyName("playerObstructionYOffset")]
    public double PlayerObstructionYOffset { get; set; }

    [JsonPropertyName("positionChangeThreshold")]
    public List<ChangeThreshold> PositionChangeThreshold { get; set; }

    [JsonPropertyName("smoothingFactor")]
    public int SmoothingFactor { get; set; }

    [JsonPropertyName("transmissionThreshold")]
    public double TransmissionThreshold { get; set; }
}

public class ChangeThreshold
{
    [JsonPropertyName("audioQuality")]
    public string AudioQuality { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class DiffractionSettings
{
    [JsonPropertyName("edgeSearchRayCount")]
    public List<ChangeThreshold> EdgeSearchRayCount { get; set; }

    [JsonPropertyName("edgeSearchRayLength")]
    public double EdgeSearchRayLength { get; set; }

    [JsonPropertyName("edgeValidationRayOffset")]
    public double EdgeValidationRayOffset { get; set; }

    [JsonPropertyName("maxEdgeDist")]
    public double MaxEdgeDist { get; set; }

    [JsonPropertyName("maxPathFactor")]
    public int MaxPathFactor { get; set; }
}

public class PropagationSettings
{
    [JsonPropertyName("absoluteHeightWeight")]
    public double AbsoluteHeightWeight { get; set; }

    [JsonPropertyName("diffractionExponent")]
    public double DiffractionExponent { get; set; }

    [JsonPropertyName("distanceWeight")]
    public double DistanceWeight { get; set; }

    [JsonPropertyName("heightExponent")]
    public double HeightExponent { get; set; }

    [JsonPropertyName("maxSegmentLength")]
    public int MaxSegmentLength { get; set; }

    [JsonPropertyName("minPortalCostPercent")]
    public double MinPortalCostPercent { get; set; }

    [JsonPropertyName("relaxationIterations")]
    public int RelaxationIterations { get; set; }

    [JsonPropertyName("routesCompressionFactorByQuality")]
    public List<ChangeThreshold> RoutesCompressionFactorByQuality { get; set; }

    [JsonPropertyName("segmentHeightWeightDown")]
    public double SegmentHeightWeightDown { get; set; }

    [JsonPropertyName("segmentHeightWeightUp")]
    public double SegmentHeightWeightUp { get; set; }

    [JsonPropertyName("typicalRoomHeight")]
    public double TypicalRoomHeight { get; set; }
}

public class ReflectionSettings
{
    [JsonPropertyName("energyLossFactorPerReflection")]
    public double EnergyLossFactorPerReflection { get; set; }

    [JsonPropertyName("initialRaysCount")]
    public List<ChangeThreshold> InitialRaysCount { get; set; }

    [JsonPropertyName("maxReflections")]
    public int MaxReflections { get; set; }

    [JsonPropertyName("minEnergyAtMaxDistance")]
    public double MinEnergyAtMaxDistance { get; set; }
}

public class TransmissionSettings
{
    [JsonPropertyName("absorptionPerUnit")]
    public double AbsorptionPerUnit { get; set; }

    [JsonPropertyName("initialRaysCount")]
    public List<ChangeThreshold> InitialRaysCount { get; set; }

    [JsonPropertyName("listenerHeightSamplingOffset")]
    public double ListenerHeightSamplingOffset { get; set; }

    [JsonPropertyName("minClearPathScore")]
    public double MinClearPathScore { get; set; }

    [JsonPropertyName("minEnergyThreshold")]
    public double MinEnergyThreshold { get; set; }

    [JsonPropertyName("obstacleMaxThickness")]
    public double ObstacleMaxThickness { get; set; }

    [JsonPropertyName("obstacleMinThickness")]
    public double ObstacleMinThickness { get; set; }

    [JsonPropertyName("raysWideningRadius")]
    public double RaysWideningRadius { get; set; }

    [JsonPropertyName("sourceHeightSamplingOffset")]
    public double SourceHeightSamplingOffset { get; set; }

    [JsonPropertyName("useRaycast")]
    public bool UseRaycast { get; set; }
}
