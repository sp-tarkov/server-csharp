using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record VaultingMovesSettings
{
    [JsonPropertyName("VaultSettings")]
    public VaultingSubMoveSettings? VaultSettings
    {
        get;
        set;
    }

    [JsonPropertyName("ClimbSettings")]
    public VaultingSubMoveSettings? ClimbSettings
    {
        get;
        set;
    }
}
