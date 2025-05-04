using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record CreateGroupMailRequest : IRequestData
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Users")]
    public List<string>? Users { get; set; }
}
