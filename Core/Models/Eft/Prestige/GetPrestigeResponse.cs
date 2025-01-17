using System.Text.Json.Serialization;

namespace Core.Models.Eft.Prestige;

public record GetPrestigeResponse
{
    [JsonPropertyName("elements")]
    public List<Common.Tables.Prestige>? Elements { get; set; }
}
