using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class SendSurveyOpinionRequest
{
    [JsonPropertyName("surveyId")]
    public int? SurveyId { get; set; }

    [JsonPropertyName("answers")]
    public List<SurveyOpinionAnswer>? Answers { get; set; }
}

public class SurveyOpinionAnswer
{
    [JsonPropertyName("questionId")]
    public int? QuestionId { get; set; }

    [JsonPropertyName("answerType")]
    public string? AnswerType { get; set; }

    [JsonPropertyName("answers")]
    public object? Answers { get; set; }
}