using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WildHead
{
    [JsonPropertyName("head")]
    public string? Head
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

    [JsonPropertyName("NotRandom")]
    public bool? NotRandom
    {
        get;
        set;
    }
}
