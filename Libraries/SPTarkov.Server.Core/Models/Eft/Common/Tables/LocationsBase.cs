using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record LocationsBase
{
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
