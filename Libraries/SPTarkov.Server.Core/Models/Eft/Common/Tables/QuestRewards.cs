using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record QuestRewards
{
    [JsonPropertyName("AvailableForStart")]
    public List<Reward>? AvailableForStart
    {
        get;
        set;
    }

    [JsonPropertyName("AvailableForFinish")]
    public List<Reward>? AvailableForFinish
    {
        get;
        set;
    }

    [JsonPropertyName("Started")]
    public List<Reward>? Started
    {
        get;
        set;
    }

    [JsonPropertyName("Success")]
    public List<Reward>? Success
    {
        get;
        set;
    }

    [JsonPropertyName("Fail")]
    public List<Reward>? Fail
    {
        get;
        set;
    }

    [JsonPropertyName("FailRestartable")]
    public List<Reward>? FailRestartable
    {
        get;
        set;
    }

    [JsonPropertyName("Expired")]
    public List<Reward>? Expired
    {
        get;
        set;
    }
}
