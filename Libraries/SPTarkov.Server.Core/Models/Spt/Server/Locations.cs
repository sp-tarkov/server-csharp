using System.Collections.Frozen;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record Locations
{
    // sometimes we get the key or value given so save changing logic in each place
    // have it key both
    private readonly FrozenDictionary<string, string> _locationMappings = new Dictionary<
        string,
        string
    >
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
    public Eft.Common.Location? Bigmap { get; set; }

    [JsonPropertyName("develop")]
    public Eft.Common.Location? Develop { get; set; }

    [JsonPropertyName("factory4_day")]
    public Eft.Common.Location? Factory4Day { get; set; }

    [JsonPropertyName("factory4_night")]
    public Eft.Common.Location? Factory4Night { get; set; }

    [JsonPropertyName("hideout")]
    public Eft.Common.Location? Hideout { get; set; }

    [JsonPropertyName("interchange")]
    public Eft.Common.Location? Interchange { get; set; }

    [JsonPropertyName("laboratory")]
    public Eft.Common.Location? Laboratory { get; set; }

    [JsonPropertyName("lighthouse")]
    public Eft.Common.Location? Lighthouse { get; set; }

    [JsonPropertyName("privatearea")]
    public Eft.Common.Location? PrivateArea { get; set; }

    [JsonPropertyName("rezervbase")]
    public Eft.Common.Location? RezervBase { get; set; }

    [JsonPropertyName("shoreline")]
    public Eft.Common.Location? Shoreline { get; set; }

    [JsonPropertyName("suburbs")]
    public Eft.Common.Location? Suburbs { get; set; }

    [JsonPropertyName("tarkovstreets")]
    public Eft.Common.Location? TarkovStreets { get; set; }

    [JsonPropertyName("labyrinth")]
    public Eft.Common.Location? Labyrinth { get; set; }

    [JsonPropertyName("terminal")]
    public Eft.Common.Location? Terminal { get; set; }

    [JsonPropertyName("town")]
    public Eft.Common.Location? Town { get; set; }

    [JsonPropertyName("woods")]
    public Eft.Common.Location? Woods { get; set; }

    [JsonPropertyName("sandbox")]
    public Eft.Common.Location? Sandbox { get; set; }

    [JsonPropertyName("sandbox_high")]
    public Eft.Common.Location? SandboxHigh { get; set; }

    /// <summary>
    ///     Holds a mapping of the linkages between locations on the UI
    /// </summary>
    [JsonPropertyName("base")]
    public LocationsBase? Base { get; set; }

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
        var classProps = typeof(Locations)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(Eft.Common.Location) && p.Name != "Item");
        _locationDictionaryCache = classProps.ToDictionary(
            propertyInfo => propertyInfo.Name,
            propertyInfo => propertyInfo.GetValue(this, null) as Eft.Common.Location,
            StringComparer.OrdinalIgnoreCase
        );
    }
}
