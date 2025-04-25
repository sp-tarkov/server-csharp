using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record StackSlot
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

    [JsonPropertyName("_max_count")]
    public double? MaxCount
    {
        get;
        set;
    }

    [JsonPropertyName("_props")]
    public StackSlotProps? Props
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

    [JsonPropertyName("upd")]
    public object? Upd
    {
        get;
        set;
    } // TODO: object here
}
