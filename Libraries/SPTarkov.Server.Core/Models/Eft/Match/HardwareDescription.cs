using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

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
