using System.Text.Json.Serialization;
using Core.Models.Eft.Hideout;

namespace Core.Models.Spt.Hideout;

public record Hideout
{
    [JsonPropertyName("areas")]
    public List<HideoutArea>? Areas
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
