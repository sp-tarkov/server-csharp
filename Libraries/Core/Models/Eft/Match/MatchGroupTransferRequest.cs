using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record MatchGroupTransferRequest : IRequestData
{
    [JsonPropertyName("aidToChange")]
    public string? AidToChange
    {
        get;
        set;
    }
}
