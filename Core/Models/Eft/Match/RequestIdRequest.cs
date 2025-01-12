using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public class RequestIdRequest : IRequestData
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }
}
