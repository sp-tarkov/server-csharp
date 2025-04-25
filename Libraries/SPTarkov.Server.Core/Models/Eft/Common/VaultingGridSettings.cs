using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record VaultingGridSettings
{
    [JsonPropertyName("GridSizeX")]
    public double? GridSizeX
    {
        get;
        set;
    }

    [JsonPropertyName("GridSizeY")]
    public double? GridSizeY
    {
        get;
        set;
    }

    [JsonPropertyName("GridSizeZ")]
    public double? GridSizeZ
    {
        get;
        set;
    }

    [JsonPropertyName("SteppingLengthX")]
    public double? SteppingLengthX
    {
        get;
        set;
    }

    [JsonPropertyName("SteppingLengthY")]
    public double? SteppingLengthY
    {
        get;
        set;
    }

    [JsonPropertyName("SteppingLengthZ")]
    public double? SteppingLengthZ
    {
        get;
        set;
    }

    [JsonPropertyName("GridOffsetX")]
    public double? GridOffsetX
    {
        get;
        set;
    }

    [JsonPropertyName("GridOffsetY")]
    public double? GridOffsetY
    {
        get;
        set;
    }

    [JsonPropertyName("GridOffsetZ")]
    public double? GridOffsetZ
    {
        get;
        set;
    }

    [JsonPropertyName("OffsetFactor")]
    public double? OffsetFactor
    {
        get;
        set;
    }
}
