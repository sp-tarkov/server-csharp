using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public class FailQuestRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "QuestFail";

    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }

    [JsonPropertyName("removeExcessItems")]
    public bool? RemoveExcessItems { get; set; }
}