using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Preset
{
    [JsonPropertyName("_id")]
    public string? Id
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

    [JsonPropertyName("_changeWeaponName")]
    public bool? ChangeWeaponName
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

    [JsonPropertyName("_items")]
    public List<Item>? Items
    {
        get;
        set;
    }

    /// <summary>
    ///     Default presets have this property
    /// </summary>
    [JsonPropertyName("_encyclopedia")]
    public string? Encyclopedia
    {
        get;
        set;
    }
}
