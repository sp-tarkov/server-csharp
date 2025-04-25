using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record Survey
{
    [JsonPropertyName("id")]
    public int? Id
    {
        get;
        set;
    }

    [JsonPropertyName("welcomePageData")]
    public WelcomePageData? WelcomePageData
    {
        get;
        set;
    }

    [JsonPropertyName("farewellPageData")]
    public FarewellPageData? FarewellPageData
    {
        get;
        set;
    }

    [JsonPropertyName("pages")]
    public List<List<int>>? Pages
    {
        get;
        set;
    }

    [JsonPropertyName("questions")]
    public List<SurveyQuestion>? Questions
    {
        get;
        set;
    }

    [JsonPropertyName("isNew")]
    public bool? IsNew
    {
        get;
        set;
    }
}
