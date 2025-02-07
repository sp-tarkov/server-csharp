using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public record ClearMailMessageRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId
    {
        get;
        set;
    }
}
