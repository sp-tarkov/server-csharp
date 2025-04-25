using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record WeaponRecoilSettingValues
{
    [JsonPropertyName("Enable")]
    public bool? Enable
    {
        get;
        set;
    }

    [JsonPropertyName("Process")]
    public WeaponRecoilProcess? Process
    {
        get;
        set;
    }

    [JsonPropertyName("Target")]
    public string? Target
    {
        get;
        set;
    }
}
