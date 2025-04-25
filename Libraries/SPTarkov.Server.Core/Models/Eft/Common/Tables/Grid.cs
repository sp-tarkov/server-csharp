using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Grid
{
    [JsonPropertyName("_name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("_parent")]
    public string? Parent
    {
        get;
        set;
    }

    [JsonPropertyName("_props")]
    public GridProps? Props
    {
        get;
        set;
    }

    [JsonPropertyName("_proto")]
    public string? Proto
    {
        get;
        set;
    }
}
