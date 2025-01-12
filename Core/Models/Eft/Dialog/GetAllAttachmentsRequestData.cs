using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class GetAllAttachmentsRequestData : IRequestData
{
    [JsonPropertyName("dialogId")]
    public string DialogId { get; set; }
}
