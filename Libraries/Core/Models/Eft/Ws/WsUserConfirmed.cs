using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Ws;

public record WsUserConfirmed : WsNotificationEvent
{
    [JsonPropertyName("profileid")]
    public string? ProfileId { get; set; }

    [JsonPropertyName("profileToken")]
    public string? ProfileToken { get; set; }

    [JsonPropertyName("status")]
    public ProfileStatus? Status { get; set; }

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
    public RaidMode? RaidMode { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("shortId")]
    public string? ShortId { get; set; }

    [JsonPropertyName("additional_info")]
    public List<object>? AdditionalInfo { get; set; } // TODO: Was `any` in the node server.
}
