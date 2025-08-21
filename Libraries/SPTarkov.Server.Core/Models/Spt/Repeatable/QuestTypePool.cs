using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Repeatable;

public record QuestTypePool
{


    [JsonPropertyName("types")]
    public required List<string> Types { get; set; }

    [JsonPropertyName("pool")]
    public required QuestPool Pool { get; set; }
}

public record QuestPool
{


    [JsonPropertyName("Exploration")]
    public required ExplorationPool Exploration { get; set; }

    [JsonPropertyName("Elimination")]
    public required EliminationPool Elimination { get; set; }

    [JsonPropertyName("Pickup")]
    public required ExplorationPool Pickup { get; set; }
}

public record ExplorationPool
{


    [JsonPropertyName("locations")]
    public Dictionary<ELocationName, List<string>>? Locations { get; set; } // TODO: check the type, originally - Partial<Record<ELocationName, string[]>>
}

public record EliminationPool
{


    [JsonPropertyName("targets")]
    public Dictionary<string, TargetLocation>? Targets { get; set; }
}

public record TargetLocation
{


    [JsonPropertyName("locations")]
    public List<string>? Locations { get; set; }
}
