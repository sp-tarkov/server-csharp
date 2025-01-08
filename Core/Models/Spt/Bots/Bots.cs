using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Bots;

public class Bots
{
    [JsonPropertyName("types")]
    public Dictionary<string, BotType> Types { get; set; }

    [JsonPropertyName("base")]
    public BotBase Base { get; set; }

    [JsonPropertyName("core")]
    public Dictionary<string, object> Core { get; set; }
}