using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record LocaleConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-locale";

    /// <summary>
    ///     e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting
    /// </summary>
    [JsonPropertyName("gameLocale")]
    public required string GameLocale
    {
        get;
        set;
    }

    /// <summary>
    ///     e.g. ru/en/cn/fr etc, or 'system', will take computer locale setting
    /// </summary>
    [JsonPropertyName("serverLocale")]
    public required string ServerLocale
    {
        get;
        set;
    }

    /// <summary>
    ///     Languages server can be translated into
    /// </summary>
    [JsonPropertyName("serverSupportedLocales")]
    public required List<string> ServerSupportedLocales
    {
        get;
        set;
    }

    [JsonPropertyName("fallbacks")]
    public required Dictionary<string, string> Fallbacks
    {
        get;
        set;
    }
}
