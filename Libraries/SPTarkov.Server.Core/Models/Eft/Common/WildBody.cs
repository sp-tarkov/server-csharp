using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WildBody
{
    [JsonPropertyName("body")]
    public string? Body
    {
        get;
        set;
    }

    [JsonPropertyName("hands")]
    public string? Hands
    {
        get;
        set;
    }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom
    {
        get;
        set;
    }
}
