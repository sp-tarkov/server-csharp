using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupTransferRequest
{
    [JsonPropertyName("aidToChange")]
    public string AidToChange { get; set; }
}