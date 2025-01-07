using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupInviteSendRequest
{
    [JsonPropertyName("to")]
    public string To { get; set; }

    [JsonPropertyName("inLobby")]
    public bool InLobby { get; set; }
}