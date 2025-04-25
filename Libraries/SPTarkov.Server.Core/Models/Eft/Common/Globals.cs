using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Globals
{
    [JsonPropertyName("config")]
    public Config? Configuration
    {
        get;
        set;
    }

    [JsonPropertyName("LocationInfection")]
    public LocationInfection? LocationInfection
    {
        get;
        set;
    }

    [JsonPropertyName("bot_presets")]
    public List<BotPreset>? BotPresets
    {
        get;
        set;
    }

    [JsonPropertyName("BotWeaponScatterings")]
    public List<BotWeaponScattering>? BotWeaponScatterings
    {
        get;
        set;
    }

    [JsonPropertyName("ItemPresets")]
    public Dictionary<string, Preset>? ItemPresets
    {
        get;
        set;
    }
}
