using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record LocaleConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-locale";

    /** e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting */
    [JsonPropertyName("gameLocale")]
    public string GameLocale { get; set; }

    /** e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting */
    [JsonPropertyName("serverLocale")]
    public string ServerLocale { get; set; }

    /** Languages server can be translated into */
    [JsonPropertyName("serverSupportedLocales")]
    public List<string> ServerSupportedLocales { get; set; }

    [JsonPropertyName("fallbacks")]
    public Dictionary<string, string> Fallbacks { get; set; }
}
