using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArtilleryShellingAirDropSettings
{
    [JsonPropertyName("UseAirDrop")]
    public bool? UseAirDrop
    {
        get;
        set;
    }

    [JsonPropertyName("AirDropTime")]
    public double? AirDropTime
    {
        get;
        set;
    }

    [JsonPropertyName("AirDropPosition")]
    public XYZ? AirDropPosition
    {
        get;
        set;
    }

    [JsonPropertyName("LootTemplateId")]
    public string? LootTemplateId
    {
        get;
        set;
    }
}
