using System.Text.Json.Serialization;

namespace Core.Models.Spt.Mod;

public class ModOrder
{
    [JsonPropertyName("order")]
    public List<string> Order
    {
        get;
        set;
    }
}
