using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RepeatableTemplates
{
    [JsonPropertyName("Elimination")]
    public RepeatableQuest? Elimination
    {
        get;
        set;
    }

    [JsonPropertyName("Completion")]
    public RepeatableQuest? Completion
    {
        get;
        set;
    }

    [JsonPropertyName("Exploration")]
    public RepeatableQuest? Exploration
    {
        get;
        set;
    }

    [JsonPropertyName("Pickup")]
    public RepeatableQuest? Pickup
    {
        get;
        set;
    }
}
