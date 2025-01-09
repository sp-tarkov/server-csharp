using System.Text.Json.Serialization;

namespace Core.Models.Eft.Health;

public class SyncHealthRequestData
{
    [JsonPropertyName("Health")]
    public List<BodyPartHealth>? Health { get; set; }

    [JsonPropertyName("IsAlive")]
    public bool? IsAlive { get; set; }

    [JsonPropertyName("Hydration")]
    public double? Hydration { get; set; }

    [JsonPropertyName("Energy")]
    public double? Energy { get; set; }

    [JsonPropertyName("Temperature")]
    public double? Temperature { get; set; }
}

public class BodyPartCollection
{
    [JsonPropertyName("Head")]
    public BodyPartHealth? Head { get; set; }

    [JsonPropertyName("Chest")]
    public BodyPartHealth? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public BodyPartHealth? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public BodyPartHealth? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public BodyPartHealth? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public BodyPartHealth? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public BodyPartHealth? RightLeg { get; set; }
}

public class BodyPartHealth
{
    [JsonPropertyName("Maximum")]
    public int? Maximum { get; set; }

    [JsonPropertyName("Current")]
    public int? Current { get; set; }

    [JsonPropertyName("Effects")]
    public Dictionary<string, int>? Effects { get; set; }
}
