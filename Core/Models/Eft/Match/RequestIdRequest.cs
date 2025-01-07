using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class RequestIdRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }
}