using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record TournamentCategories
{
    [JsonPropertyName("dogtags")]
    public bool? Dogtags
    {
        get;
        set;
    }
}
