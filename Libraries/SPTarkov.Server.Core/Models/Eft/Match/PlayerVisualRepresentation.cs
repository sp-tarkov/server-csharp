using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record PlayerVisualRepresentation
{
    [JsonPropertyName("Info")]
    public VisualInfo? Info
    {
        get;
        set;
    }

    [JsonPropertyName("Customization")]
    public Customization? Customization
    {
        get;
        set;
    }

    [JsonPropertyName("Equipment")]
    public Equipment? Equipment
    {
        get;
        set;
    }
}
