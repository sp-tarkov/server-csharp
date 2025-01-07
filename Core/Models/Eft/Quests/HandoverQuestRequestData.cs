using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public class HandoverQuestRequestData
{
	[JsonPropertyName("Action")]
	public string Action { get; set; } = "QuestHandover";

	[JsonPropertyName("qid")]
	public string QuestId { get; set; }

	[JsonPropertyName("conditionId")]
	public string ConditionId { get; set; }

	[JsonPropertyName("items")]
	public List<HandoverItem> Items { get; set; }
}

public class HandoverItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("count")]
	public int Count { get; set; }
}