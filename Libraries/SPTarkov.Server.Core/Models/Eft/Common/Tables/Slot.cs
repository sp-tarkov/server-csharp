using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Slot
{
    private string? _name;

    private string? _proto;

    [JsonPropertyName("_name")]
    public string? Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value == null ? null : string.Intern(value);
        }
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
    public SlotProps? Props
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

    [JsonPropertyName("_required")]
    public bool? Required
    {
        get;
        set;
    }

    [JsonPropertyName("_mergeSlotWithChildren")]
    public bool? MergeSlotWithChildren
    {
        get;
        set;
    }

    [JsonPropertyName("_proto")]
    public string? Proto
    {
        get
        {
            return _proto;
        }
        set
        {
            _proto = value == null ? null : string.Intern(value);
        }
    }
}
