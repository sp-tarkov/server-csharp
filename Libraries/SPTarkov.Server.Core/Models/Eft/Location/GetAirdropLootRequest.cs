using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Location;

public record GetAirdropLootRequest : IRequestData
{
    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; }
}
