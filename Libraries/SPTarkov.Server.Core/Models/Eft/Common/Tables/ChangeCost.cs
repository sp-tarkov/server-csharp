using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ChangeCost
{
    /// <summary>
    ///     What item it will take to reset daily
    /// </summary>
    [JsonPropertyName("templateId")]
    public string? TemplateId
    {
        get;
        set;
    }

    /// <summary>
    ///     Amount of item needed to reset
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }
}
