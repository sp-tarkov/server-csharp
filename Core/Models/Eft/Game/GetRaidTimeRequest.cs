using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GetRaidTimeRequest
{
    [JsonPropertyName("Side")]
    public string Side { get; set; }

    [JsonPropertyName("Location")]
    public string Location { get; set; }
}