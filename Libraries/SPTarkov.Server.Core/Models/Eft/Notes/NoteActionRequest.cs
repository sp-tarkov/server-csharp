using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Request;

namespace SPTarkov.Server.Core.Models.Eft.Notes;

public record NoteActionRequest : BaseInteractionRequestData
{
    [JsonPropertyName("index")]
    public int? Index
    {
        get;
        set;
    }

    [JsonPropertyName("note")]
    public Note? Note
    {
        get;
        set;
    }
}
