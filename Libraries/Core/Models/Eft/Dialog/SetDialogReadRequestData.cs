using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public record SetDialogReadRequestData : IRequestData
{
    [JsonPropertyName("dialogs")]
    public List<string>? Dialogs { get; set; }
}
