using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Tables;

/// <summary>
/// Model for Assets/database/server.json
/// </summary>
public record ServerTable
{
    [JsonPropertyName("ip")]
    public required string Ip { get; set; }

    [JsonPropertyName("port")]
    public required int Port { get; set; }
}
