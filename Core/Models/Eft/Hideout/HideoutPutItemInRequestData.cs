using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutPutItemInRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "HideoutPutItemsInAreaSlots";

    [JsonPropertyName("areaType")]
    public int AreaType { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, ItemDetails> Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public class ItemDetails
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}