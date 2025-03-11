using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Spt.Templates;

public record Templates
{
    [JsonPropertyName("character")]
    public List<MongoId>? Character
    {
        get;
        set;
    }

    [JsonPropertyName("customisationStorage")]
    public List<CustomisationStorage>? CustomisationStorage
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public Dictionary<MongoId, TemplateItem>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("prestige")]
    public Prestige? Prestige
    {
        get;
        set;
    }

    [JsonPropertyName("quests")]
    public Dictionary<MongoId, Quest>? Quests
    {
        get;
        set;
    }

    [JsonPropertyName("repeatableQuests")]
    public RepeatableQuestDatabase? RepeatableQuests
    {
        get;
        set;
    }

    [JsonPropertyName("handbook")]
    public HandbookBase? Handbook
    {
        get;
        set;
    }

    [JsonPropertyName("customization")]
    public Dictionary<MongoId, CustomizationItem>? Customization
    {
        get;
        set;
    }

    /// <summary>
    /// The profile templates listed in the launcher on profile creation, split by account type (e.g. Standard) then side (e.g. bear/usec)
    /// </summary>
    [JsonPropertyName("profiles")]
    public ProfileTemplates? Profiles
    {
        get;
        set;
    }

    /// <summary>
    /// Flea prices of items - gathered from online flea market dump
    /// </summary>
    [JsonPropertyName("prices")]
    public Dictionary<MongoId, double>? Prices
    {
        get;
        set;
    }

    /// <summary>
    /// Default equipment loadouts that show on main inventory screen
    /// </summary>
    [JsonPropertyName("defaultEquipmentPresets")]
    public List<DefaultEquipmentPreset>? DefaultEquipmentPresets
    {
        get;
        set;
    }

    /// <summary>
    /// Achievements
    /// </summary>
    [JsonPropertyName("achievements")]
    public List<Achievement>? Achievements
    {
        get;
        set;
    }

    /// <summary>
    /// Location services data
    /// </summary>
    [JsonPropertyName("locationServices")]
    public LocationServices? LocationServices
    {
        get;
        set;
    }
}
