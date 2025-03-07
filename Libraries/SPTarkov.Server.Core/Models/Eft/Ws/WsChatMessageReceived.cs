using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsChatMessageReceived : WsNotificationEvent
{
    [JsonPropertyName("dialogId")]
    public string? DialogId
    {
        get;
        set;
    }

    [JsonPropertyName("message")]
    public Message? Message
    {
        get;
        set;
    }

    [JsonPropertyName("profiles")]
    public List<GroupCharacter>? Profiles
    {
        get;
        set;
    }
}
