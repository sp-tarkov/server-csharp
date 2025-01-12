using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class PinDialogRequestData : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}
