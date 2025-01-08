using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Bots;

public class GenerateWeaponResult
{
    [JsonPropertyName("weapon")]
    public List<Item>? Weapon { get; set; }

    [JsonPropertyName("chosenAmmoTpl")]
    public string? ChosenAmmoTemplate { get; set; }

    [JsonPropertyName("chosenUbglAmmoTpl")]
    public string? ChosenUbglAmmoTemplate { get; set; }

    [JsonPropertyName("weaponMods")]
    public GlobalMods? WeaponMods { get; set; }

    [JsonPropertyName("weaponTemplate")]
    public TemplateItem? WeaponTemplate { get; set; }
}