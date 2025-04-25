using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Transit
{
    [JsonPropertyName("activateAfterSec")]
    public int? ActivateAfterSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("active")]
    public bool? IsActive
    {
        get;
        set;
    }

    [JsonPropertyName("events")]
    public bool? Events
    {
        get;
        set;
    }

    [JsonPropertyName("hideIfNoKey")]
    public bool? HideIfNoKey
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public string? Conditions
    {
        get;
        set;
    }

    [JsonPropertyName("description")]
    public string? Description
    {
        get;
        set;
    }

    [JsonPropertyName("id")]
    public int? Id
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public string? Location
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

    [JsonPropertyName("time")]
    public long? Time
    {
        get;
        set;
    }
}
