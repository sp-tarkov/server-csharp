using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class SendMessageRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }

    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("replyTo")]
    public string? ReplyTo { get; set; }
}
