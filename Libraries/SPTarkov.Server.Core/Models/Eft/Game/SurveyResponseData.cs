using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SurveyResponseData
{
    [JsonPropertyName("locale")]
    public Dictionary<string, Dictionary<string, string>>? Locale
    {
        get;
        set;
    }

    [JsonPropertyName("survey")]
    public Survey? Survey
    {
        get;
        set;
    }
}
