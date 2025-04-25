using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Item
{
    private string? _id;

    private string? _parentId;

    private string? _SlotId;

    private string? _tpl;

    // MongoId
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

    [JsonPropertyName("_tpl")]
    // MongoId
    public string? Template
    {
        get
        {
            return _tpl;
        }
        set
        {
            _tpl = string.Intern(value);
        }
    }

    [JsonPropertyName("parentId")]
    public string? ParentId
    {
        get
        {
            return _parentId;
        }
        set
        {
            _parentId = value == null ? null : string.Intern(value);
        }
    }

    [JsonPropertyName("slotId")]
    public string? SlotId
    {
        get
        {
            return _SlotId;
        }
        set
        {
            _SlotId = value == null ? null : string.Intern(value);
        }
    }

    [JsonPropertyName("location")]
    public object? Location
    {
        get;
        set;
    } // TODO: Can be IItemLocation or number

    [JsonPropertyName("desc")]
    public string? Desc
    {
        get;
        set;
    }

    [JsonPropertyName("upd")]
    public Upd? Upd
    {
        get;
        set;
    }
}
