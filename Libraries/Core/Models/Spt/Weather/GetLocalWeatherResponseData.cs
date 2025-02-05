using Core.Models.Enums;
using System.Text.Json.Serialization;

namespace Core.Models.Spt.Weather;

public record GetLocalWeatherResponseData
{
    [JsonPropertyName("season")]
    public Season? Season { get; set; }

    [JsonPropertyName("weather")]
    public List<Eft.Weather.Weather>? Weather { get; set; }
}
