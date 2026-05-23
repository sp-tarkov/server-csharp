using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public sealed record PrepatchResultEntry
{
    [JsonPropertyName("modGuid")]
    public required string ModGuid { get; init; }

    [JsonPropertyName("succeeded")]
    public required bool Succeeded { get; init; }
}
