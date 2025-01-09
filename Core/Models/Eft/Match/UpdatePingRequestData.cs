using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class UpdatePingRequestData
{
    [JsonPropertyName("servers")]
    public List<object>? servers { get; set; }
}
