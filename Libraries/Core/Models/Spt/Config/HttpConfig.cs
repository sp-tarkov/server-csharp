using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record HttpConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-http";

    /// <summary>
    ///     Address used by webserver
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip
    {
        get;
        set;
    }

    [JsonPropertyName("port")]
    public int Port
    {
        get;
        set;
    }

    /// <summary>
    ///     Address used by game client to connect to
    /// </summary>
    [JsonPropertyName("backendIp")]
    public string BackendIp
    {
        get;
        set;
    }

    [JsonPropertyName("backendPort")]
    public int BackendPort
    {
        get;
        set;
    }

    [JsonPropertyName("webSocketPingDelayMs")]
    public int WebSocketPingDelayMs
    {
        get;
        set;
    }

    [JsonPropertyName("logRequests")]
    public bool LogRequests
    {
        get;
        set;
    }

    /// <summary>
    ///     e.g. "SPT_Data/Server/images/traders/579dc571d53a0658a154fbec.png": "SPT_Data/Server/images/traders/NewTraderImage.png"
    /// </summary>
    [JsonPropertyName("serverImagePathOverride")]
    public Dictionary<string, string> ServerImagePathOverride
    {
        get;
        set;
    }

    [JsonPropertyName("certificatePassword")]
    public string? CertificatePassword
    {
        get;
        set;
    }
}
