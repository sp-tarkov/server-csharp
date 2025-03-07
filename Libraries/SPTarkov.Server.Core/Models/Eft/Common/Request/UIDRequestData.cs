using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Common.Request;

public record UIDRequestData : IRequestData
{
    [JsonPropertyName("uid")]
    public string? Uid
    {
        get;
        set;
    }
}
