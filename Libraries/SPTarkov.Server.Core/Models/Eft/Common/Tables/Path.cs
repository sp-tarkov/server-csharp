using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Path
{
    [JsonPropertyName("Source")]
    public string? Source
    {
        get;
        set;
    }

    [JsonPropertyName("Destination")]
    public string? Destination
    {
        get;
        set;
    }

    public bool? Event
    {
        get;
        set;
    }
}
