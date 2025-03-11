using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.PresetBuild;

public record PresetBuildActionRequestData : IRequestData
{
    [JsonPropertyName("Action")]
    public string? Action
    {
        get;
        set;
    }

    [JsonPropertyName("Id")]
    public MongoId? Id
    {
        get;
        set;
    }

    /// <summary>
    /// name of preset given by player
    /// </summary>
    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("Root")]
    public MongoId? Root
    {
        get;
        set;
    }

    [JsonPropertyName("Items")]
    public List<Item>? Items
    {
        get;
        set;
    }
}
