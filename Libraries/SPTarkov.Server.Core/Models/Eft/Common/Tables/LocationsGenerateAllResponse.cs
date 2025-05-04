using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record LocationsGenerateAllResponse
{
    [JsonPropertyName("locations")]
    public Dictionary<string, LocationBase> Locations { get; set; }

    [JsonPropertyName("paths")]
    public List<Path>? Paths { get; set; }
}
