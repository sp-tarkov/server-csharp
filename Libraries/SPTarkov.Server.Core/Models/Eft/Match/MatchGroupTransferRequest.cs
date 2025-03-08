using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupTransferRequest : IRequestData
{
    [JsonPropertyName("aidToChange")]
    public string? AidToChange
    {
        get;
        set;
    }
}
