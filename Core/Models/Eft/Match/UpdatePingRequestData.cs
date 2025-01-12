using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public class UpdatePingRequestData : IRequestData
{
    [JsonPropertyName("servers")]
    public List<object>? servers { get; set; }
}
