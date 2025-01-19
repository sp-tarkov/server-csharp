using System.Text.Json.Serialization;

namespace Core.Models.Eft.Repair;

public record TraderRepairActionDataRequest : BaseRepairActionDataRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "TraderRepair";

    [JsonPropertyName("tid")]
    public string? TId { get; set; }

    [JsonPropertyName("repairItems")]
    public List<RepairItem>? RepairItems { get; set; }
}

public record RepairItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
