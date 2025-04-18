using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record LocaleBase
{
    [JsonPropertyName("global")]
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE LOCALESERVICE INSTEAD
    /// THIS IS LAZY LOADED AND YOUR CHANGES WILL NOT BE SAVED
    public Dictionary<string, LazyLoad<Dictionary<string, string>>>? Global
    {
        get;
        set;
    }

    [JsonPropertyName("menu")]
    public Dictionary<string, Dictionary<string, object>>? Menu
    {
        get;
        set;
    }

    [JsonPropertyName("languages")]
    public Dictionary<string, string>? Languages
    {
        get;
        set;
    }
}
