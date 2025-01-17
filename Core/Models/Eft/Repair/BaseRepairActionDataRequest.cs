using System.Text.Json.Serialization;

namespace Core.Models.Eft.Repair;

public record BaseRepairActionDataRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }
}
