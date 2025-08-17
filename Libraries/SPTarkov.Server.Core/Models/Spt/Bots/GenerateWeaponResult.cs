using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record GenerateWeaponResult
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("weapon")]
    public List<Item>? Weapon { get; set; }

    [JsonPropertyName("chosenAmmoTpl")]
    public MongoId ChosenAmmoTemplate { get; set; }

    [JsonPropertyName("chosenUbglAmmoTpl")]
    public MongoId? ChosenUbglAmmoTemplate { get; set; }

    [JsonPropertyName("weaponMods")]
    public GlobalMods? WeaponMods { get; set; }

    [JsonPropertyName("weaponTemplate")]
    public TemplateItem? WeaponTemplate { get; set; }
}
