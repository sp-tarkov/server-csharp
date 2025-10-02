using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Launcher;

public record GetMiniProfileRequestData : IRequestData
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }
}
