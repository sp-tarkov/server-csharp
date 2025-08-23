using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record SaveDialogueStateRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("nodePathTraveled")]
    public List<NodePathTraveled> NodePathTraveled { get; set; }
}

public class NodePathTraveled
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; }

    [JsonPropertyName("dialogueId")]
    public string DialogueId { get; set; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }
}
