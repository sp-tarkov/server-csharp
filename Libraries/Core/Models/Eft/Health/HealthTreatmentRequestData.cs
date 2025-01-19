using System.Text.Json.Serialization;

namespace Core.Models.Eft.Health;

public record HealthTreatmentRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "RestoreHealth";

    [JsonPropertyName("trader")]
    public string? Trader { get; set; }

    [JsonPropertyName("items")]
    public List<ItemCost>? Items { get; set; }

    [JsonPropertyName("difference")]
    public Difference? Difference { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public record ItemCost
{
    /** Id of stack to take money from */
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /** Amount of money to take off player for treatment */
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

public record Difference
{
    [JsonPropertyName("BodyParts")]
    public BodyParts? BodyParts { get; set; }

    [JsonPropertyName("Energy")]
    public int? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public int? Hydration { get; set; }
}

public record BodyParts
{
    [JsonPropertyName("Head")]
    public BodyPart? Head { get; set; }

    [JsonPropertyName("Chest")]
    public BodyPart? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public BodyPart? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public BodyPart? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public BodyPart? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public BodyPart? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public BodyPart? RightLeg { get; set; }
}
