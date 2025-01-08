using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class LocationsBase
{
    [JsonPropertyName("locations")]
    public Locations? Locations { get; set; }

    [JsonPropertyName("paths")]
    public List<Path>? Paths { get; set; }
}

public class Locations
{
    // Add properties as necessary
}

public class Path
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Destination")]
    public string? Destination { get; set; }

    public bool? Event { get; set; }
}