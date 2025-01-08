using System.Text.Json.Serialization;

namespace Core.Models.Eft.PresetBuild;

public class RemoveBuildRequestData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}