using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record SendMessageRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public MessageType? Type
    {
        get;
        set;
    }

    [JsonPropertyName("text")]
    public string? Text
    {
        get;
        set;
    }

    [JsonPropertyName("replyTo")]
    public string? ReplyTo
    {
        get;
        set;
    }
}
