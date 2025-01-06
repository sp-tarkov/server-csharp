using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class ChangeGroupMailOwnerRequest
{
    [JsonPropertyName("dialogId")]
    public string DialogId { get; set; }

    [JsonPropertyName("uid")]
    public string Uid { get; set; }
}