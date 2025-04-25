using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SurveyAnswer
{
    [JsonPropertyName("id")]
    public int? Id
    {
        get;
        set;
    }

    [JsonPropertyName("questionId")]
    public int? QuestionId
    {
        get;
        set;
    }

    [JsonPropertyName("sortIndex")]
    public int? SortIndex
    {
        get;
        set;
    }

    [JsonPropertyName("localeKey")]
    public string? LocaleKey
    {
        get;
        set;
    }
}
