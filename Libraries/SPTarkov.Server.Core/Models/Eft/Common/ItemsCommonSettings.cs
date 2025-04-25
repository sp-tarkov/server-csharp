using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ItemsCommonSettings
{
    [JsonPropertyName("ItemRemoveAfterInterruptionTime")]
    public double? ItemRemoveAfterInterruptionTime
    {
        get;
        set;
    }
}
