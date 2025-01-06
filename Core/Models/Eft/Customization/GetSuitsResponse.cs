using System.Text.Json.Serialization;

namespace Core.Models.Eft.Customization;

public class GetSuitsResponse
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("suites")]
    public string[] Suites { get; set; }
}