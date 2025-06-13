using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Common;

public record IdWithCount
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     ID of stack to take money from
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    /// <summary>
    ///     Amount of money to take off player for treatment
    /// </summary>
    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }
}
