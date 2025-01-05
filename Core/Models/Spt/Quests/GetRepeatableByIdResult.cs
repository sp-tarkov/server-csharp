using System.Text.Json.Serialization;

namespace Types.Models.Spt.Quests;

public class GetRepeatableByIdResult
{
    [JsonPropertyName("quest")]
    public RepeatableQuest Quest { get; set; }
    
    [JsonPropertyName("repeatableType")]
    public PmcDataRepeatableQuest RepeatableType { get; set; }
}