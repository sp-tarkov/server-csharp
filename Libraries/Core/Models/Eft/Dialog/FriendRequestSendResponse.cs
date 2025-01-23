using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Dialog;

public record FriendRequestSendResponse
{
    [JsonPropertyName("status")]
    public BackendErrorCodes? Status { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("retryAfter")]
    public int? RetryAfter { get; set; }
}
