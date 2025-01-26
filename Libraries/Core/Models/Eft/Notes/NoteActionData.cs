using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Notes;

public record NoteActionData : BaseInteractionRequestData
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("note")]
    public Note? Note { get; set; }
}

public record Note
{
    [JsonPropertyName("Time")]
    public double? Time { get; set; }

    [JsonPropertyName("Text")]
    public string? Text { get; set; }
}
