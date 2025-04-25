using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WildFeet
{
    [JsonPropertyName("feet")]
    public string? Feet
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
