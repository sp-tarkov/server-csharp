using System.Collections.Frozen;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record Locations
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    // sometimes we get the key or value given so save changing logic in each place
    // have it key both
    private readonly FrozenDictionary<string, string> _locationMappings = new Dictionary<string, string>
    {
        // EFT
        { "factory4_day", "Factory4Day" },
        { "bigmap", "Bigmap" },
        { "develop", "Develop" },
        { "factory4_night", "Factory4Night" },
        { "hideout", "Hideout" },
        { "interchange", "Interchange" },
        { "laboratory", "Laboratory" },
        { "lighthouse", "Lighthouse" },
        { "privatearea", "PrivateArea" },
        { "rezervbase", "RezervBase" },
        { "shoreline", "Shoreline" },
        { "suburbs", "Suburbs" },
        { "tarkovstreets", "TarkovStreets" },
        { "labyrinth", "Labyrinth" },
        { "terminal", "Terminal" },
        { "town", "Town" },
        { "woods", "Woods" },
        { "sandbox", "Sandbox" },
        { "sandbox_high", "SandboxHigh" },
        // SPT
        { "Factory4Day", "Factory4Day" },
        { "Bigmap", "Bigmap" },
        { "Develop", "Develop" },
        { "Factory4Night", "Factory4Night" },
        { "Hideout", "Hideout" },
        { "Interchange", "Interchange" },
        { "Laboratory", "Laboratory" },
        { "Lighthouse", "Lighthouse" },
        { "PrivateArea", "PrivateArea" },
        { "RezervBase", "RezervBase" },
        { "Shoreline", "Shoreline" },
        { "Suburbs", "Suburbs" },
        { "TarkovStreets", "TarkovStreets" },
        { "Terminal", "Terminal" },
        { "Town", "Town" },
        { "Woods", "Woods" },
        { "Labyrinth", "Labyrinth" },
        { "Sandbox", "Sandbox" },
        { "SandboxHigh", "SandboxHigh" },
    }.ToFrozenDictionary();

    private Dictionary<string, Eft.Common.Location>? _locationDictionaryCache;

    [JsonPropertyName("bigmap")]
    public required Eft.Common.Location Bigmap { get; init; }

    [JsonPropertyName("develop")]
    public required Eft.Common.Location? Develop { get; init; }

    [JsonPropertyName("factory4_day")]
    public required Eft.Common.Location Factory4Day { get; init; }

    [JsonPropertyName("factory4_night")]
    public required Eft.Common.Location Factory4Night { get; init; }

    [JsonPropertyName("hideout")]
    public required Eft.Common.Location? Hideout { get; init; }

    [JsonPropertyName("interchange")]
    public required Eft.Common.Location Interchange { get; init; }

    [JsonPropertyName("laboratory")]
    public required Eft.Common.Location Laboratory { get; init; }

    [JsonPropertyName("lighthouse")]
    public required Eft.Common.Location Lighthouse { get; init; }

    [JsonPropertyName("privatearea")]
    public required Eft.Common.Location? PrivateArea { get; init; }

    [JsonPropertyName("rezervbase")]
    public required Eft.Common.Location RezervBase { get; init; }

    [JsonPropertyName("shoreline")]
    public required Eft.Common.Location Shoreline { get; init; }

    [JsonPropertyName("suburbs")]
    public required Eft.Common.Location? Suburbs { get; init; }

    [JsonPropertyName("tarkovstreets")]
    public required Eft.Common.Location TarkovStreets { get; init; }

    [JsonPropertyName("labyrinth")]
    public required Eft.Common.Location Labyrinth { get; init; }

    [JsonPropertyName("terminal")]
    public required Eft.Common.Location? Terminal { get; init; }

    [JsonPropertyName("town")]
    public required Eft.Common.Location? Town { get; init; }

    [JsonPropertyName("woods")]
    public required Eft.Common.Location Woods { get; init; }

    [JsonPropertyName("sandbox")]
    public required Eft.Common.Location Sandbox { get; init; }

    [JsonPropertyName("sandbox_high")]
    public required Eft.Common.Location SandboxHigh { get; init; }

    /// <summary>
    ///     Holds a mapping of the linkages between locations on the UI
    /// </summary>
    [JsonPropertyName("base")]
    public required LocationsBase Base { get; init; }

    /// <summary>
    ///     Get map locations as a dictionary, keyed by its name e.g. Factory4Day
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Eft.Common.Location> GetDictionary()
    {
        if (_locationDictionaryCache is null)
        {
            HydrateDictionary();
        }

        return _locationDictionaryCache;
    }

    /// <summary>
    ///     Convert any type of key to Locations actual Property name.
    ///     "factory4_day" or "Factory4Day" returns "Factory4Day"
    /// </summary>
    /// <returns></returns>
    public string GetMappedKey(string key)
    {
        return _locationMappings.GetValueOrDefault(key, key);
    }

    private void HydrateDictionary()
    {
        var classProps = typeof(Locations).GetProperties().Where(p => p.PropertyType == typeof(Eft.Common.Location) && p.Name != "Item");
        _locationDictionaryCache = classProps.ToDictionary(
            propertyInfo => propertyInfo.Name,
            propertyInfo => propertyInfo.GetValue(this, null) as Eft.Common.Location,
            StringComparer.OrdinalIgnoreCase
        );
    }
}
