using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public record RegisterData : LoginRequestData
{
    [JsonPropertyName("edition")]
    public string? Edition
    {
        get;
        set;
    }
}
