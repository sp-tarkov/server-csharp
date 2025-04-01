using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace SPTarkov.Server.Core.Models.Spt.Hideout;

public record Hideout
{
    [JsonPropertyName("areas")]
    public List<HideoutArea>? Areas
    {
        get;
        set;
    }
    [JsonPropertyName("customAreas")]
    public List<HideoutArea>? CustomAreas
    {
        get;
        set;
    }

    [JsonPropertyName("customisation")]
    public HideoutCustomisation? Customisation
    {
        get;
        set;
    }

    [JsonPropertyName("production")]
    public HideoutProductionData? Production
    {
        get;
        set;
    }

    [JsonPropertyName("settings")]
    public HideoutSettingsBase? Settings
    {
        get;
        set;
    }

    [JsonPropertyName("qte")]
    public List<QteData>? Qte
    {
        get;
        set;
    }
}
