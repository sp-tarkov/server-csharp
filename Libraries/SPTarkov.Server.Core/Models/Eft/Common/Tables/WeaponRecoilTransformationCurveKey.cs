using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record WeaponRecoilTransformationCurveKey
{
    [JsonPropertyName("inTangent")]
    public double? InTangent
    {
        get;
        set;
    }

    [JsonPropertyName("outTangent")]
    public double? OutTangent
    {
        get;
        set;
    }

    [JsonPropertyName("time")]
    public double? Time
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }
}
