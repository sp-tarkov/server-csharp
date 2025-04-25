using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MaxItemCountInLocation
{
    [JsonPropertyName("TemplateId")]
    public string? TemplateId
    {
        get;
        set;
    }

    [JsonPropertyName("Value")]
    public int? Value
    {
        get;
        set;
    }
}
