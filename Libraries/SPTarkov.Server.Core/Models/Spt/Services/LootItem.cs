using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public record LootItem
{
    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("tpl")]
    public MongoId? Tpl { get; set; }

    [JsonPropertyName("isPreset")]
    public bool? IsPreset { get; set; }

    [JsonPropertyName("stackCount")]
    public int? StackCount { get; set; }
}
