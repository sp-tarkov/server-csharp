using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public record Server
{
    [JsonPropertyName("ping")]
    public int? Ping
    {
        get;
        set;
    }

    [JsonPropertyName("ip")]
    public string? Ip
    {
        get;
        set;
    }

    [JsonPropertyName("port")]
    public int? Port
    {
        get;
        set;
    }
}
