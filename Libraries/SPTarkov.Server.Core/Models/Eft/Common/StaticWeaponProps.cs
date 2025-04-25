using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

[Obsolete("use SpawnpointTemplate")]
public record StaticWeaponProps : StaticPropsBase
{
    [JsonPropertyName("Items")]
    public Item[] Items
    {
        get;
        set;
    }
}
