using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Notifier;

public record SelectProfileResponse
{
    [JsonPropertyName("status")]
    public string? Status
    {
        get;
        set;
    }
}
