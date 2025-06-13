using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SendSurveyOpinionRequest : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("resultJson")]
    public string? ResultJson
    {
        get;
        set;
    }

    [JsonPropertyName("surveyId")]
    public int? SurveyId
    {
        get;
        set;
    }

    [JsonPropertyName("answers")]
    public List<SurveyOpinionAnswer>? Answers
    {
        get;
        set;
    }
}

public record SurveyOpinionAnswer
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

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
