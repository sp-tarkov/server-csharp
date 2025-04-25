using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Content
{
    [JsonPropertyName("ip")]
    public string? Ip
    {
        get;
        set;
    }

    [JsonPropertyName("port")]
    public double? Port
    {
        get;
        set;
    }

    [JsonPropertyName("root")]
    public string? Root
    {
        get;
        set;
    }
}
