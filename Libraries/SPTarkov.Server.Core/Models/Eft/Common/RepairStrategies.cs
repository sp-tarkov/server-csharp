using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RepairStrategies
{
    [JsonPropertyName("Armor")]
    public RepairStrategy? Armor
    {
        get;
        set;
    }

    [JsonPropertyName("Firearms")]
    public RepairStrategy? Firearms
    {
        get;
        set;
    }
}
