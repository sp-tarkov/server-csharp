using System.Text.Json.Serialization;

namespace Core.Models.Spt.Server;

public class LocaleBase
{
    [JsonPropertyName("global")]
    public Dictionary<string, Dictionary<string, string>> Global { get; set; }

    [JsonPropertyName("menu")]
    public Dictionary<string, string> Menu { get; set; }

    [JsonPropertyName("languages")]
    public Dictionary<string, string> Languages { get; set; }

    [JsonPropertyName("server")]
    public Dictionary<string, Dictionary<string, string>> Server { get; set; }
}