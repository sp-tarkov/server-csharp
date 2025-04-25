using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArtilleryGun
{
    [JsonPropertyName("Position")]
    public XYZ? Position
    {
        get;
        set;
    }
}
