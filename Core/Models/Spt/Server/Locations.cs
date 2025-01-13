using System.Reflection;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Common;

namespace Core.Models.Spt.Server;

public class Locations
{
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

    /** Holds a mapping of the linkages between locations on the UI */
    [JsonPropertyName("base")]
    public LocationsBase? Base { get; set; }

    public Eft.Common.Location? this[string key]
    {
        get
        {
            return (Eft.Common.Location?)GetType()
                .GetProperties()
                .First(p => p.Name.ToLower() == key.ToLower()).GetGetMethod()?
                .Invoke(this, null) ?? null;
        }
        set
        {
            GetType()
                .GetProperties()
                .First(p => p.Name.ToLower() == key.ToLower()).GetSetMethod()?
                .Invoke(this, [value]);
        }
    }

    private Dictionary<string, Eft.Common.Location>? _locationDictionaryCache;

    /// <summary>
    /// Get map locations as a dictionary, keyed by its name e.g. factory4_day
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Eft.Common.Location> GetDictionary()
    {
        if (_locationDictionaryCache is null)
        {
            var classProps = typeof(Locations).GetProperties().Where(p => p.PropertyType == typeof(Eft.Common.Location) && p.Name != "Item");
            _locationDictionaryCache = classProps
                .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(this, null) as Eft.Common.Location);
        }

        return _locationDictionaryCache;
    }
}
