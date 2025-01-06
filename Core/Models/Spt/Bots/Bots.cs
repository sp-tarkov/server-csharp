using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Bots;

public class Bots
{
    public Dictionary<string, BotType> types { get; }
    [JsonPropertyName("base")]
    public BotBase Base { get; }
    public BotCode core { get; }
}