using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record PutMetricsRequestData : IRequestData
{
    [JsonPropertyName("sid")]
    public string? SessionId
    {
        get;
        set;
    }

    [JsonPropertyName("Settings")]
    public object? Settings
    {
        get;
        set;
    }

    [JsonPropertyName("SharedSettings")]
    public SharedSettings? SharedSettings
    {
        get;
        set;
    }

    [JsonPropertyName("HardwareDescription")]
    public HardwareDescription? HardwareDescription
    {
        get;
        set;
    }

    [JsonPropertyName("Location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("Metrics")]
    public object? Metrics
    {
        get;
        set;
    }

    [JsonPropertyName("ClientEvents")]
    public ClientEvents? ClientEvents
    {
        get;
        set;
    }

    [JsonPropertyName("SpikeSamples")]
    public List<object>? SpikeSamples
    {
        get;
        set;
    }

    [JsonPropertyName("mode")]
    public string? Mode
    {
        get;
        set;
    }
}

public record SharedSettings
{
    [JsonPropertyName("StatedFieldOfView")]
    public double? StatedFieldOfView
    {
        get;
        set;
    }
}

public record HardwareDescription
{
    [JsonPropertyName("deviceUniqueIdentifier")]
    public string? DeviceUniqueIdentifier
    {
        get;
        set;
    }

    [JsonPropertyName("systemMemorySize")]
    public double? SystemMemorySize
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceID")]
    public double? GraphicsDeviceId
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceName")]
    public string? GraphicsDeviceName
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceType")]
    public string? GraphicsDeviceType
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceVendor")]
    public string? GraphicsDeviceVendor
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceVendorID")]
    public double? GraphicsDeviceVendorId
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsDeviceVersion")]
    public string? GraphicsDeviceVersion
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsMemorySize")]
    public double? GraphicsMemorySize
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsMultiThreaded")]
    public bool? GraphicsMultiThreaded
    {
        get;
        set;
    }

    [JsonPropertyName("graphicsShaderLevel")]
    public double? GraphicsShaderLevel
    {
        get;
        set;
    }

    [JsonPropertyName("operatingSystem")]
    public string? OperatingSystem
    {
        get;
        set;
    }

    [JsonPropertyName("processorCount")]
    public double? ProcessorCount
    {
        get;
        set;
    }

    [JsonPropertyName("processorFrequency")]
    public double? ProcessorFrequency
    {
        get;
        set;
    }

    [JsonPropertyName("processorType")]
    public string? ProcessorType
    {
        get;
        set;
    }

    [JsonPropertyName("driveType")]
    public string? DriveType
    {
        get;
        set;
    }

    [JsonPropertyName("swapDriveType")]
    public string? SwapDriveType
    {
        get;
        set;
    }
}

public record ClientEvents
{
    [JsonPropertyName("MatchingCompleted")]
    public double? MatchingCompleted
    {
        get;
        set;
    }

    [JsonPropertyName("MatchingCompletedReal")]
    public double? MatchingCompletedReal
    {
        get;
        set;
    }

    [JsonPropertyName("LocationLoaded")]
    public double? LocationLoaded
    {
        get;
        set;
    }

    [JsonPropertyName("LocationLoadedReal")]
    public double? LocationLoadedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GamePrepared")]
    public double? GamePrepared
    {
        get;
        set;
    }

    [JsonPropertyName("GamePreparedReal")]
    public double? GamePreparedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameCreated")]
    public double? GameCreated
    {
        get;
        set;
    }

    [JsonPropertyName("GameCreatedReal")]
    public double? GameCreatedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GamePooled")]
    public double? GamePooled
    {
        get;
        set;
    }

    [JsonPropertyName("GamePooledReal")]
    public double? GamePooledReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameRunned")]
    public double? GameRunned
    {
        get;
        set;
    }

    [JsonPropertyName("GameRunnedReal")]
    public double? GameRunnedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawn")]
    public double? GameSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawnReal")]
    public double? GameSpawnReal
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerSpawnEvent")]
    public double? PlayerSpawnEvent
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerSpawnEventReal")]
    public double? PlayerSpawnEventReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawned")]
    public double? GameSpawned
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawnedReal")]
    public double? GameSpawnedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameStarting")]
    public double? GameStarting
    {
        get;
        set;
    }

    [JsonPropertyName("GameStartingReal")]
    public double? GameStartingReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameStarted")]
    public double? GameStarted
    {
        get;
        set;
    }

    [JsonPropertyName("GameStartedReal")]
    public double? GameStartedReal
    {
        get;
        set;
    }
}
