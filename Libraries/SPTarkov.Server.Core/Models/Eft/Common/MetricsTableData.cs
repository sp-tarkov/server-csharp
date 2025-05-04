using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MetricsTableData
{
    [JsonPropertyName("Keys")]
    public List<int>? Keys { get; set; }

    [JsonPropertyName("NetProcessingBins")]
    public List<int>? NetProcessingBins { get; set; }

    [JsonPropertyName("RenderBins")]
    public List<int>? RenderBins { get; set; }

    [JsonPropertyName("GameUpdateBins")]
    public List<int>? GameUpdateBins { get; set; }

    [JsonPropertyName("MemoryMeasureInterval")]
    public int? MemoryMeasureInterval { get; set; }
}
