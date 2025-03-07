using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CustomizationItem
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("_name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("_parent")]
    public string? Parent
    {
        get;
        set;
    }

    [JsonPropertyName("_type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("_props")]
    public CustomizationProps? Properties
    {
        get;
        set;
    }

    [JsonPropertyName("_proto")]
    public string? Proto
    {
        get;
        set;
    }
}

public class CustomizationProps
{
    [JsonPropertyName("Prefab")]
    public object? Prefab
    {
        get;
        set;
    } // Prefab object or string

    [JsonPropertyName("WatchPrefab")]
    public Prefab? WatchPrefab
    {
        get;
        set;
    }

    [JsonPropertyName("WatchRotation")]
    public XYZ? WatchRotation
    {
        get;
        set;
    }

    [JsonPropertyName("WatchPosition")]
    public XYZ? WatchPosition
    {
        get;
        set;
    }

    [JsonPropertyName("IntegratedArmorVest")]
    public bool? IntegratedArmorVest
    {
        get;
        set;
    }

    [JsonPropertyName("MannequinPoseName")]
    public string? MannequinPoseName
    {
        get;
        set;
    }

    [JsonPropertyName("BodyPart")]
    public string? BodyPart
    {
        get;
        set;
    }

    [JsonPropertyName("Game")]
    public List<string>? Game
    {
        get;
        set;
    }

    [JsonPropertyName("Hands")]
    public string? Hands
    {
        get;
        set;
    }

    [JsonPropertyName("Feet")]
    public string? Feet
    {
        get;
        set;
    }

    [JsonPropertyName("Body")]
    public string? Body
    {
        get;
        set;
    }

    [JsonPropertyName("ProfileVersions")]
    public List<string>? ProfileVersions
    {
        get;
        set;
    }

    [JsonPropertyName("Side")]
    public List<string>? Side
    {
        get;
        set;
    }

    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("ShortName")]
    public string? ShortName
    {
        get;
        set;
    }

    [JsonPropertyName("Description")]
    public string? Description
    {
        get;
        set;
    }

    [JsonPropertyName("AvailableAsDefault")]
    public bool? AvailableAsDefault
    {
        get;
        set;
    }

    [JsonPropertyName("EnvironmentUIType")]
    public string? EnvironmentUIType
    {
        get;
        set;
    }

    [JsonPropertyName("Interaction")]
    public string? Interaction
    {
        get;
        set;
    }

    [JsonPropertyName("UsecTemplateId")]
    public string? UsecTemplateId
    {
        get;
        set;
    }

    [JsonPropertyName("BearTemplateId")]
    public string? BearTemplateId
    {
        get;
        set;
    }

    [JsonPropertyName("AssetPath")]
    public Prefab? AssetPath
    {
        get;
        set;
    }

    [JsonPropertyName("HideGarbage")]
    public bool? HideGarbage
    {
        get;
        set;
    }
}
