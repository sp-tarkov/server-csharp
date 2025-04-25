using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ServerMapBtrsettings
{
    [JsonPropertyName("MapID")]
    public string? MapID
    {
        get;
        set;
    }

    [JsonPropertyName("ChanceSpawn")]
    public double? ChanceSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("SpawnPeriod")]
    public XYZ? SpawnPeriod
    {
        get;
        set;
    }

    [JsonPropertyName("MoveSpeed")]
    public float? MoveSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("ReadyToDepartureTime")]
    public float? ReadyToDepartureTime
    {
        get;
        set;
    }

    [JsonPropertyName("CheckTurnDistanceTime")]
    public float? CheckTurnDistanceTime
    {
        get;
        set;
    }

    [JsonPropertyName("TurnCheckSensitivity")]
    public float? TurnCheckSensitivity
    {
        get;
        set;
    }

    [JsonPropertyName("DecreaseSpeedOnTurnLimit")]
    public float? DecreaseSpeedOnTurnLimit
    {
        get;
        set;
    }

    [JsonPropertyName("EndSplineDecelerationDistance")]
    public float? EndSplineDecelerationDistance
    {
        get;
        set;
    }

    [JsonPropertyName("AccelerationSpeed")]
    public float? AccelerationSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("DecelerationSpeed")]
    public float? DecelerationSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("PauseDurationRange")]
    public XYZ? PauseDurationRange
    {
        get;
        set;
    }

    [JsonPropertyName("BodySwingReturnSpeed")]
    public float? BodySwingReturnSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("BodySwingDamping")]
    public float? BodySwingDamping
    {
        get;
        set;
    }

    [JsonPropertyName("BodySwingIntensity")]
    public float? BodySwingIntensity
    {
        get;
        set;
    }
}
