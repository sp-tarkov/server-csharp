using Core.Models.Eft.Common.Tables;
using System.Text.Json.Serialization;
using Core.Models.Eft.Profile;

namespace Core.Models.Spt.Templates;

public class Templates
{
    [JsonPropertyName("character")]
    public List<string>? Character { get; set; }

    [JsonPropertyName("customisationStorage")]
    public List<CustomisationStorage>? CustomisationStorage { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, TemplateItem>? Items { get; set; }

    [JsonPropertyName("prestige")]
    public Prestige? Prestige { get; set; }

    [JsonPropertyName("quests")]
    public Dictionary<string, Quest>? Quests { get; set; }

    [JsonPropertyName("repeatableQuests")]
    public RepeatableQuestDatabase? RepeatableQuests { get; set; }

    [JsonPropertyName("handbook")]
    public HandbookBase? Handbook { get; set; }

    [JsonPropertyName("customization")]
    public Dictionary<string, CustomizationItem>? Customization { get; set; }

    /** The profile templates listed in the launcher on profile creation, split by account type (e.g. Standard) then side (e.g. bear/usec) */
    [JsonPropertyName("profiles")]
    public ProfileTemplates? Profiles { get; set; }

    /** Flea prices of items - gathered from online flea market dump */
    [JsonPropertyName("prices")]
    public Dictionary<string, double>? Prices { get; set; }

    /** Default equipment loadouts that show on main inventory screen */
    [JsonPropertyName("defaultEquipmentPresets")]
    public List<DefaultEquipmentPreset>? DefaultEquipmentPresets { get; set; }

    /** Achievements */
    [JsonPropertyName("achievements")]
    public List<Achievement>? Achievements { get; set; }

    /** Location services data */
    [JsonPropertyName("locationServices")]
    public LocationServices? LocationServices { get; set; }
}
