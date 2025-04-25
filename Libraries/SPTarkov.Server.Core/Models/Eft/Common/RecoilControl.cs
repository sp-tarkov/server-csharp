using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RecoilControl
{
    [JsonPropertyName("RecoilAction")]
    public double? RecoilAction
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilBonusPerLevel")]
    public double? RecoilBonusPerLevel
    {
        get;
        set;
    }
}
