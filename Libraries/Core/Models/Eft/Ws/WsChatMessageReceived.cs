using System.Text.Json.Serialization;
using Core.Models.Eft.Match;
using Core.Models.Eft.Profile;

namespace Core.Models.Eft.Ws;

public record WsChatMessageReceived : WsNotificationEvent
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }

    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    [JsonPropertyName("profiles")]
    public List<GroupCharacter>? Profiles { get; set; }
}
