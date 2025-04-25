using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record VisibilityCondition
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("target")]
    public string? Target
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public int? Value
    {
        get;
        set;
    }

    [JsonPropertyName("dynamicLocale")]
    public bool? DynamicLocale
    {
        get;
        set;
    }

    [JsonPropertyName("oneSessionOnly")]
    public bool? OneSessionOnly
    {
        get;
        set;
    }

    [JsonPropertyName("conditionType")]
    public string? ConditionType
    {
        get;
        set;
    }
}
