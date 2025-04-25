using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record GenerationData
{
    /// <summary>
    ///     key: number of items, value: weighting
    /// </summary>
    [JsonPropertyName("weights")]
    public Dictionary<double, double>? Weights
    {
        get;
        set;
    }

    /// <summary>
    ///     Array of item tpls
    /// </summary>
    [JsonPropertyName("whitelist")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Whitelist
    {
        get;
        set;
    }
}
