using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Location;

public record GetAirdropLootRequest : IRequestData
{
    [JsonPropertyName("containerId")]
    public string? ContainerId
    {
        get;
        set;
    }
}
