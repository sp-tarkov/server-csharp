using System.Text.Json.Serialization;

namespace Core.Models.Eft.Notifier;

public class NotifierChannel
{
	[JsonPropertyName("server")]
	public string Server { get; set; }

	[JsonPropertyName("channel_id")]
	public string ChannelId { get; set; }

	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonPropertyName("notifierServer")]
	public string NotifierServer { get; set; }

	[JsonPropertyName("ws")]
	public string WebSocket { get; set; }
}