using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record VaultingSettings
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive
    {
        get;
        set;
    }

    [JsonPropertyName("VaultingInputTime")]
    public double? VaultingInputTime
    {
        get;
        set;
    }

    [JsonPropertyName("GridSettings")]
    public VaultingGridSettings? GridSettings
    {
        get;
        set;
    }

    [JsonPropertyName("MovesSettings")]
    public VaultingMovesSettings? MovesSettings
    {
        get;
        set;
    }
}
