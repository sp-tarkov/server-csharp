using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class RemoveMailMessageRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}
