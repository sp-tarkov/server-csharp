using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public record FriendRequestData : IRequestData
{
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("retryAfter")]
    public int? RetryAfter { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }
}
