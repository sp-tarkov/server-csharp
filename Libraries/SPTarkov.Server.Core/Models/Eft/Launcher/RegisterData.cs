using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Launcher;

public record RegisterData : LoginRequestData
{
    [JsonPropertyName("edition")]
    public string? Edition
    {
        get;
        set;
    }
}
