using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record FriendRequestSendResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("status")]
    public BackendErrorCodes? Status
    {
        get;
        set;
    }

    [JsonPropertyName("requestId")]
    public string? RequestId
    {
        get;
        set;
    }

    [JsonPropertyName("retryAfter")]
    public int? RetryAfter
    {
        get;
        set;
    }
}
