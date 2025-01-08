using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public class CompleteQuestRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    /** Quest Id */
    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }

    [JsonPropertyName("removeExcessItems")]
    public bool? RemoveExcessItems { get; set; }
}