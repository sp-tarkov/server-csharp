using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public class MatchGroupTransferRequest : IRequestData
{
    [JsonPropertyName("aidToChange")]
    public string? AidToChange { get; set; }
}
