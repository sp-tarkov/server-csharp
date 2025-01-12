using System.Text.Json.Serialization;
using Core.Models.Eft.Profile;
using Core.Models.Utils;

namespace Core.Models.Eft.Builds;

public class SetMagazineRequest : IRequestData
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Caliber")]
    public string? Caliber { get; set; }

    [JsonPropertyName("Items")]
    public List<MagazineTemplateAmmoItem>? Items { get; set; }

    [JsonPropertyName("TopCount")]
    public int? TopCount { get; set; }

    [JsonPropertyName("BottomCount")]
    public int? BottomCount { get; set; }
}
