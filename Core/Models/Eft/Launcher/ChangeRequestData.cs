using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public class ChangeRequestData : LoginRequestData
{
    [JsonPropertyName("change")]
    public string Change { get; set; }
}