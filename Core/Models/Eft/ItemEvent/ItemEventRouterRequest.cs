using System.Text.Json.Serialization;

namespace Core.Models.Eft.ItemEvent;

public class ItemEventRouterRequest
{
    [JsonPropertyName("data")]
    public List<Daum>? Data { get; set; }

    [JsonPropertyName("tm")]
    public int? Time { get; set; }

    [JsonPropertyName("reload")]
    public int? Reload { get; set; }
}

public class Daum
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("to")]
    public To? To { get; set; }
}

public class To
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

public class Location
{
    [JsonPropertyName("x")]
    public int? X { get; set; }

    [JsonPropertyName("y")]
    public int? Y { get; set; }

    [JsonPropertyName("r")]
    public string? R { get; set; }

    [JsonPropertyName("isSearched")]
    public bool? IsSearched { get; set; }
}