using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record WeaponRecoilSettings
{
    [JsonPropertyName("Enable")]
    public bool? Enable
    {
        get;
        set;
    }

    [JsonPropertyName("Values")]
    public List<WeaponRecoilSettingValues>? Values
    {
        get;
        set;
    }
}
