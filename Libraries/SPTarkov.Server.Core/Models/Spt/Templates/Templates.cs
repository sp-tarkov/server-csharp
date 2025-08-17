using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Spt.Templates;

public record Templates
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("character")]
    public required List<string> Character { get; init; }

    [JsonPropertyName("customisationStorage")]
    public required List<CustomisationStorage> CustomisationStorage { get; init; }

    [JsonPropertyName("items")]
    public required Dictionary<MongoId, TemplateItem> Items { get; init; }

    [JsonPropertyName("prestige")]
    public required Prestige Prestige { get; init; }

    [JsonPropertyName("quests")]
    public required Dictionary<MongoId, Quest> Quests { get; init; }

    [JsonPropertyName("repeatableQuests")]
    public required RepeatableQuestDatabase RepeatableQuests { get; init; }

    [JsonPropertyName("handbook")]
    public required HandbookBase Handbook { get; init; }

    [JsonPropertyName("customization")]
    public required Dictionary<MongoId, CustomizationItem> Customization { get; init; }

    /// <summary>
    ///     The profile templates listed in the launcher on profile creation, split by account type (e.g. Standard) then side (e.g. bear/usec)
    /// </summary>
    [JsonPropertyName("profiles")]
    public required Dictionary<string, ProfileSides> Profiles { get; init; }

    /// <summary>
    ///     Flea prices of items - gathered from online flea market dump
    /// </summary>
    [JsonPropertyName("prices")]
    public required Dictionary<MongoId, double> Prices { get; init; }

    /// <summary>
    ///     Default equipment loadouts that show on main inventory screen
    /// </summary>
    [JsonPropertyName("defaultEquipmentPresets")]
    public required List<DefaultEquipmentPreset> DefaultEquipmentPresets { get; init; }

    /// <summary>
    ///     Achievements
    /// </summary>
    [JsonPropertyName("achievements")]
    public required List<Achievement> Achievements { get; init; }

    /// <summary>
    ///     Achievements
    /// </summary>
    [JsonPropertyName("customAchievements")]
    public required List<Achievement> CustomAchievements { get; init; }

    /// <summary>
    ///     Location services data
    /// </summary>
    [JsonPropertyName("locationServices")]
    public required LocationServices LocationServices { get; init; }
}
