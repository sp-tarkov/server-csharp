using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ServiceRequirements
{
    [JsonPropertyName("CompletedQuests")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<CompletedQuest>? CompletedQuests
    {
        get;
        set;
    }

    [JsonPropertyName("Standings")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, StandingRequirement>? Standings
    {
        get;
        set;
    }
}
