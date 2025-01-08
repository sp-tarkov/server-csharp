using System.Text.Json.Serialization;

namespace Core.Models.Spt.Weather;

public class GetLocalWeatherResponseData
{
    [JsonPropertyName("season")]
    public int? Season { get; set; }

    [JsonPropertyName("weather")]
    public List<Eft.Weather.Weather>? Weather { get; set; }
}