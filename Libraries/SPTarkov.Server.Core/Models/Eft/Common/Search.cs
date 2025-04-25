using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Search
{
    [JsonPropertyName("SearchAction")]
    public double? SearchAction
    {
        get;
        set;
    }

    [JsonPropertyName("FindAction")]
    public double? FindAction
    {
        get;
        set;
    }
}
