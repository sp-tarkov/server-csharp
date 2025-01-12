using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class ChangeGroupMailOwnerRequest : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
}
