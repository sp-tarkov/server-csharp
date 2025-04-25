using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TemplateItem
{
    private string? _id;

    private string? _name;

    private string? _parent;

    private string _prototype;

    private string? _type;

    [JsonPropertyName("_id")]
    public string? Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = string.Intern(value);
        }
    }

    [JsonPropertyName("_name")]
    public string? Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = string.Intern(value);
        }
    }

    [JsonPropertyName("_parent")]
    public string? Parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = string.Intern(value);
        }
    }

    [JsonPropertyName("_type")]
    public string? Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = string.Intern(value);
        }
    }

    [JsonPropertyName("_props")]
    public Props? Properties
    {
        get;
        set;
    }

    [JsonPropertyName("_proto")]
    public string? Prototype
    {
        get
        {
            return _prototype;
        }
        set
        {
            _prototype = string.Intern(value);
        }
    }
}
