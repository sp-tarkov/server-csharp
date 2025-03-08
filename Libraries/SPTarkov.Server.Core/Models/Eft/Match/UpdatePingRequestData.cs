using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record UpdatePingRequestData : IRequestData
{
    [JsonPropertyName("servers")]
    public List<object>? servers
    {
        get;
        set;
    }
}
