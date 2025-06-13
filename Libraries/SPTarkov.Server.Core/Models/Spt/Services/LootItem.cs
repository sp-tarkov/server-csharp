using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public record LootItem
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("tpl")]
    public string? Tpl
    {
        get;
        set;
    }

    [JsonPropertyName("isPreset")]
    public bool? IsPreset
    {
        get;
        set;
    }

    [JsonPropertyName("stackCount")]
    public int? StackCount
    {
        get;
        set;
    }
}
