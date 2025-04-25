using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Alpinist
{
    [JsonPropertyName("Requirement")]
    public string? Requirement
    {
        get;
        set;
    }

    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Count")]
    public double? Count
    {
        get;
        set;
    }

    [JsonPropertyName("RequiredSlot")]
    public string? RequiredSlot
    {
        get;
        set;
    }

    [JsonPropertyName("RequirementTip")]
    public string? RequirementTip
    {
        get;
        set;
    }
}
