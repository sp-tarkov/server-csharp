using System.Collections.Generic;
using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Spt.Repeatable;

public record QuestTypePool
{
    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }

    [JsonPropertyName("pool")]
    public QuestPool? Pool { get; set; }
}

public record QuestPool
{
    [JsonPropertyName("Exploration")]
    public ExplorationPool? Exploration { get; set; }

    [JsonPropertyName("Elimination")]
    public EliminationPool? Elimination { get; set; }

    [JsonPropertyName("Pickup")]
    public ExplorationPool? Pickup { get; set; }
}

public record ExplorationPool
{
    [JsonPropertyName("locations")]
    public Dictionary<ELocationName, List<string>>? Locations { get; set; } // TODO: check the type, originally - Partial<Record<ELocationName, string[]>>
}

public record EliminationPool
{
    [JsonPropertyName("targets")]
    public EliminationTargetPool? Targets { get; set; }
}

public record EliminationTargetPool
{
    [JsonPropertyName("Savage")]
    public TargetLocation? Savage { get; set; }

    [JsonPropertyName("AnyPmc")]
    public TargetLocation? AnyPmc { get; set; }

    [JsonPropertyName("bossBully")]
    public TargetLocation? BossBully { get; set; }

    [JsonPropertyName("bossGluhar")]
    public TargetLocation? BossGluhar { get; set; }

    [JsonPropertyName("bossKilla")]
    public TargetLocation? BossKilla { get; set; }

    [JsonPropertyName("bossSanitar")]
    public TargetLocation? BossSanitar { get; set; }

    [JsonPropertyName("bossTagilla")]
    public TargetLocation? BossTagilla { get; set; }

    [JsonPropertyName("bossKnight")]
    public TargetLocation? BossKnight { get; set; }

    [JsonPropertyName("bossZryachiy")]
    public TargetLocation? BossZryachiy { get; set; }

    [JsonPropertyName("bossBoar")]
    public TargetLocation? BossBoar { get; set; }

    [JsonPropertyName("bossBoarSniper")]
    public TargetLocation? BossBoarSniper { get; set; }
}

public record TargetLocation
{
    [JsonPropertyName("locations")]
    public List<string>? Locations { get; set; }
}
