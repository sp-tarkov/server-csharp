using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record Bots
{
    [JsonPropertyName("types")]
    public Dictionary<string, BotType?>? Types
    {
        get;
        set;
    }

    [JsonPropertyName("base")]
    public BotBase? Base
    {
        get;
        set;
    }

    [JsonPropertyName("core")]
    public Dictionary<string, object>? Core
    {
        get;
        set;
    }
}
