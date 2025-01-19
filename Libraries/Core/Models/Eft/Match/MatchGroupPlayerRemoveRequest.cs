using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record MatchGroupPlayerRemoveRequest : IRequestData
{
    [JsonPropertyName("aidToKick")]
    public string? AidToKick { get; set; }
}
