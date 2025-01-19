using System.Text.Json.Serialization;

namespace Core.Models.Spt.Services;

public record LootItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("isPreset")]
    public bool? IsPreset { get; set; }

    [JsonPropertyName("stackCount")]
    public int? StackCount { get; set; }
}
