using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record MapMarker
{
    [JsonPropertyName("Type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("X")]
    public double? X
    {
        get;
        set;
    }

    [JsonPropertyName("Y")]
    public double? Y
    {
        get;
        set;
    }

    [JsonPropertyName("Note")]
    public string? Note
    {
        get;
        set;
    }
}
