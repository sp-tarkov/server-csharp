using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BtrMapConfig
{
    [JsonPropertyName("BtrSkin")]
    public string? BtrSkin
    {
        get;
        set;
    }

    [JsonPropertyName("CheckSurfaceForWheelsTimer")]
    public double? CheckSurfaceForWheelsTimer
    {
        get;
        set;
    }

    [JsonPropertyName("DiameterWheel")]
    public double? DiameterWheel
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheel")]
    public double? HeightWheel
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelMaxPosLimit")]
    public double? HeightWheelMaxPosLimit
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelMinPosLimit")]
    public double? HeightWheelMinPosLimit
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelOffset")]
    public double? HeightWheelOffset
    {
        get;
        set;
    }

    [JsonPropertyName("SnapToSurfaceWheelsSpeed")]
    public double? SnapToSurfaceWheelsSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("SuspensionDamperStiffness")]
    public double? SuspensionDamperStiffness
    {
        get;
        set;
    }

    [JsonPropertyName("SuspensionRestLength")]
    public double? SuspensionRestLength
    {
        get;
        set;
    }

    [JsonPropertyName("SuspensionSpringStiffness")]
    public double? SuspensionSpringStiffness
    {
        get;
        set;
    }

    [JsonPropertyName("SuspensionTravel")]
    public double? SuspensionTravel
    {
        get;
        set;
    }

    [JsonPropertyName("SuspensionWheelRadius")]
    public double? SuspensionWheelRadius
    {
        get;
        set;
    }

    [JsonPropertyName("mapID")]
    public string? MapID
    {
        get;
        set;
    }

    [JsonPropertyName("pathsConfigurations")]
    public List<PathConfig>? PathsConfigurations
    {
        get;
        set;
    }
}
