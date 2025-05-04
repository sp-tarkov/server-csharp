using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameEmptyCrcRequestData : IRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }
}
