using System.Text.Json.Serialization;

namespace Core.Models.Spt.Server;

public class ServerBase
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }
}