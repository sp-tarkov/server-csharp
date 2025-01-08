using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class ClearMailMessageRequest
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}