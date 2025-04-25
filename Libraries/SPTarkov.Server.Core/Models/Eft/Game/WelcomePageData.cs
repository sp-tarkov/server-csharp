using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record WelcomePageData
{
    [JsonPropertyName("titleLocaleKey")]
    public string? TitleLocaleKey
    {
        get;
        set;
    }

    [JsonPropertyName("timeLocaleKey")]
    public string? TimeLocaleKey
    {
        get;
        set;
    }

    [JsonPropertyName("descriptionLocaleKey")]
    public string? DescriptionLocaleKey
    {
        get;
        set;
    }
}
