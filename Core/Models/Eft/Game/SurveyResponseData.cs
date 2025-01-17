using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record SurveyResponseData
{
    [JsonPropertyName("locale")]
    public Dictionary<string, Dictionary<string, string>>? Locale { get; set; }

    [JsonPropertyName("survey")]
    public Survey? Survey { get; set; }
}

public record Survey
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("welcomePageData")]
    public WelcomePageData? WelcomePageData { get; set; }

    [JsonPropertyName("farewellPageData")]
    public FarewellPageData? FarewellPageData { get; set; }

    [JsonPropertyName("pages")]
    public List<List<int>>? Pages { get; set; }

    [JsonPropertyName("questions")]
    public List<SurveyQuestion>? Questions { get; set; }

    [JsonPropertyName("isNew")]
    public bool? IsNew { get; set; }
}

public record WelcomePageData
{
    [JsonPropertyName("titleLocaleKey")]
    public string? TitleLocaleKey { get; set; }

    [JsonPropertyName("timeLocaleKey")]
    public string? TimeLocaleKey { get; set; }

    [JsonPropertyName("descriptionLocaleKey")]
    public string? DescriptionLocaleKey { get; set; }
}

public record FarewellPageData
{
    [JsonPropertyName("textLocaleKey")]
    public string? TextLocaleKey { get; set; }
}

public record SurveyQuestion
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("sortIndex")]
    public int? SortIndex { get; set; }

    [JsonPropertyName("titleLocaleKey")]
    public string? TitleLocaleKey { get; set; }

    [JsonPropertyName("hintLocaleKey")]
    public string? HintLocaleKey { get; set; }

    [JsonPropertyName("answerLimit")]
    public int? AnswerLimit { get; set; }

    [JsonPropertyName("answerType")]
    public string? AnswerType { get; set; }

    [JsonPropertyName("answers")]
    public List<SurveyAnswer>? Answers { get; set; }
}

public record SurveyAnswer
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("questionId")]
    public int? QuestionId { get; set; }

    [JsonPropertyName("sortIndex")]
    public int? SortIndex { get; set; }

    [JsonPropertyName("localeKey")]
    public string? LocaleKey { get; set; }
}
