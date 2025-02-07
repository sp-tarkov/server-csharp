using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Hideout;

public record HandleQTEEventRequestData : InventoryBaseActionRequestData
{
    /**
     * true if QTE was successful, otherwise false
     */
    [JsonPropertyName("results")]
    public List<bool>? Results
    {
        get;
        set;
    }

    /**
     * Id of the QTE object used from db/hideout/qte.json
     */
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get;
        set;
    }
}
