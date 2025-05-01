using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record LocaleConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-locale";

    /// <summary>
    ///     e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting
    /// </summary>
    [JsonPropertyName("gameLocale")]
    public string GameLocale
    {
        get;
        set;
    }

    /// <summary>
    ///     e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting
    /// </summary>
    [JsonPropertyName("serverLocale")]
    public string ServerLocale
    {
        get;
        set;
    }

    /// <summary>
    ///     Languages server can be translated into
    /// </summary>
    [JsonPropertyName("serverSupportedLocales")]
    public List<string> ServerSupportedLocales
    {
        get;
        set;
    }

    [JsonPropertyName("fallbacks")]
    public Dictionary<string, string> Fallbacks
    {
        get;
        set;
    }
}
