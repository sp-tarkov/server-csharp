using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class ModOrder
{
    [JsonPropertyName("order")]
    public List<string> Order { get; set; }
}
