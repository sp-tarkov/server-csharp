using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetClientDialogueRequestData : IRequestData
{
    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }
}
