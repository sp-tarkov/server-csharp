using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Bot;

public record GenerateBotsRequestData : IRequestData
{
    [JsonPropertyName("conditions")]
    public List<GenerateCondition>? Conditions
    {
        get;
        set;
    }
}
