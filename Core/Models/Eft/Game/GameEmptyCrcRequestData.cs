using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GameEmptyCrcRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }
}