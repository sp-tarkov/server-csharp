using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ItemGlobalRestrictions
{
    [JsonPropertyName("MaxFlea")]
    public double? MaxFlea
    {
        get;
        set;
    }

    [JsonPropertyName("MaxFleaStacked")]
    public double? MaxFleaStacked
    {
        get;
        set;
    }

    [JsonPropertyName("TemplateId")]
    public string? TemplateId
    {
        get;
        set;
    }
}
