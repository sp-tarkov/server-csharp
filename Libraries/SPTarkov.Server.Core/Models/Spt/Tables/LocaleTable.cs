using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Models.Spt.Tables;

public record LocaleTable
{
    /// <summary>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE LOCALESERVICE INSTEAD
    /// THIS IS LAZY LOADED AND YOUR CHANGES WILL NOT BE SAVED
    /// </summary>
    [JsonPropertyName("global")]
    public required Dictionary<string, LazyLoad<GlobalLocaleDictionary>> Global
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);

            field = new Dictionary<string, LazyLoad<GlobalLocaleDictionary>>(value, StringComparer.OrdinalIgnoreCase);
        }
    }

    [JsonPropertyName("menu")]
    public required Dictionary<string, Dictionary<string, object>> Menu { get; init; }

    [JsonPropertyName("languages")]
    public required Dictionary<string, string> Languages { get; init; }
}

public sealed class GlobalLocaleDictionary : Dictionary<string, string>
{
    public GlobalLocaleDictionary()
        : base(StringComparer.OrdinalIgnoreCase) { }
}
