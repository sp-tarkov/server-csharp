using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ProfileSides
{
    [JsonPropertyName("descriptionLocaleKey")]
    public string? DescriptionLocaleKey
    {
        get;
        set;
    }

    [JsonPropertyName("usec")]
    public TemplateSide? Usec
    {
        get;
        set;
    }

    [JsonPropertyName("bear")]
    public TemplateSide? Bear
    {
        get;
        set;
    }
}
