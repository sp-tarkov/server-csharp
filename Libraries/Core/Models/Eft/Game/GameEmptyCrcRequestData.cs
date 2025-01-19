using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Game;

public record GameEmptyCrcRequestData : IRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }
}
