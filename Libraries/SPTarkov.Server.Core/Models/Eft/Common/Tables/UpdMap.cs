using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdMap
{
    [JsonPropertyName("Markers")]
    public List<MapMarker>? Markers
    {
        get;
        set;
    }
}
