using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Class
{
    // Checked in client
    [JsonPropertyName("resistance")]
    public int? Resistance
    {
        get;
        set;
    }
}
