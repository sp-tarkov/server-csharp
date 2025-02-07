using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Launcher;

public record GetMiniProfileRequestData : IRequestData
{
    [JsonPropertyName("username")]
    public string? Username
    {
        get;
        set;
    }

    [JsonPropertyName("password")]
    public string? Password
    {
        get;
        set;
    }
}
