using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class LocationsGenerateAllResponse
{
    [JsonPropertyName("locations")]
    public Locations Locations { get; set; }

    [JsonPropertyName("paths")]
    public List<Path> Paths { get; set; }
}