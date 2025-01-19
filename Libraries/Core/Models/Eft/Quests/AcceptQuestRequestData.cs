using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public record AcceptQuestRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "QuestAccept";

    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
