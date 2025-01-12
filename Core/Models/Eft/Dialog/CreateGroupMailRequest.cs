using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class CreateGroupMailRequest : IRequestData
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Users")]
    public List<string>? Users { get; set; }
}
