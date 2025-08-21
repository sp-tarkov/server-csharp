using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record ChooseRandomCompatibleModResult
{


    [JsonPropertyName("incompatible")]
    public bool? Incompatible { get; set; }

    [JsonPropertyName("found")]
    public bool? Found { get; set; }

    [JsonPropertyName("chosenTpl")]
    public MongoId? ChosenTemplate { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("slotBlocked")]
    public bool? SlotBlocked { get; set; }
}
