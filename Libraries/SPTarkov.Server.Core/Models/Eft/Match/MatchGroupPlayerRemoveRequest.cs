using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupPlayerRemoveRequest : IRequestData
{
    [JsonPropertyName("aidToKick")]
    public string? AidToKick
    {
        get;
        set;
    }
}
