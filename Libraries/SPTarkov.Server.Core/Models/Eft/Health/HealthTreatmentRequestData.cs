using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Health;

public record HealthTreatmentRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("trader")]
    public string? Trader { get; set; }

    /// <summary>
    ///     Id of stack to take money from <br />
    ///     Amount of money to take off player for treatment
    /// </summary>
    [JsonPropertyName("items")]
    public List<IdWithCount>? Items { get; set; }

    [JsonPropertyName("difference")]
    public Difference? Difference { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public record Difference
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("BodyParts")]
    public Dictionary<string, BodyPartValues>? BodyParts { get; set; }

    [JsonPropertyName("Energy")]
    public double? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public double? Hydration { get; set; }
}

public record BodyPartValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    public double Health { get; set; }

    /// <summary>
    ///     Effects in array to be removed
    /// </summary>
    public List<string>? Effects { get; set; }
}
