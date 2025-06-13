using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record XY
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("x")]
    public double? X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public double? Y
    {
        get;
        set;
    }
}
