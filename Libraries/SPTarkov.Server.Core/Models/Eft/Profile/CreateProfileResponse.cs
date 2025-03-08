using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record CreateProfileResponse
{
    [JsonPropertyName("uid")]
    public string? UserId
    {
        get;
        set;
    }
}
