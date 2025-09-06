using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record PinDialogRequestData : IRequestData
{
    [JsonPropertyName("dialogId")]
    public MongoId? DialogId { get; set; }
}
