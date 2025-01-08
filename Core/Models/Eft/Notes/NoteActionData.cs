using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Notes;

public class NoteActionData : BaseInteractionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("note")]
    public Note? Note { get; set; }
}

public class Note
{
    [JsonPropertyName("Time")]
    public int? Time { get; set; }

    [JsonPropertyName("Text")]
    public string? Text { get; set; }
}