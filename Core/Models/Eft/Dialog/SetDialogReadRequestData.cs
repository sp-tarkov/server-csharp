using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class SetDialogReadRequestData
{
    [JsonPropertyName("dialogId")]
    public List<string>? DialogId { get; set; }
}