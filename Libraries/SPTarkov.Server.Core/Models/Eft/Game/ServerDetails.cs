using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record ServerDetails
{
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
