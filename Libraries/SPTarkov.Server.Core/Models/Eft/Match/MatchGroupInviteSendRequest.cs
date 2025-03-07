using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupInviteSendRequest : IRequestData
{
    [JsonPropertyName("to")]
    public string? To
    {
        get;
        set;
    }

    [JsonPropertyName("inLobby")]
    public bool? InLobby
    {
        get;
        set;
    }
}
