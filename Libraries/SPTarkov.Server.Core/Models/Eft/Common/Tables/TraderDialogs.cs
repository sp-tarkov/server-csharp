using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TraderDialogs
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("elements")]
    public required List<TraderDialogElements> Elements;
}

public record TraderDialogElements
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("CanBeFirstDialogue")]
    public bool CanBeFirstDialog { get; set; } = true;

    [JsonPropertyName("Id")]
    public required MongoId Id { get; set; }

    [JsonPropertyName("MainVariable")]
    public required MongoId MainVariable { get; set; }

    [JsonPropertyName("Trader")]
    public required MongoId MainTrader { get; set; }

    [JsonPropertyName("SubTraders")]
    public required List<MongoId> SubTraders { get; set; }

    //Todo: Still needs fixing up
    [JsonPropertyName("Lines")]
    public required List<object> Lines { get; set; }

    [JsonPropertyName("StartPoints")]
    public required Dictionary<MongoId, int> StartPoints { get; set; }

    [JsonPropertyName("localization")]
    public required Dictionary<string, Dictionary<string, string>> LocalizationDictionary { get; set; }
}
