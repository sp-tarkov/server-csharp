using System.Text.Json.Serialization;

namespace Core.Models.Eft.Location;

public class GetAirdropLootRequest
{
    [JsonPropertyName("containerId")]
    public string ContainerId { get; set; }
}