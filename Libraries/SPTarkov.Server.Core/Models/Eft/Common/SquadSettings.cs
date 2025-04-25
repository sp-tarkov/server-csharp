using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SquadSettings
{
    [JsonPropertyName("CountOfRequestsToOnePlayer")]
    public double? CountOfRequestsToOnePlayer
    {
        get;
        set;
    }

    [JsonPropertyName("SecondsForExpiredRequest")]
    public double? SecondsForExpiredRequest
    {
        get;
        set;
    }

    [JsonPropertyName("SendRequestDelaySeconds")]
    public double? SendRequestDelaySeconds
    {
        get;
        set;
    }
}
