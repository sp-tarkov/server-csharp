using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class GetMailDialogViewRequestData : IRequestData
{
    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    [JsonPropertyName("dialogId")]
    public string? DialogId { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("time")]
    public decimal? Time { get; set; } // decimal
}
