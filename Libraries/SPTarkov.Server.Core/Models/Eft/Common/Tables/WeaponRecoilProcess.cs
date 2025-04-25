using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record WeaponRecoilProcess
{
    [JsonPropertyName("ComponentType")]
    public string? ComponentType
    {
        get;
        set;
    }

    [JsonPropertyName("CurveAimingValueMultiply")]
    public double? CurveAimingValueMultiply
    {
        get;
        set;
    }

    [JsonPropertyName("CurveTimeMultiply")]
    public double? CurveTimeMultiply
    {
        get;
        set;
    }

    [JsonPropertyName("CurveValueMultiply")]
    public double? CurveValueMultiply
    {
        get;
        set;
    }

    [JsonPropertyName("TransformationCurve")]
    public WeaponRecoilTransformationCurve? TransformationCurve
    {
        get;
        set;
    }
}
