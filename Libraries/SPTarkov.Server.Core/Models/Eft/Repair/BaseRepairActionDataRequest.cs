using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Repair;

public record BaseRepairActionDataRequest
{
    [JsonPropertyName("Action")]
    public string? Action
    {
        get;
        set;
    }
}
