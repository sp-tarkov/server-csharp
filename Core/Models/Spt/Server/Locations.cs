using System.Text.Json.Serialization;

namespace Core.Models.Spt.Server;

public class Locations
{
    [JsonPropertyName("bigmap")]
    public Location? Bigmap { get; set; }

    [JsonPropertyName("develop")]
    public Location? Develop { get; set; }

    [JsonPropertyName("factory4_day")]
    public Location? Factory4Day { get; set; }

    [JsonPropertyName("factory4_night")]
    public Location? Factory4Night { get; set; }

    [JsonPropertyName("hideout")]
    public Location? Hideout { get; set; }

    [JsonPropertyName("interchange")]
    public Location? Interchange { get; set; }

    [JsonPropertyName("laboratory")]
    public Location? Laboratory { get; set; }

    [JsonPropertyName("lighthouse")]
    public Location? Lighthouse { get; set; }

    [JsonPropertyName("privatearea")]
    public Location? PrivateArea { get; set; }

    [JsonPropertyName("rezervbase")]
    public Location? RezervBase { get; set; }

    [JsonPropertyName("shoreline")]
    public Location? Shoreline { get; set; }

    [JsonPropertyName("suburbs")]
    public Location? Suburbs { get; set; }

    [JsonPropertyName("tarkovstreets")]
    public Location? TarkovStreets { get; set; }

    [JsonPropertyName("terminal")]
    public Location? Terminal { get; set; }

    [JsonPropertyName("town")]
    public Location? Town { get; set; }

    [JsonPropertyName("woods")]
    public Location? Woods { get; set; }

    [JsonPropertyName("sandbox")]
    public Location? Sandbox { get; set; }

    [JsonPropertyName("sandbox_high")]
    public Location? SandboxHigh { get; set; }

    /** Holds a mapping of the linkages between locations on the UI */
    [JsonPropertyName("base")]
    public LocationsBase? Base { get; set; }
}