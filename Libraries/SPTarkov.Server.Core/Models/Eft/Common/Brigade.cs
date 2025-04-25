using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Brigade
{
    [JsonPropertyName("ID")]
    public double? Id
    {
        get;
        set;
    }

    [JsonPropertyName("ArtilleryGuns")]
    public List<ArtilleryGun>? ArtilleryGuns
    {
        get;
        set;
    }
}
