using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ShellingZone
{
    [JsonPropertyName("ID")]
    public double? ID
    {
        get;
        set;
    }

    [JsonPropertyName("PointsInShellings")]
    public XYZ? PointsInShellings
    {
        get;
        set;
    }

    [JsonPropertyName("ShellingRounds")]
    public double? ShellingRounds
    {
        get;
        set;
    }

    [JsonPropertyName("ShotCount")]
    public double? ShotCount
    {
        get;
        set;
    }

    [JsonPropertyName("PauseBetweenRounds")]
    public XYZ? PauseBetweenRounds
    {
        get;
        set;
    }

    [JsonPropertyName("PauseBetweenShots")]
    public XYZ? PauseBetweenShots
    {
        get;
        set;
    }

    [JsonPropertyName("Center")]
    public XYZ? Center
    {
        get;
        set;
    }

    [JsonPropertyName("Rotate")]
    public double? Rotate
    {
        get;
        set;
    }

    [JsonPropertyName("GridStep")]
    public XYZ? GridStep
    {
        get;
        set;
    }

    [JsonPropertyName("Points")]
    public XYZ? Points
    {
        get;
        set;
    }

    [JsonPropertyName("PointRadius")]
    public double? PointRadius
    {
        get;
        set;
    }

    [JsonPropertyName("ExplosionDistanceRange")]
    public XYZ? ExplosionDistanceRange
    {
        get;
        set;
    }

    [JsonPropertyName("AlarmStages")]
    public List<AlarmStage>? AlarmStages
    {
        get;
        set;
    }

    [JsonPropertyName("BeforeShellingSignalTime")]
    public double? BeforeShellingSignalTime
    {
        get;
        set;
    }

    [JsonPropertyName("UsedInPlanedShelling")]
    public bool? UsedInPlanedShelling
    {
        get;
        set;
    }

    [JsonPropertyName("UseInCalledShelling")]
    public bool? UseInCalledShelling
    {
        get;
        set;
    }

    [JsonPropertyName("IsActive")]
    public bool? IsActive
    {
        get;
        set;
    }
}
