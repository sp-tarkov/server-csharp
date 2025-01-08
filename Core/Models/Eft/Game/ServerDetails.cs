using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class ServerDetails
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("port")]
    public int? Port { get; set; }
}