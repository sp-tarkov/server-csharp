using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record RequestIdRequest : IRequestData
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }
}
