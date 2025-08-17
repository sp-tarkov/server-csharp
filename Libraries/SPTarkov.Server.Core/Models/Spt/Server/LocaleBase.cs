using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record LocaleBase
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    /// <summary>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE LOCALESERVICE INSTEAD
    /// THIS IS LAZY LOADED AND YOUR CHANGES WILL NOT BE SAVED
    /// </summary>
    [JsonPropertyName("global")]
    public required Dictionary<string, LazyLoad<Dictionary<string, string>>> Global { get; init; }

    [JsonPropertyName("menu")]
    public required Dictionary<string, Dictionary<string, object>> Menu { get; init; }

    [JsonPropertyName("languages")]
    public required Dictionary<string, string> Languages { get; init; }
}
