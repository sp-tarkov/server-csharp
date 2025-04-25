using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record WeaponRecoilTransformationCurve
{
    [JsonPropertyName("Keys")]
    public List<WeaponRecoilTransformationCurveKey>? Keys
    {
        get;
        set;
    }
}
