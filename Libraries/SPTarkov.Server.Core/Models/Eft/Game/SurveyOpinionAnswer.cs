using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SurveyOpinionAnswer
{
    [JsonPropertyName("questionId")]
    public int? QuestionId
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
    public object? Answers
    {
        get;
        set;
    }
}
