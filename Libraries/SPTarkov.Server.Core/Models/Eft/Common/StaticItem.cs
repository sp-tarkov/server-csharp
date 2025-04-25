using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticItem
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("_tpl")]
    public string? Tpl
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
