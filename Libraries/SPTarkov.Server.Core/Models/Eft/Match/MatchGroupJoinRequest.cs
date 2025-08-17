using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupJoinRequest : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("savage")]
    public bool? Savage { get; set; }

    [JsonPropertyName("dt")]
    public string? Dt { get; set; }

    [JsonPropertyName("servers")]
    public List<JoinServer>? Servers { get; set; }

    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }
}

public record JoinServer
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("ping")]
    public int? Ping { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("port")]
    public string? Port { get; set; }
}
