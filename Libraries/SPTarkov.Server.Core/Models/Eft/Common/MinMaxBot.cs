using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MinMaxBot : MinMax<int>
{
    [JsonPropertyName("WildSpawnType")]
    public string? WildSpawnType
    {
        get;
        set;
    } // TODO: Could be WildSpawnType or string
}
