using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Ragfair;

public record TplWithFleaPrice
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("tpl")]
    public string? Tpl
    {
        get;
        set;
    }

    /// <summary>
    ///     Roubles
    /// </summary>
    [JsonPropertyName("price")]
    public double? Price
    {
        get;
        set;
    }
}
