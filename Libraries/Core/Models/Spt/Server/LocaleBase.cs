using System.Text.Json.Serialization;
using Core.Utils.Json;

namespace Core.Models.Spt.Server;

public record LocaleBase
{
    [JsonPropertyName("global")]
    public Dictionary<string, LazyLoad<Dictionary<string, string>>>? Global { get; set; }

    [JsonPropertyName("menu")]
    public Dictionary<string, Dictionary<string, object>>? Menu { get; set; }

    [JsonPropertyName("languages")]
    public Dictionary<string, string>? Languages { get; set; }
}
