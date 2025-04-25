using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LooseLoot
{
    [JsonPropertyName("spawnpointCount")]
    public SpawnpointCount? SpawnpointCount
    {
        get;
        set;
    }

    [JsonPropertyName("spawnpointsForced")]
    public List<Spawnpoint>? SpawnpointsForced
    {
        get;
        set;
    }

    [JsonPropertyName("spawnpoints")]
    public List<Spawnpoint>? Spawnpoints
    {
        get;
        set;
    }
}
