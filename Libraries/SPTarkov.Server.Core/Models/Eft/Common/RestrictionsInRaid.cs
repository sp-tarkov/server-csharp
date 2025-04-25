using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RestrictionsInRaid
{
    [JsonPropertyName("MaxInLobby")]
    public double? MaxInLobby
    {
        get;
        set;
    }

    [JsonPropertyName("MaxInRaid")]
    public double? MaxInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("TemplateId")]
    public string? TemplateId
    {
        get;
        set;
    }
}
