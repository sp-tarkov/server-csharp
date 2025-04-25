using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Request;

public record OwnerInfo
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }
}
