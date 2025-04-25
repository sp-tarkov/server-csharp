using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MinPlayerWaitTime
{
    [JsonPropertyName("minPlayers")]
    public int? MinPlayers
    {
        get;
        set;
    }

    [JsonPropertyName("time")]
    public long? Time
    {
        get;
        set;
    }
}
