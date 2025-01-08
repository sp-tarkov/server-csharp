using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class PinDialogRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}