using System.Text.Json.Serialization;

namespace Core.Models.Eft.Quests;

public class ListQuestsRequestData
{
    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }
}