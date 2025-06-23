using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Repeatable;

public record QuestTypePool
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("types")]
    public required List<string> Types { get; set; }

    [JsonPropertyName("pool")]
    public required QuestPool Pool { get; set; }
}

public record QuestPool
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Exploration")]
    public required ExplorationPool Exploration { get; set; }

    [JsonPropertyName("Elimination")]
    public required EliminationPool Elimination { get; set; }

    [JsonPropertyName("Pickup")]
    public required ExplorationPool Pickup { get; set; }
}

public record ExplorationPool
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("locations")]
    public Dictionary<ELocationName, List<string>>? Locations { get; set; } // TODO: check the type, originally - Partial<Record<ELocationName, string[]>>
}

public record EliminationPool
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("targets")]
    public Dictionary<string, TargetLocation>? Targets { get; set; }
}

public record TargetLocation
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("locations")]
    public List<string>? Locations { get; set; }
}
