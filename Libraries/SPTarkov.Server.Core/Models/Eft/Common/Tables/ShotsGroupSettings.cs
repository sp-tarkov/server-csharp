using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ShotsGroupSettings
{
    [JsonPropertyName("EndShotIndex")]
    public double? EndShotIndex
    {
        get;
        set;
    }

    [JsonPropertyName("ShotRecoilPositionStrength")]
    public XYZ? ShotRecoilPositionStrength
    {
        get;
        set;
    }

    [JsonPropertyName("ShotRecoilRadianRange")]
    public XYZ? ShotRecoilRadianRange
    {
        get;
        set;
    }

    [JsonPropertyName("ShotRecoilRotationStrength")]
    public XYZ? ShotRecoilRotationStrength
    {
        get;
        set;
    }

    [JsonPropertyName("StartShotIndex")]
    public double? StartShotIndex
    {
        get;
        set;
    }
}
