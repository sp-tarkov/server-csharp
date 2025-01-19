using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public record FriendRequestSendResponse
{
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("retryAfter")]
    public int? RetryAfter { get; set; }
}
