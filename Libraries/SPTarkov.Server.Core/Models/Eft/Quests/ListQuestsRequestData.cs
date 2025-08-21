using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record ListQuestsRequestData : IRequestData
{
    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }
}
