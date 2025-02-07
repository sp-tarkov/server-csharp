using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Quests;

public record ListQuestsRequestData : IRequestData
{
    [JsonPropertyName("completed")]
    public bool? Completed
    {
        get;
        set;
    }
}
