using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TemplateSide
{
    [JsonPropertyName("character")]
    public PmcData? Character
    {
        get;
        set;
    }

    [JsonPropertyName("suits")]
    public List<string>? Suits
    {
        get;
        set;
    }

    [JsonPropertyName("dialogues")]
    public Dictionary<string, Dialogue>? Dialogues
    {
        get;
        set;
    }

    [JsonPropertyName("userbuilds")]
    public UserBuilds? UserBuilds
    {
        get;
        set;
    }

    [JsonPropertyName("trader")]
    public ProfileTraderTemplate? Trader
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentBuilds")]
    public object? EquipmentBuilds
    {
        get;
        set;
    }

    [JsonPropertyName("weaponbuilds")]
    public object? WeaponBuilds
    {
        get;
        set;
    }
}
