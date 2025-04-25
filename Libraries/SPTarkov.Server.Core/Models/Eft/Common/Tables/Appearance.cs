using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Appearance
{
    [JsonPropertyName("body")]
    public Dictionary<string, double>? Body
    {
        get;
        set;
    }

    [JsonPropertyName("feet")]
    public Dictionary<string, double>? Feet
    {
        get;
        set;
    }

    [JsonPropertyName("hands")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Hands
    {
        get;
        set;
    }

    [JsonPropertyName("head")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Head
    {
        get;
        set;
    }

    [JsonPropertyName("voice")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Voice
    {
        get;
        set;
    }
}
