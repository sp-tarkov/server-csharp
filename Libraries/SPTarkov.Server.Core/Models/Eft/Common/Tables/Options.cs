using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Options
{
    [JsonPropertyName("Completion")]
    public CompletionFilter? Completion
    {
        get;
        set;
    }
}
