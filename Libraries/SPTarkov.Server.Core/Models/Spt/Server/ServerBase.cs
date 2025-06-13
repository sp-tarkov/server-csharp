using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Server;

/// <summary>
/// Model for Assets/database/server.json
/// </summary>
public record ServerBase
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("ip")]
    public required string Ip
    {
        get;
        set;
    }

    [JsonPropertyName("port")]
    public required int Port
    {
        get;
        set;
    }
}
