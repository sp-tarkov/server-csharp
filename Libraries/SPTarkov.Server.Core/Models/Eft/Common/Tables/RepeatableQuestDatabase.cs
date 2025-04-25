using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RepeatableQuestDatabase
{
    [JsonPropertyName("templates")]
    public RepeatableTemplates? Templates
    {
        get;
        set;
    }

    [JsonPropertyName("rewards")]
    public RewardOptions? Rewards
    {
        get;
        set;
    }

    [JsonPropertyName("data")]
    public Options? Data
    {
        get;
        set;
    }

    [JsonPropertyName("samples")]
    public List<SampleQuests?>? Samples
    {
        get;
        set;
    }
}
