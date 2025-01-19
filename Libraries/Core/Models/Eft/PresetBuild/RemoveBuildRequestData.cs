using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.PresetBuild;

public record RemoveBuildRequestData : IRequestData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
