using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Notifier;

public record NotifierChannel
{


    [JsonPropertyName("server")]
    public string? Server { get; set; }

    [JsonPropertyName("channel_id")]
    public MongoId? ChannelId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("notifierServer")]
    public string? NotifierServer { get; set; }

    [JsonPropertyName("ws")]
    public string? WebSocket { get; set; }
}
