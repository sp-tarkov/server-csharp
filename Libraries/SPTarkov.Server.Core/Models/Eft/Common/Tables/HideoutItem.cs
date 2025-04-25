using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record HideoutItem
{
    /// <summary>
    ///     Hideout inventory id that was used by improvement action
    /// </summary>
    [JsonPropertyName("_id")]
    public string? _Id
    {
        get
        {
            return Id;
        }
        set
        {
            if (value == null)
            {
                return;
            }

            Id = value;
        }
    }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("_tpl")]
    public string? Template
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

    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }

    public Item ConvertToItem()
    {
        return new Item
        {
            Id = Id,
            Template = Template,
            Upd = Upd
        };
    }
}
