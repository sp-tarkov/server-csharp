using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CustomizationItem
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("_name")]
    public string Name { get; set; }

    [JsonPropertyName("_parent")]
    public string Parent { get; set; }

    [JsonPropertyName("_type")]
    public string Type { get; set; }

    [JsonPropertyName("_props")]
    public CustomizationProps Properties { get; set; }

    [JsonPropertyName("_proto")]
    public string Proto { get; set; }
}

public class CustomizationProps
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("Prefab")]
    public object? Prefab { get; set; } // Prefab object or string

    [JsonPropertyName("WatchPrefab")]
    public Prefab? WatchPrefab { get; set; }

    [JsonPropertyName("WatchRotation")]
    public XYZ? WatchRotation { get; set; }

    [JsonPropertyName("WatchPosition")]
    public XYZ? WatchPosition { get; set; }

    [JsonPropertyName("IntegratedArmorVest")]
    public bool? IntegratedArmorVest { get; set; }

    [JsonPropertyName("MannequinPoseName")]
    public string? MannequinPoseName { get; set; }

    [JsonPropertyName("BodyPart")]
    public string? BodyPart { get; set; }

    [JsonPropertyName("Game")]
    public List<string>? Game { get; set; }

    [JsonPropertyName("Hands")]
    public MongoId? Hands { get; set; }

    [JsonPropertyName("Feet")]
    public MongoId? Feet { get; set; }

    [JsonPropertyName("Body")]
    public MongoId? Body { get; set; }

    [JsonPropertyName("ProfileVersions")]
    public List<string> ProfileVersions { get; set; }

    [JsonPropertyName("Side")]
    public List<string> Side { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("ShortName")]
    public string ShortName { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }

    [JsonPropertyName("DisableForMannequin")]
    public bool DisableForMannequin { get; set; }

    [JsonPropertyName("IsNotRandom")]
    public bool? IsNotRandom { get; set; }

    [JsonPropertyName("AvailableAsDefault")]
    public bool AvailableAsDefault { get; set; }

    [JsonPropertyName("EnvironmentUIType")]
    public string? EnvironmentUIType { get; set; }

    [JsonPropertyName("Interaction")]
    public string? Interaction { get; set; }

    [JsonPropertyName("UsecTemplateId")]
    public MongoId? UsecTemplateId { get; set; }

    [JsonPropertyName("BearTemplateId")]
    public MongoId? BearTemplateId { get; set; }

    [JsonPropertyName("AssetPath")]
    public Prefab AssetPath { get; set; }

    [JsonPropertyName("HideGarbage")]
    public bool? HideGarbage { get; set; }
}
