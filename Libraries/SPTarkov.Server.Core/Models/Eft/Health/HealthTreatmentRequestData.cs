using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Health;

public record HealthTreatmentRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("trader")]
    public string? Trader
    {
        get;
        set;
    }

    /// <summary>
    ///     Id of stack to take money from <br />
    ///     Amount of money to take off player for treatment
    /// </summary>
    [JsonPropertyName("items")]
    public List<IdWithCount>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("difference")]
    public Difference? Difference
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get;
        set;
    }
}

public record Difference
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("BodyParts")]
    public BodyParts? BodyParts
    {
        get;
        set;
    }

    [JsonPropertyName("Energy")]
    public double? Energy
    {
        get;
        set;
    }

    [JsonPropertyName("Hydration")]
    public double? Hydration
    {
        get;
        set;
    }
}

public record BodyParts
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Head")]
    public BodyPartEffects? Head
    {
        get;
        set;
    }

    [JsonPropertyName("Chest")]
    public BodyPartEffects? Chest
    {
        get;
        set;
    }

    [JsonPropertyName("Stomach")]
    public BodyPartEffects? Stomach
    {
        get;
        set;
    }

    [JsonPropertyName("LeftArm")]
    public BodyPartEffects? LeftArm
    {
        get;
        set;
    }

    [JsonPropertyName("RightArm")]
    public BodyPartEffects? RightArm
    {
        get;
        set;
    }

    [JsonPropertyName("LeftLeg")]
    public BodyPartEffects? LeftLeg
    {
        get;
        set;
    }

    [JsonPropertyName("RightLeg")]
    public BodyPartEffects? RightLeg
    {
        get;
        set;
    }
}

public record BodyPartEffects
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public double Health
    {
        get;
        set;
    }

    /// <summary>
    ///     Effects in array to be removed
    /// </summary>
    public List<string> Effects
    {
        get;
        set;
    }
}
