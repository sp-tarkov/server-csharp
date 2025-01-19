using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.ItemEvent;

public record ItemEventRouterRequest : IRequestData
{
    [JsonPropertyName("data")]
    public List<Daum>? Data { get; set; }

    [JsonPropertyName("tm")]
    public int? Time { get; set; }

    [JsonPropertyName("reload")]
    public int? Reload { get; set; }
}

public record Daum
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("to")]
    public To? To { get; set; }
}

public record To
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("location")]
    public Location? Location { get; set; }
}

public record Location
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
