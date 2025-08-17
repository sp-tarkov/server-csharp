using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetMailDialogViewRequestData : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    [JsonPropertyName("dialogId")]
    public MongoId DialogId { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("time")]
    public decimal? Time { get; set; } // decimal
}
