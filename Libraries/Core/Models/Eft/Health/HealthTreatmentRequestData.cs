using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Health;

public record HealthTreatmentRequestData : BaseInteractionRequestData
{

    [JsonPropertyName("trader")]
    public string? Trader { get; set; }

    /** Id of stack to take money from */
    /** Amount of money to take off player for treatment */
    [JsonPropertyName("items")]
    public List<IdWithCount>? Items { get; set; }

    [JsonPropertyName("difference")]
    public Difference? Difference { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
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
    public BodyPartEffects? Head { get; set; }

    [JsonPropertyName("Chest")]
    public BodyPartEffects? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public BodyPartEffects? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public BodyPartEffects? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public BodyPartEffects? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public BodyPartEffects? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public BodyPartEffects? RightLeg { get; set; }
}

public record BodyPartEffects
{
    public double Health { get; set; }
    /** Effects in array to be removed */
    public List<string> Effects { get; set; }
}
