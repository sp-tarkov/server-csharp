using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupPlayerRemoveRequest
{
    [JsonPropertyName("aidToKick")]
    public string AidToKick { get; set; }
}