using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Launcher;

public record ChangeRequestData : LoginRequestData
{
    [JsonPropertyName("change")]
    public string? Change
    {
        get;
        set;
    }
}
