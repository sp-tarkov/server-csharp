using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class GetAllAttachmentsRequestData
{
    [JsonPropertyName("dialogId")]
    public string DialogId { get; set; }
}
