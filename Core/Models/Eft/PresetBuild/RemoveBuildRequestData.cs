using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.PresetBuild;

public class RemoveBuildRequestData : IRequestData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
