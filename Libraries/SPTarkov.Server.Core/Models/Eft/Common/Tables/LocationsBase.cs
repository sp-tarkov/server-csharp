using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record LocationsBase
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("locations")]
    public Locations? Locations
    {
        get;
        set;
    }

    [JsonPropertyName("paths")]
    public List<Path>? Paths
    {
        get;
        set;
    }
}

public record Locations
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    // Add properties as necessary
}

public record Path
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Source")]
    public string? Source
    {
        get;
        set;
    }

    [JsonPropertyName("Destination")]
    public string? Destination
    {
        get;
        set;
    }

    public bool? Event
    {
        get;
        set;
    }
}
