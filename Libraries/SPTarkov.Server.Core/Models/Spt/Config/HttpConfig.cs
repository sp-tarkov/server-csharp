using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record HttpConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-http";

    /// <summary>
    ///     Address used by webserver
    /// </summary>
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

    /// <summary>
    ///     Address used by game client to connect to
    /// </summary>
    [JsonPropertyName("backendIp")]
    public required string BackendIp
    {
        get;
        set;
    }

    [JsonPropertyName("backendPort")]
    public required int BackendPort
    {
        get;
        set;
    }

    [JsonPropertyName("webSocketPingDelayMs")]
    public required int WebSocketPingDelayMs
    {
        get;
        set;
    }

    [JsonPropertyName("logRequests")]
    public required bool LogRequests
    {
        get;
        set;
    }

    /// <summary>
    ///     e.g. "SPT_Data/Server/images/traders/579dc571d53a0658a154fbec.png": "SPT_Data/Server/images/traders/NewTraderImage.png"
    /// </summary>
    [JsonPropertyName("serverImagePathOverride")]
    public required Dictionary<string, string> ServerImagePathOverride
    {
        get;
        set;
    }

    [JsonPropertyName("certificatePassword")]
    public required string CertificatePassword
    {
        get;
        set;
    }
}
