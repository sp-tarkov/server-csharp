using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class GetMailDialogInfoRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }
}