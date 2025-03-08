using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameStartResponse
{
    [JsonPropertyName("utc_time")]
    public double UtcTime
    {
        get;
        set;
    }
}
