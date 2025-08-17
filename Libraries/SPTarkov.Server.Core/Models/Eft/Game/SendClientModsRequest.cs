using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record SendClientModsRequest : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("activeClientMods")]
    public List<ProfileActiveClientMods> ActiveClientMods { get; set; } = [];
}
