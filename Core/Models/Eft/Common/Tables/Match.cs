using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;


public class Match
{
    [JsonPropertyName("metrics")]
    public Metrics? Metrics { get; set; }
}

public class Metrics
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

    [JsonPropertyName("PauseReasons")]
    public List<int>? PauseReasons { get; set; }
}