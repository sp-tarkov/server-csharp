using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class CreateGroupMailRequest
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Users")]
    public List<string>? Users { get; set; }
}