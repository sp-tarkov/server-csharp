using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Notes;

public record Note
{
    [JsonPropertyName("Time")]
    public double? Time
    {
        get;
        set;
    }

    [JsonPropertyName("Text")]
    public string? Text
    {
        get;
        set;
    }
}
