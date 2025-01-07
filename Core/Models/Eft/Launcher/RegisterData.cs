using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public class RegisterData : LoginRequestData
{
    [JsonPropertyName("edition")]
    public string Edition { get; set; }
}