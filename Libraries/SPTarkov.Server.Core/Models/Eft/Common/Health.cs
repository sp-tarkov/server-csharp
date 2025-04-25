using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Health
{
    [JsonPropertyName("Falling")]
    public Falling? Falling
    {
        get;
        set;
    }

    [JsonPropertyName("Effects")]
    public Effects? Effects
    {
        get;
        set;
    }

    [JsonPropertyName("HealPrice")]
    public HealPrice? HealPrice
    {
        get;
        set;
    }

    [JsonPropertyName("ProfileHealthSettings")]
    public ProfileHealthSettings? ProfileHealthSettings
    {
        get;
        set;
    }
}
