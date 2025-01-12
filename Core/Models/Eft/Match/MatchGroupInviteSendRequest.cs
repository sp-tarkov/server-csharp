using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public class MatchGroupInviteSendRequest : IRequestData
{
    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("inLobby")]
    public bool? InLobby { get; set; }
}
