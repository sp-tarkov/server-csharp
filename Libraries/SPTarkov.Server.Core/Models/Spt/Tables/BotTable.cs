using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;

namespace SPTarkov.Server.Core.Models.Spt.Tables;

public record BotTable
{
    [JsonPropertyName("types")]
    public required Dictionary<string, BotType?> Types { get; init; }

    [JsonPropertyName("base")]
    public required BotBase Base { get; init; }

    [JsonPropertyName("core")]
    public required CoreBot Core { get; init; }
}
