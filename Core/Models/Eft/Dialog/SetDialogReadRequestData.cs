using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class SetDialogReadRequestData : IRequestData
{
    [JsonPropertyName("dialogId")]
    public List<string>? Dialogs { get; set; }
}
