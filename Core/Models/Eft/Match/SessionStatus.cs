using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class SessionStatus
{
    [JsonPropertyName("profileid")]
    public string? ProfileId { get; set; }

    [JsonPropertyName("profileToken")]
    public string? ProfileToken { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("port")]
    public int? Port { get; set; }

    [JsonPropertyName("sid")]
    public string? Sid { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("raidMode")]
    public string? RaidMode { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("shortId")]
    public string? ShortId { get; set; }

    [JsonPropertyName("additional_info")]
    public List<object>? AdditionalInfo { get; set; }
}