using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CustomisationStorage
{
    // Customisation.json/itemId
    [JsonPropertyName("id")]
    public string Id
    {
        get;
        set;
    }

    [JsonPropertyName("source")]
    public string? Source
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }
}
