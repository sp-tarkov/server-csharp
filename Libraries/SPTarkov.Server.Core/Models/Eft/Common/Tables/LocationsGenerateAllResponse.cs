using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record LocationsGenerateAllResponse
{
    [JsonPropertyName("locations")]
    public Dictionary<MongoId, LocationBase> Locations { get; set; }

    [JsonPropertyName("paths")]
    public List<Path>? Paths { get; set; }
}
