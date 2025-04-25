using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SurveyQuestion
{
    [JsonPropertyName("id")]
    public int? Id
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

    [JsonPropertyName("titleLocaleKey")]
    public string? TitleLocaleKey
    {
        get;
        set;
    }

    [JsonPropertyName("hintLocaleKey")]
    public string? HintLocaleKey
    {
        get;
        set;
    }

    [JsonPropertyName("answerLimit")]
    public int? AnswerLimit
    {
        get;
        set;
    }

    [JsonPropertyName("answerType")]
    public string? AnswerType
    {
        get;
        set;
    }

    [JsonPropertyName("answers")]
    public List<SurveyAnswer>? Answers
    {
        get;
        set;
    }
}
