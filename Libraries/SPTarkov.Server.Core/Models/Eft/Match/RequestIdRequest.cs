using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record RequestIdRequest : IRequestData
{
    [JsonPropertyName("requestId")]
    public string? RequestId
    {
        get;
        set;
    }
}
