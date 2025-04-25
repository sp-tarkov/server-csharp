using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BufferZone
{
    [JsonPropertyName("CustomerAccessTime")]
    public double? CustomerAccessTime
    {
        get;
        set;
    }

    [JsonPropertyName("CustomerCriticalTimeStart")]
    public double? CustomerCriticalTimeStart
    {
        get;
        set;
    }

    [JsonPropertyName("CustomerKickNotifTime")]
    public double? CustomerKickNotifTime
    {
        get;
        set;
    }
}
