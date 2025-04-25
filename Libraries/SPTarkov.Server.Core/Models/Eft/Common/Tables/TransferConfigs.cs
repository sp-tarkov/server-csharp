using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TransferConfigs
{
    [JsonPropertyName("stashConfig")]
    public StashPrestigeConfig? StashConfig
    {
        get;
        set;
    }

    [JsonPropertyName("skillConfig")]
    public PrestigeSkillConfig? SkillConfig
    {
        get;
        set;
    }

    [JsonPropertyName("masteringConfig")]
    public PrestigeMasteringConfig? MasteringConfig
    {
        get;
        set;
    }
}
