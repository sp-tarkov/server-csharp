using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public class RepeatableQuestChangeRequest
{
	[JsonPropertyName("Action")]
	public string Action { get; set; } = "RepeatableQuestChange";

	[JsonPropertyName("qid")]
	public string QuestId { get; set; }
}