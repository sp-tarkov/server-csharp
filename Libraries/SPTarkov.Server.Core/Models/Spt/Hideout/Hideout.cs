using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace SPTarkov.Server.Core.Models.Spt.Hideout;

public record Hideout
{


    [JsonPropertyName("areas")]
    public required List<HideoutArea> Areas { get; init; }

    [JsonPropertyName("customAreas")]
    public required List<HideoutArea>? CustomAreas { get; init; }

    [JsonPropertyName("customisation")]
    public required HideoutCustomisation Customisation { get; init; }

    [JsonPropertyName("production")]
    public required HideoutProductionData Production { get; init; }

    [JsonPropertyName("settings")]
    public required HideoutSettingsBase Settings { get; init; }

    [JsonPropertyName("qte")]
    public required List<QteData> Qte { get; init; }
}
