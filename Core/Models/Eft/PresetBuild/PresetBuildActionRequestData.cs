using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;

namespace Core.Models.Eft.PresetBuild;

public record PresetBuildActionRequestData  : IRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    /** name of preset given by player */
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Root")]
    public string? Root { get; set; }

    [JsonPropertyName("Items")]
    public List<Item>? Items { get; set; }
}
