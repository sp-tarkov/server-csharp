using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public record ChangeRequestData : LoginRequestData
{
    [JsonPropertyName("change")]
    public string? Change { get; set; }
}
