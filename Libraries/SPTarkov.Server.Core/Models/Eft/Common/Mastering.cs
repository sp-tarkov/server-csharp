using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Mastering
{
    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("Templates")]
    public List<string>? Templates
    {
        get;
        set;
    }

    [JsonPropertyName("Progress")]
    public double? Progress
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("Level2")]
    public int? Level2
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("Level3")]
    public int? Level3
    {
        get;
        set;
    }
}
